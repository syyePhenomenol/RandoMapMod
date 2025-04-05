using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class ModEnabledText : ControlPanelText
{
    private protected override string Name => "Mod Enabled";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn;
    }

    private protected override Vector4 GetColor()
    {
        return RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        return $"Ctrl-M: {"Disable mod".L()}";
    }
}
