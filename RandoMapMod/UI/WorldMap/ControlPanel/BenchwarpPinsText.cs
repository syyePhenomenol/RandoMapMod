using RandoMapMod.Input;
using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class BenchwarpPinsText : ControlPanelText
{
    private protected override string Name => "Benchwarp Pins";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn;
    }

    private protected override Vector4 GetColor()
    {
        if (Interop.HasBenchwarp)
        {
            return RandoMapMod.GS.ShowBenchwarpPins
                ? RmmColors.GetColor(RmmColorSetting.UI_On)
                : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        if (Interop.HasBenchwarp)
        {
            return $"{"Benchwarp pins".L()} {ToggleBenchwarpPinsInput.Instance.GetBindingsText()}: {(RandoMapMod.GS.ShowBenchwarpPins ? "On" : "Off").L()}";
        }

        return "Benchwarp is not installed or outdated".L();
    }
}
