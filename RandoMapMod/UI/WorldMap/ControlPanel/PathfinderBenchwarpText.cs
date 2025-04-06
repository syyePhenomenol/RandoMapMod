using RandoMapMod.Input;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using UnityEngine;

namespace RandoMapMod.UI;

internal class PathfinderBenchwarpText : ControlPanelText
{
    private protected override string Name => "Pathfinder Benchwarp";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
    }

    private protected override Vector4 GetColor()
    {
        if (Interop.HasBenchwarp)
        {
            return RandoMapMod.GS.PathfinderBenchwarp
                ? RmmColors.GetColor(RmmColorSetting.UI_On)
                : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        if (Interop.HasBenchwarp)
        {
            return $"{"Pathfinder benchwarp".L()} {PathfinderBenchwarpInput.Instance.GetBindingsText()}: {(RandoMapMod.GS.PathfinderBenchwarp ? "On" : "Off").L()}";
        }

        return "Benchwarp is not installed or outdated".L();
    }
}
