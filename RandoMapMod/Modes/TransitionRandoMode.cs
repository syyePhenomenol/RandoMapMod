using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Transition;
using UnityEngine;

namespace RandoMapMod.Modes
{
    internal abstract class TransitionRandoMode : RmmMapMode
    {
        public override bool InitializeToThis()
        {
            if (!TransitionData.IsTransitionRando()) return false;

            if (RandoMapMod.GS.OverrideDefaultMode)
            {
                return ModeName == RandoMapMod.GS.TransitionRandoModeOverride.ToString().ToCleanName();
            }
            else
            {
                return ModeName == Settings.RmmMode.Transition_Normal.ToString().ToCleanName();
            }
        }

        public override bool DisableAreaNames => true;
        public override bool DisableNextArea => true;

        public override bool? RoomActiveOverride(RoomSprite roomSprite)
        {
            return TransitionTracker.GetRoomActive(roomSprite.Rsd.SceneName);
        }

        public override Vector4? RoomColorOverride(RoomSprite roomSprite)
        {
            return roomSprite.Selected ? RmmColors.GetColor(RmmColorSetting.Room_Selected) : TransitionTracker.GetRoomColor(roomSprite.Rsd.SceneName);
        }

        public override Vector4? QuickMapTitleColorOverride(QuickMapTitle qmt)
        {
            return RmmColors.GetColor(ColorSetting.UI_Neutral);
        }
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
