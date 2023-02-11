using ConnectionMetadataInjector.Util;
using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using RandomizerCore;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Pins
{
    internal sealed class VanillaRmmPin : RmmPin
    {
        private static readonly Vector4 vanillaColor = new(UNREACHABLE_COLOR_MULTIPLIER, UNREACHABLE_COLOR_MULTIPLIER, UNREACHABLE_COLOR_MULTIPLIER, 1f);

        internal override HashSet<string> ItemPoolGroups => new() { LocationPoolGroup };

        private ISprite locationSprite;

        internal void Initialize(GeneralizedPlacement placement)
        {
            Initialize();

            SceneName = RandomizerMod.RandomizerData.Data.GetLocationDef(name)?.SceneName ?? ItemChanger.Finder.GetLocation(name)?.sceneName;

            LocationPoolGroup = SubcategoryFinder.GetLocationPoolGroup(placement.Location.Name).FriendlyName();
            locationSprite = new PinLocationSprite(LocationPoolGroup);

            if (InteropProperties.GetDefaultMapLocations(name) is (string, float, float)[] mapLocations)
            {
                MapRoomPosition mlp = new(mapLocations);
                MapPosition = mlp;
                MapZone = mlp.MapZone;
            }
            else
            {
                RmmPinManager.GridPins.Add(this);
            }
        }

        private protected override bool ActiveBySettings()
        {
            Settings.PoolState poolState = RandoMapMod.LS.GetPoolGroupSetting(LocationPoolGroup);

            return poolState == Settings.PoolState.On || (poolState == Settings.PoolState.Mixed && RandoMapMod.LS.VanillaOn);
        }

        protected private override bool ActiveByProgress()
        {
            return !Tracker.HasClearedLocation(name) || RandoMapMod.GS.ShowClearedPins;
        }

        private protected override void UpdatePinSprite()
        {
            Sprite = locationSprite.Value;
        }

        private protected override void UpdatePinSize()
        {
            Size = pinSizes[RandoMapMod.GS.PinSize];

            if (Selected)
            {
                Size *= SELECTED_MULTIPLIER;
            }
            else
            {
                Size *= UNREACHABLE_SIZE_MULTIPLIER;
            }
        }

        private protected override void UpdatePinColor()
        {
            Color = vanillaColor;
        }

        private protected override void UpdateBorderColor()
        {
            Vector4 color;

            if (Tracker.HasClearedLocation(name))
            {
                color = RmmColors.GetColor(RmmColorSetting.Pin_Cleared);
            }
            else if (IsPersistent())
            {
                color = RmmColors.GetColor(RmmColorSetting.Pin_Persistent);
            }
            else
            {
                color = RmmColors.GetColor(RmmColorSetting.Pin_Normal);
            }

            BorderColor = new(color.x * UNREACHABLE_COLOR_MULTIPLIER, color.y * UNREACHABLE_COLOR_MULTIPLIER, color.z * UNREACHABLE_COLOR_MULTIPLIER, color.w);
        }

        internal override string GetSelectionText()
        {
            string text = base.GetSelectionText();

            text += $"\n\n{L.Localize("Status")}:";

            if (Tracker.HasClearedLocation(name))
            {
                text += $" {L.Localize("Not randomized, cleared")}";
            }
            else
            {
                if (IsPersistent())
                {
                    text += $" {L.Localize("Not randomized, persistent")}";
                }
                else
                {
                    text += $" {L.Localize("Not randomized, unchecked")}";
                }
            }

            text += $"\n\n{L.Localize("Logic")}: {Logic?.InfixSource ?? "not found"}";

            return text; 
        }

        private bool IsPersistent()
        {
            return LocationPoolGroup == PoolGroup.LifebloodCocoons.FriendlyName()
                || LocationPoolGroup == PoolGroup.SoulTotems.FriendlyName()
                || LocationPoolGroup == PoolGroup.LoreTablets.FriendlyName();
        }
    }
}
