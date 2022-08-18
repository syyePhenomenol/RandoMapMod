using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class MapKeyText : ControlPanelText
    {
        private protected override string Name => "Map Key";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.MapKeyOn ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("Toggle map key")} (Ctrl-K): ";
            return text + (RandoMapMod.GS.MapKeyOn ? "On" : "Off");
        }
    }
}
