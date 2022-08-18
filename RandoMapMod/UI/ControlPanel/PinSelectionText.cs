using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinSelectionText : ControlPanelText
    {
        private protected override string Name => "Pin Selection";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.PinSelectionOn ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("Toggle pin selection")} (Ctrl-P): ";
            return text + (RandoMapMod.GS.PinSelectionOn ? "On" : "Off");
        }
    }
}
