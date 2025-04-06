using RandoMapMod.Input;
using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class PinSelectionText : ControlPanelText
{
    private protected override string Name => "Pin Selection";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn;
    }

    private protected override Vector4 GetColor()
    {
        return RandoMapMod.GS.PinSelectionOn
            ? RmmColors.GetColor(RmmColorSetting.UI_On)
            : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        return $"{"Toggle pin selection".L()} {PinPanelInput.Instance.GetBindingsText()}: {(RandoMapMod.GS.PinSelectionOn ? "On" : "Off").L()}";
    }
}
