using RandoMapMod.Input;
using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

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
        return $"{ControlPanelInput.Instance.GetBindingsText()}: {(RandoMapMod.GS.ControlPanelOn ? "Hide hotkeys" : "More hotkeys").L()}";
    }
}
