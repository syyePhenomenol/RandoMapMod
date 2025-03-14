﻿using ConnectionMetadataInjector.Util;
using MapChanger;
using MapChanger.Defs;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore;
using RandomizerCore.Logic;
using UnityEngine;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pins
{
    internal sealed class VanillaPin : RmmPin, ILogicPin
    {
        internal static Vector4 VanillaColor => new(SHRINK_COLOR_MULTIPLIER, SHRINK_COLOR_MULTIPLIER, SHRINK_COLOR_MULTIPLIER, 1f);

        private string locationPoolGroup;

        // private readonly List<string> items = new();
        private readonly Dictionary<string, string> itemPoolGroups = [];

        internal override IReadOnlyCollection<string> ItemPoolGroups => new HashSet<string> (itemPoolGroups.Values);
        internal override IReadOnlyCollection<string> LocationPoolGroups => [locationPoolGroup];

        private LogicDef logic;
        public LogicDef Logic => logic;

        private HintDef hintDef;
        public HintDef HintDef => hintDef;

        internal void Initialize(GeneralizedPlacement placement)
        {
            Initialize();

            SceneName = RandomizerMod.RandomizerData.Data.GetLocationDef(name)?.SceneName ?? ItemChanger.Finder.GetLocation(name)?.sceneName;

            locationPoolGroup =  SubcategoryFinder.GetLocationPoolGroup(placement.Location.Name).FriendlyName();

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

            if (RM.RS.TrackerData.lm.LogicLookup.TryGetValue(placement.Location.Name, out LogicDef ld))
            {
                logic = ld;
            }
            else
            {
                RandoMapMod.Instance.LogWarn($"No well-defined logic for vanilla placement {placement.Location.Name}");
            }

            hintDef = new(InteropProperties.GetDefaultLocationHints(placement.Location.Name));

            textBuilders.InsertRange(textBuilders.IndexOf(GetLockText),
                [
                    this.GetLogicText,
                    this.GetHintText
                ]
            );
        }

        // internal void AddPlacement(GeneralizedPlacement gp)
        // {
        //     if (gp.Location.Name != name)
        //     {
        //         RandoMapMod.Instance.LogWarn($"Trying to add GeneralizedPlacement {gp.Location.Name} to {name}");
        //         return;
        //     }

        //     items.Add(gp.Item.Name);
        //     itemPoolGroups.Add(gp.Item.Name, SubcategoryFinder.GetItemPoolGroup(gp.Item.Name).FriendlyName());
        // }

        public void UpdateLogic()
        {
            hintDef.UpdateHintText();
        }

        private protected override bool ActiveBySettings()
        {
            return RandoMapMod.LS.IsActivePoolGroup(locationPoolGroup, PoolsCollection.Vanilla);
        }

        private protected override bool ActiveByProgress()
        {
            return !Tracker.HasClearedLocation(name) || RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.All;
        }

        private protected override void UpdatePinSprites()
        {
            CycleSprites = [Psm.GetLocationSprite(locationPoolGroup)];
        }

        private protected override void UpdatePinSize()
        {
            base.UpdatePinSize();

            if (!Selected)
            {
                Size *= SHRINK_SIZE_MULTIPLIER;
            }
        }

        private protected override void UpdatePinColor()
        {
            Color = VanillaColor;
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

            BorderColor = new(color.x * SHRINK_COLOR_MULTIPLIER, color.y * SHRINK_COLOR_MULTIPLIER, color.z * SHRINK_COLOR_MULTIPLIER, color.w);
        }

        private protected override string GetStatusText()
        {
            string text = $"\n\n{"Status".L()}: {"Not randomized".L()}, ";

            if (Tracker.HasClearedLocation(name))
            {
                text += "cleared".L();
            }
            else
            {
                if (IsPersistent())
                {
                    text += "persistent".L();
                }
                else
                {
                    text += "unchecked".L();
                }
            }

            return text; 
        }

        private bool IsPersistent()
        {
            return locationPoolGroup == PoolGroup.LifebloodCocoons.FriendlyName()
                || locationPoolGroup == PoolGroup.SoulTotems.FriendlyName()
                || locationPoolGroup == PoolGroup.LoreTablets.FriendlyName();
        }
    }
}
