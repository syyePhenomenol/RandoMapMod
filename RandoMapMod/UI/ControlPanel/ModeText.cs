using RandoMapMod.Modes;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ModeText : ControlPanelText
    {
        private protected override string Name => "Mode";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            if (MapChanger.Settings.CurrentMode() is FullMapMode)
            {
                return RmmColors.GetColor(RmmColorSetting.UI_On);
            }
            else if (Conditions.TransitionRandoModeEnabled())
            {
                return RmmColors.GetColor(RmmColorSetting.UI_Special);
            }
            else
            {
                return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }
        }

        private protected override string GetText()
        {
            return $"{L.Localize("Mode")} (Ctrl-T): {MapChanger.Settings.CurrentMode().ModeName}";
        }
    }
}
