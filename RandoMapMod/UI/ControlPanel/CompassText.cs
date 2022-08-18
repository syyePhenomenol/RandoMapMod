using RandoMapMod.Modes;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class CompassText : ControlPanelText
    {
        private protected override string Name => "Compass";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.ShowRouteCompass ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("Show route compass")} (Ctrl-C): ";
            return text + (RandoMapMod.GS.ShowRouteCompass ? "On" : "Off");
        }
    }
}
