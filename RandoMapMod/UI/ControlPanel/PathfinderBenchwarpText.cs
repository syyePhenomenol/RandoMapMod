using RandoMapMod.Modes;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PathfinderBenchwarpText : ControlPanelText
    {
        private protected override string Name => "Pathfinder Benchwarp";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            if (Interop.HasBenchwarp())
            {
                return RandoMapMod.GS.PathfinderBenchwarp ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }
            
            return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            if (Interop.HasBenchwarp())
            {
                string text = $"{L.Localize("Pathfinder benchwarp")} (Ctrl-B): ";
                return text + (RandoMapMod.GS.PathfinderBenchwarp ? "On" : "Off");
            }

            return "Benchwarp is not installed or outdated";
        }
    }
}
