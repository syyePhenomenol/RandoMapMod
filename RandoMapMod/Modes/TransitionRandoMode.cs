using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Pathfinder;
using UnityEngine;

namespace RandoMapMod.Modes
{
    internal abstract class TransitionRandoMode : RmmMapMode
    {
        public override bool DisableAreaNames => true;

        public override bool? RoomActiveOverride(RoomSprite roomSprite)
        {
            return RmmPathfinder.Slt.GetRoomActive(roomSprite.Rsd.SceneName);
        }

        public override Vector4? RoomColorOverride(RoomSprite roomSprite)
        {
            return roomSprite.Selected ? RmmColors.GetColor(RmmColorSetting.Room_Selected) : RmmPathfinder.Slt.GetRoomColor(roomSprite.Rsd.SceneName);
        }

        public override Vector4? QuickMapTitleColorOverride(QuickMapTitle qmt)
        {
            return RmmColors.GetColor(ColorSetting.UI_Neutral);
        }

        public override bool? NextAreaNameActiveOverride(NextAreaName nextAreaName) => false;
        public override bool? NextAreaArrowActiveOverride(NextAreaArrow nextAreaArrow) => false;
    }

    internal class TransitionNormalMode : TransitionRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.Transition_Normal.ToString().ToCleanName();
    }

    internal class TransitionVisitedOnlyMode : TransitionRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.Transition_Visited_Only.ToString().ToCleanName();
    }

    internal class TransitionAllRoomsMode : TransitionRandoMode
    {
        public override string Mod => RandoMapMod.MOD;
        public override string ModeName => Settings.RmmMode.Transition_All_Rooms.ToString().ToCleanName();
    }
}
