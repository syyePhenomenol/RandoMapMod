using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ShowReticleText : ControlPanelText
    {
        private protected override string Name => "Show Reticle";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.ShowReticle ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("Show reticles")} (Ctrl-S): ";
            return text + (RandoMapMod.GS.ShowReticle ? "On" : "Off");
        }
    }
}
