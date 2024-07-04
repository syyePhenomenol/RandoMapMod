using System.Collections.ObjectModel;
using ItemChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using SD = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins
{
    internal abstract class AbstractPlacementsPin : RmmPin, IPeriodicUpdater
    {
        private protected abstract PoolsCollection PoolsCollection { get; }

        private protected List<AbstractPlacement> placements = [];
        internal ReadOnlyCollection<AbstractPlacement> Placements => placements.AsReadOnly();

        private protected Dictionary<AbstractPlacement, AbstractPlacementPinDef> placementDefs = [];

        private protected List<AbstractPlacement> activePlacements = [];
        private protected AbstractPlacement CurrentPlacement => activePlacements.FirstOrDefault();
        private protected AbstractPlacementPinDef CurrentPlacementDef => placementDefs[CurrentPlacement];
        private protected AbstractPlacementState CurrentPlacementState => CurrentPlacementDef.State;

        internal string[] HighlightScenes { get; private set; }
        internal ISelectable[] HighlightRooms { get; private set; }

        private readonly HashSet<string> locationPoolGroups = [];
        internal override IReadOnlyCollection<string> LocationPoolGroups => locationPoolGroups;

        private readonly HashSet<string> itemPoolGroups = [];
        internal override IReadOnlyCollection<string> ItemPoolGroups => itemPoolGroups;

        internal override LogicDef Logic => CurrentPlacementDef.Logic;
        internal override string HintText => CurrentPlacementDef.HintDef.Text;

        internal virtual void Initialize(AbstractPlacement placement)
        {
            Initialize();

            AbstractPlacementPinDef appd = new(placement);
            ModSource = SD.Of(placement).Get(InteropProperties.ModSource);
            SceneName = appd.SceneName;

            HighlightScenes = SD.Of(placement).Get(InteropProperties.HighlightScenes);

            if (HighlightScenes is not null)
            {
                HashSet<ISelectable> highlightRooms = [];

                foreach (string scene in HighlightScenes)
                {
                    if (!TransitionRoomSelector.Instance.Objects.TryGetValue(scene, out List<ISelectable> rooms)) continue;

                    foreach (ISelectable room in rooms)
                    {
                        highlightRooms.Add(room);
                    }
                }

                HighlightRooms = [.. highlightRooms];
            }

            AddPlacement(appd);

            // This has default behaviour when the CoordinateLocation exists and no other properties are provided
            if (!MapChanger.Finder.TryGetLocation(placement.Name, out MapLocationDef _)
                && !SD.Of(placement).IsNonDefault(InteropProperties.MapLocations)
                && !SD.Of(placement).IsNonDefault(InteropProperties.AbsMapLocation)
                && !SD.Of(placement).IsNonDefault(InteropProperties.PinGridIndex)
                && SD.Of(placement).Get(InteropProperties.WorldMapLocations) is (string, float, float)[] worldMapLocations)
            {
                WorldMapPosition wmp = new(worldMapLocations);
                MapPosition = wmp;
                MapZone = wmp.MapZone;
            }
            // This doesn't have a default handler and will always fall through when the property is not provided
            else if (SD.Of(placement).Get(InteropProperties.AbsMapLocation) is (float, float) absMapLocation)
            {
                MapPosition = new AbsMapPosition(absMapLocation);
            }
            // This has a default handler and might not fall through when the property is not provided
            else if (SD.Of(placement).Get(InteropProperties.MapLocations) is (string, float, float)[] mapLocations)
            {
                MapRoomPosition mlp = new(mapLocations);
                MapPosition = mlp;
                MapZone = mlp.MapZone;
            }
            // This has a default handler
            else
            {
                PinGridIndex = SD.Of(placement).Get(InteropProperties.PinGridIndex);
                RmmPinManager.GridPins.Add(this);
            }

            textBuilders.InsertRange(textBuilders.IndexOf(GetLogicText), new Func<string>[]
                {
                    GetPreviewText,
                    GetObtainedText,
                    GetPersistentText,
                    GetUnobtainedText
                }
            );
        }

        internal void AddPlacement(AbstractPlacementPinDef appd)
        {
            placements.Add(appd.Placement);
            placementDefs.Add(appd.Placement, appd);

            locationPoolGroups.Add(appd.LocationPoolGroup);
            itemPoolGroups.UnionWith(appd.ItemPoolGroups);
        }

        private protected override void UpdateHintText()
        {
            foreach (var placementDef in placementDefs.Values)
            {
                placementDef.HintDef.UpdateHintText();
            }
        }

        private protected override bool ActiveBySettings()
        {
            activePlacements.Clear();

            if (RandoMapMod.LS.GroupBy is GroupBySetting.Item)
            {
                foreach (var placement in placements)
                {
                    if (placementDefs[placement].ItemPoolGroups.Any(p => RandoMapMod.LS.IsActivePoolGroup(p, PoolsCollection)))
                    {
                        activePlacements.Add(placement);
                    }
                }
            }
            else
            {
                foreach (var placement in placements)
                {
                    if (RandoMapMod.LS.IsActivePoolGroup(placementDefs[placement].LocationPoolGroup, PoolsCollection))
                    {
                        activePlacements.Add(placement);
                    }
                }
            }

            // Sort by Previewable > NotCleared > Cleared (persistent) > Cleared
            activePlacements.OrderBy(p => placementDefs[p].State).ThenByDescending(p => p.GetObtainablePersistentItems().Count());

            return activePlacements.Any();
        }
        
        private protected IEnumerable<AbstractItem> activeItems;
        private protected bool showPersistentItems;

        private protected override bool ActiveByProgress()
        {
            activeItems = null;
            showPersistentItems = false;

            // The first placement determines what set of sprites to show
            switch (CurrentPlacementState)
            {
                case AbstractPlacementState.Previewable:
                    activeItems = activePlacements.GetPreviewableItems();
                    break;
                case AbstractPlacementState.NotCleared:
                    if (RandoMapMod.LS.SpoilerOn)
                    {
                        activeItems = activePlacements.GetNeverObtainedItems();
                    }
                    break;
                case AbstractPlacementState.Cleared:
                    if (RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.Persistent
                        && activePlacements.GetObtainablePersistentItems() is var persistentItems && persistentItems.Any())
                    {
                        activeItems = persistentItems;
                        showPersistentItems = true;
                        break;
                    }

                    if (RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.All)
                    {
                        activeItems = activePlacements.GetEverObtainedItems();
                        break;
                    }

                    return false;
                default:
                    break;
            }

            return true;
        }

        private protected override void UpdatePinSprites()
        {
            if (activeItems is not null)
            {
                CycleSprites = activeItems.Select(PinSpriteManager.GetSprite).GroupBy(s => s.Value).Select(g => g.First());
            }
            else
            {
                CycleSprites = new ScaledPinSprite[] { PinSpriteManager.GetLocationSprite(CurrentPlacement) };
            }
        }

        private protected override PinShape GetMixedPinShape()
        {
            if (CurrentPlacementState is AbstractPlacementState.Previewable) return PinShape.Diamond;

            if (CurrentPlacementState is AbstractPlacementState.Cleared && showPersistentItems) return PinShape.Hexagon;

            return base.GetMixedPinShape();
        }

        private protected override void UpdateBorderColor()
        {
            if (showPersistentItems)
            {
                BorderColor = RmmColors.GetColor(RmmColorSetting.Pin_Persistent);
                return;
            }

            BorderColor = CurrentPlacementState switch
            {
                AbstractPlacementState.Previewable => RmmColors.GetColor(RmmColorSetting.Pin_Previewed),
                AbstractPlacementState.Cleared => RmmColors.GetColor(RmmColorSetting.Pin_Cleared),
                _ => RmmColors.GetColor(RmmColorSetting.Pin_Normal),
            };
        }

        private protected override string GetNameText()
        {
            if (placements.Count() > 1)
            {
                return base.GetNameText() + $" {"and".L()} {placements.Count() - 1} {"other locations...".L()}";
            }

            return base.GetNameText();
        }

        private protected override string GetRoomText()
        {
            if (HighlightScenes is not null)
            {
                string text = $"\n\n{"Rooms".L()}:";

                foreach (string scene in HighlightScenes)
                {
                    text += $" {scene.LC()},";
                }

                return text.Substring(0, text.Length - 1);
            }

            return $"\n\n{"Room".L()}: {(CurrentPlacementDef.SceneName ?? "Unknown").LC()}";
        }

        private protected override string GetStatusText()
        {
            string text = $"\n\n{"Status".L()}: ";

            text += PoolsCollection switch
            {
                PoolsCollection.Randomized => "Randomized".L(),
                PoolsCollection.Vanilla => "Vanilla".L(),
                _ => "Unknown".L()
            };

            text += CurrentPlacementState switch
            {
                AbstractPlacementState.Previewable => $", {"previewed".L()}",
                AbstractPlacementState.NotCleared => $", {"unchecked".L()}",
                AbstractPlacementState.Cleared => $", {"cleared".L()}",
                _ => ""
            };

            return text;
        }

        private protected virtual string GetPreviewText()
        {
            string text = "";

            if (placements.SelectMany(p => p.GetPreviewText()) is var previewTexts && previewTexts.Any())
            {
                text += $"\n\n{"Previewed item(s)".L()}";

                if (placements.Count() > 1)
                {
                    text += $" {"at all overlapping locations".L()}";
                }

                text += ":";

                foreach (string previewText in previewTexts)
                {
                    text += $" {previewText},";
                }

                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        private protected virtual string GetObtainedText()
        {
            string text = "";

            if (placements.GetEverObtainedItems() is var obtainedItems && obtainedItems.Any())
            {
                text += $"\n\n{"Obtained item(s)".L()}";

                if (placements.Count() > 1)
                {
                    text += $" {"at all overlapping locations".L()}";
                }

                text += $": {ToStringList(obtainedItems)}";
            }

            return text;
        }

        private protected virtual string GetPersistentText()
        {
            string text = "";

            if (showPersistentItems)
            {
                text += $"\n\n{"Available persistent item(s)".L()}";

                if (placements.Count() > 1)
                {
                    text += $" {"at all overlapping locations".L()}";
                }

                text += $": {ToStringList(activeItems)}";
            }

            return text;
        }

        private protected virtual string GetUnobtainedText()
        {
            string text = "";

            if (!RandoMapMod.LS.SpoilerOn) return text;

            if (placements.GetNeverObtainedUnpreviewableItems() is var unobtainedItems && unobtainedItems.Any())
            {
                text += $"\n\n{"Spoiler item(s)".L()}";

                if (placements.Count() > 1)
                {
                    text += $" {"at all overlapping locations".L()}";
                }

                text += $": {ToStringList(unobtainedItems)}";
            }

            return text;
        }

        private string ToStringList(IEnumerable<AbstractItem> items)
        {
            string text = "";

            foreach (AbstractItem item in items)
            {
                text += $" {item.GetPreviewName().LC()},";
            }

            return text.Substring(0, text.Length - 1);
        }
    }
}