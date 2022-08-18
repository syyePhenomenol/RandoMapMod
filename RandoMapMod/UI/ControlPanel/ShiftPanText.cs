using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ShiftPanText : ControlPanelText
    {
        private protected override string Name => "Shift Pan";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            return $"{L.Localize("Hold Shift")}: {L.Localize("Pan faster")}";
        }
    }
}
