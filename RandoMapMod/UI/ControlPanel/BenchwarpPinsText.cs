using RandoMapMod.Modes;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class BenchwarpPinsText : ControlPanelText
    {
        private protected override string Name => "Benchwarp Pins";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.ItemRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            if (Interop.HasBenchwarp())
            {
                return RandoMapMod.GS.ShowBenchwarpPins ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            if (Interop.HasBenchwarp())
            {
                string text = $"{L.Localize("Benchwarp pins")} (Ctrl-W): ";
                return text + (RandoMapMod.GS.ShowBenchwarpPins ? "On" : "Off");
            }

            return "Benchwarp is not installed or outdated";
        }
    }
}
