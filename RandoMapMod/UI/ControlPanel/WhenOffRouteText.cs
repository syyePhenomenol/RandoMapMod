using RandoMapMod.Modes;
using RandoMapMod.Settings;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class WhenOffRouteText : ControlPanelText
    {
        private protected override string Name => "When Off Route";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.WhenOffRoute switch
            {
                OffRouteBehaviour.Reevaluate => RmmColors.GetColor(RmmColorSetting.UI_On),
                _ => RmmColors.GetColor(RmmColorSetting.UI_Neutral),
            };
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("When off-route")} (Ctrl-E): ";

            return RandoMapMod.GS.WhenOffRoute switch 
            {
                OffRouteBehaviour.Keep => text + L.Localize("Keep route"),
                OffRouteBehaviour.Cancel => text + L.Localize("Cancel route"),
                OffRouteBehaviour.Reevaluate => text + L.Localize("Reevaluate route"),
                _ => text
            };
        }
    }
}
