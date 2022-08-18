using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConnectionMetadataInjector;
using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Defs;
using RandoMapMod.Rooms;
using UnityEngine;
using L = RandomizerMod.Localization;
using RM = RandomizerMod.RandomizerMod;
using SD = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins
{
    internal sealed class RandomizedRmmPin : RmmPin, IPeriodicUpdater
    {
        private AbstractPlacement placement;
        private RandoPlacementState placementState;

        private IEnumerable<AbstractItem> remainingItems;
        private int itemIndex = 0;

        private Dictionary<AbstractItem, string> itemPoolGroups;
        internal override HashSet<string> ItemPoolGroups => new(itemPoolGroups.Values);

        private ISprite locationSprite;
        private float locationSpriteScale;
        private Dictionary<AbstractItem, (ISprite, float)> itemSprites;

        private bool showItemSprite = false;

        internal string[] HighlightScenes { get; private set; }
        internal HashSet<ISelectable> HighlightRooms { get; private set; }

        public float UpdateWaitSeconds { get; } = 1f;

        private Coroutine periodicUpdate;
        public IEnumerator PeriodicUpdate()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(UpdateWaitSeconds);
                itemIndex = (itemIndex + 1) % remainingItems.Count();
                UpdatePinSprite();
            }
        }

        private void StartPeriodicUpdate()
        {
            if (periodicUpdate is null)
            {
                periodicUpdate = StartCoroutine(PeriodicUpdate());
            }
        }

        private void StopPeriodicUpdate()
        {
            if (periodicUpdate is not null)
            {
                StopCoroutine(periodicUpdate);
                periodicUpdate = null;
            }
        }

        internal void Initialize(AbstractPlacement placement)
        {
            Initialize();

            this.placement = placement;

            SceneName = placement.RandoModLocation()?.LocationDef?.SceneName ?? ItemChanger.Finder.GetLocation(name)?.sceneName;

            ModSource = SD.OfPlacementAndLocations(placement).Get(InteropProperties.ModSource);

            LocationPoolGroup = SD.OfPlacementAndLocations(placement).Get(InjectedProps.LocationPoolGroup);
            locationSprite = SD.OfPlacementAndLocations(placement).Get(InteropProperties.LocationPinSprite);
            locationSpriteScale = GetPinSpriteScale(locationSprite, SD.OfPlacementAndLocations(placement).Get(InteropProperties.LocationPinSpriteSize));

            itemPoolGroups = new();
            itemSprites = new();
            foreach (AbstractItem item in placement.Items)
            {
                itemPoolGroups[item] = SD.Of(item).Get(InjectedProps.ItemPoolGroup);
                ISprite sprite = SD.Of(item).Get(InteropProperties.ItemPinSprite);
                itemSprites[item] = (sprite, GetPinSpriteScale(sprite, SD.Of(item).Get(InteropProperties.ItemPinSpriteSize)));
            }

            HighlightScenes = SD.OfPlacementAndLocations(placement).Get(InteropProperties.HighlightScenes);

            if (HighlightScenes is not null)
            {
                HighlightRooms = new();

                foreach (string scene in HighlightScenes)
                {
                    if (!TransitionRoomSelector.Instance.Objects.TryGetValue(scene, out List<ISelectable> rooms)) continue;

                    foreach (ISelectable room in rooms)
                    {
                        HighlightRooms.Add(room);
                    }
                }
            }

            if (SD.OfPlacementAndLocations(placement).Get(InteropProperties.MapLocations) is (string, float, float)[] mapLocations)
            {
                MapRoomPosition mlp = new(mapLocations);
                MapPosition = mlp;
                MapZone = mlp.MapZone;
            }
            else if (SD.OfPlacementAndLocations(placement).Get(InteropProperties.WorldMapLocations) is (string, float, float)[] worldMapLocations)
            {
                WorldMapPosition wmp = new(worldMapLocations);
                MapPosition = wmp;
                MapZone = wmp.MapZone;
            }
            else if (SD.OfPlacementAndLocations(placement).Get(InteropProperties.AbsMapLocation) is (float, float) absMapLocation)
            {
                MapPosition = new AbsMapPosition(absMapLocation);
            }
            else
            {
                PinGridIndex = SD.OfPlacementAndLocations(placement).Get(InteropProperties.PinGridIndex);
                RmmPinManager.GridPins.Add(this);
            }
        }

        private float GetPinSpriteScale(ISprite sprite, (int, int)? interopSize)
        {
            if (interopSize is (int width, int height))
            {
                return SpriteManager.DEFAULT_PIN_SPRITE_SIZE / ((width + height) / 2f);
            }
            else
            {
                if (sprite is PinLocationSprite or PinItemSprite || sprite.Value is null)
                {
                    return 1f;
                }
                else
                {
                    return SpriteManager.DEFAULT_PIN_SPRITE_SIZE / ((sprite.Value.rect.width + sprite.Value.rect.height) / 2f);
                }
            }
        }

        protected private override bool ActiveBySettings()
        {
            if (RandoMapMod.LS.GroupBy == Settings.GroupBySetting.Item)
            {
                foreach (string poolGroup in remainingItems.Select(item => itemPoolGroups[item]))
                {
                    Settings.PoolState poolState = RandoMapMod.LS.GetPoolGroupSetting(poolGroup);

                    if (poolState == Settings.PoolState.On || (poolState == Settings.PoolState.Mixed && RandoMapMod.LS.RandomizedOn))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Settings.PoolState poolState = RandoMapMod.LS.GetPoolGroupSetting(LocationPoolGroup);

                return poolState == Settings.PoolState.On || (poolState == Settings.PoolState.Mixed && RandoMapMod.LS.RandomizedOn);
            }
        }

        protected private override bool ActiveByProgress()
        {
            return (placementState != RandoPlacementState.Cleared)
                && (placementState != RandoPlacementState.ClearedPersistent || RandoMapMod.GS.ShowPersistentPins);
        }

        public override void OnMainUpdate(bool active)
        {
            itemIndex = 0;

            if (RandoMapMod.GS.ShowPersistentPins)
            {
                remainingItems = placement.Items.Where(item => !item.WasEverObtained() || item.IsPersistent());
            }
            else
            {
                remainingItems = placement.Items.Where(item => !item.WasEverObtained());
            }

            showItemSprite = RandoMapMod.LS.SpoilerOn
                || (placementState is RandoPlacementState.Previewed && placement.CanPreview())
                || placementState is RandoPlacementState.ClearedPersistent;

            StopPeriodicUpdate();

            if (showItemSprite && active)
            {
                StartPeriodicUpdate();
            }

            base.OnMainUpdate(active);
        }

        protected private override void UpdatePinSprite()
        {
            if (showItemSprite)
            {
                if (itemSprites.TryGetValue(remainingItems.ElementAt(itemIndex), out (ISprite itemSprite, float scale) spriteInfo))
                {
                    Sprite = spriteInfo.itemSprite.Value;
                    Sr.transform.localScale = new Vector3(spriteInfo.scale, spriteInfo.scale, 1f);
                }
            }
            else
            {
                Sprite = locationSprite.Value;
                Sr.transform.localScale = new Vector3(locationSpriteScale, locationSpriteScale, 1f);
            }
        }

        protected private override void UpdatePinSize()
        {
            float size = pinSizes[RandoMapMod.GS.PinSize];

            if (Selected)
            {
                size *= SELECTED_MULTIPLIER;
            }
            else if (placementState is RandoPlacementState.UncheckedUnreachable or RandoPlacementState.ClearedPersistent)
            {
                size *= UNREACHABLE_SIZE_MULTIPLIER;
            }

            Size = size;
        }

        protected private override void UpdatePinColor()
        {
            Vector4 color = UnityEngine.Color.white;

            if (placementState == RandoPlacementState.UncheckedUnreachable)
            {
                Color = new Vector4(color.x * UNREACHABLE_COLOR_MULTIPLIER, color.y * UNREACHABLE_COLOR_MULTIPLIER, color.z * UNREACHABLE_COLOR_MULTIPLIER, color.w);
                return;
            }

            Color = color;
        }

        protected private override void UpdateBorderColor()
        {
            Vector4 color = placementState switch
            {
                RandoPlacementState.OutOfLogicReachable => RmmColors.GetColor(RmmColorSetting.Pin_Out_of_logic),
                RandoPlacementState.Previewed => RmmColors.GetColor(RmmColorSetting.Pin_Previewed),
                RandoPlacementState.Cleared => RmmColors.GetColor(RmmColorSetting.Pin_Cleared),
                RandoPlacementState.ClearedPersistent => RmmColors.GetColor(RmmColorSetting.Pin_Persistent),
                _ => RmmColors.GetColor(RmmColorSetting.Pin_Normal),
            };

            if (placementState == RandoPlacementState.UncheckedUnreachable)
            {
                BorderColor = new Vector4(color.x * UNREACHABLE_COLOR_MULTIPLIER, color.y * UNREACHABLE_COLOR_MULTIPLIER, color.z * UNREACHABLE_COLOR_MULTIPLIER, color.w);
            }
            else
            {
                BorderColor = color;
            }
        }

        internal void UpdatePlacementState()
        {
            if (RM.RS.TrackerData.clearedLocations.Contains(name))
            {
                if (placement.IsPersistent())
                {
                    placementState = RandoPlacementState.ClearedPersistent;
                }
                else
                {
                    placementState = RandoPlacementState.Cleared;
                }
            }
            // Does not guarantee the item sprites should show (for a cost-only or a "none" preview)
            else if (RM.RS.TrackerData.previewedLocations.Contains(name))
            {
                placementState = RandoPlacementState.Previewed;
            }
            else if (RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableLocations.Contains(name))
            {
                placementState = RandoPlacementState.UncheckedReachable;
            }
            else if (RM.RS.TrackerData.uncheckedReachableLocations.Contains(name))
            {
                placementState = RandoPlacementState.OutOfLogicReachable;
            }
            else
            {
                placementState = RandoPlacementState.UncheckedUnreachable;
            }
        }

        internal override string GetSelectionText()
        {
            string text = base.GetSelectionText();

            if (HighlightRooms is not null)
            {
                text += $"\n\n{L.Localize("Rooms")}:";

                foreach (string scene in HighlightScenes)
                {
                    text += $" {scene},";
                }

                text = text.Substring(0, text.Length - 1);
            }

            text += $"\n\n{L.Localize("Status")}:";

            text += placementState switch
            {
                RandoPlacementState.UncheckedUnreachable => $" {L.Localize("Randomized, unchecked, unreachable")}",
                RandoPlacementState.UncheckedReachable => $" {L.Localize("Randomized, unchecked, reachable")}",
                RandoPlacementState.OutOfLogicReachable => $" {L.Localize("Randomized, unchecked, reachable through sequence break")}",
                RandoPlacementState.Previewed => $" {L.Localize("Randomized, previewed")}",
                RandoPlacementState.Cleared => $" {L.Localize("Cleared")}",
                RandoPlacementState.ClearedPersistent => $" {L.Localize("Randomized, cleared, persistent")}",
                _ => ""
            };

            text += $"\n\n{L.Localize("Logic")}: {Logic?? "not found"}";

            if (placementState is RandoPlacementState.Previewed && placement.TryGetPreviewText(out List<string> previewText))
            {
                text += $"\n\n{L.Localize("Previewed item(s)")}:";

                foreach (string preview in previewText)
                {
                    text += $" {ToCleanPreviewText(preview)},";
                }

                text = text.Substring(0, text.Length - 1);
            }

            IEnumerable<AbstractItem> obtainedItems = placement.Items.Where(item => item.WasEverObtained());

            if (obtainedItems.Any())
            {
                text += $"\n\n{L.Localize("Obtained item(s)")}:";

                foreach (AbstractItem item in obtainedItems)
                {
                    text += $" {item.GetPreviewName()},";
                }

                text = text.Substring(0, text.Length - 1);
            }

            IEnumerable<AbstractItem> spoilerItems = placement.Items.Where(item => !item.WasEverObtained());

            if (spoilerItems.Any() && RandoMapMod.LS.SpoilerOn
                && !(placementState is RandoPlacementState.Previewed && placement.CanPreview()))
            {
                text += $"\n\n{L.Localize("Spoiler item(s)")}:";

                foreach (AbstractItem item in spoilerItems)
                {
                    text += $" {item.GetPreviewName()},";
                }

                text = text.Substring(0, text.Length - 1);
            }

            return text;

            static string ToCleanPreviewText(string text)
            {
                return text.Replace("Pay ", "")
                    .Replace("Once you own ", "")
                    .Replace(", I'll gladly sell it to you.", "")
                    .Replace("Requires ", "");
            }
        }
    }
}