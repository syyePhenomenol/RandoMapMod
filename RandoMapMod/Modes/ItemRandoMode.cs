using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Transition;
using UnityEngine;

namespace RandoMapMod.Modes
{
    internal class ItemRandoMode : RmmMapMode
    {
        public override bool InitializeToThis()
        {
            if (TransitionData.IsTransitionRando()) return false;

            if (RandoMapMod.GS.OverrideDefaultMode)
            {
                return ModeName == RandoMapMod.GS.ItemRandoModeOverride.ToString().ToCleanName();
            }
            else
            {
                return ModeName == Settings.RmmMode.Full_Map.ToString().ToCleanName();
            }
        }

        public override Vector4? RoomColorOverride(RoomSprite roomSprite)
        {
            if (roomSprite.Selected)
            {
                return RmmColors.GetColor(RmmColorSetting.Room_Highlighted);
            }

            return GetCustomColor(roomSprite.Rsd.ColorSetting);
        }

        public override Vector4? AreaNameColorOverride(AreaName areaName) { return GetCustomColor(areaName.MiscObjectDef.ColorSetting); }

        public override Vector4? NextAreaColorOverride(NextArea nextArea) { return GetCustomColor(nextArea.MiscObjectDef.ColorSetting); }


        private Vector4? GetCustomColor(ColorSetting colorSetting)
        {
            Vector4 customColor = RmmColors.GetColor(colorSetting);

            if (!customColor.Equals(Vector4.negativeInfinity))
            {
                return customColor.ToOpaque();
            }

            return null;
        }

        public override Vector4? QuickMapTitleColorOverride(QuickMapTitle qmt) 
        {
            Vector4 customColor = RmmColors.GetColorFromMapZone(Finder.GetCurrentMapZone());

            if (!customColor.Equals(Vector4.negativeInfinity))
            {
                return customColor.ToOpaque();
            }

            return null;
        }
    }

    internal class FullMapMode : ItemRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.Full_Map.ToString().ToCleanName();
    }

    internal class AllPinsMode : ItemRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.All_Pins.ToString().ToCleanName();
        public override bool FullMap => false;
    }

    internal class PinsOverMapMode : ItemRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.Pins_Over_Map.ToString().ToCleanName();
        public override bool FullMap => false;
    }
}
