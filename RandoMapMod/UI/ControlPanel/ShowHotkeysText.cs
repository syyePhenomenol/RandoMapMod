using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ShowHotkeysText : ControlPanelText
    {
        private protected override string Name => "Show Hotkeys";

        private protected override bool ActiveCondition()
        {
            return true;
        }

        private protected override Vector4 GetColor()
        {
            return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            if (RandoMapMod.GS.ControlPanelOn)
            {
                return $"Ctrl-H: {L.Localize("Hide hotkeys")}";
            }
            else
            {
                return $"Ctrl-H: {L.Localize("More hotkeys")}";
            }
        }
    }
}
