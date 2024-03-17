using RandoMapMod.Modes;
using UnityEngine;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class RoomSelectionText : ControlPanelText
    {
        private protected override string Name => "Room Selection";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.RoomSelectionOn ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            return $"{"Toggle room selection".L()} (Ctrl-R): {(RandoMapMod.GS.RoomSelectionOn ? "On" : "Off").L()}";
        }
    }
}
