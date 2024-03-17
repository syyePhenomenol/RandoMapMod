using UnityEngine;
using RandoMapMod.Localization;

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
            return $"Ctrl-H: {(RandoMapMod.GS.ControlPanelOn ? "Hide hotkeys" : "More hotkeys").L()}";
        }
    }
}
