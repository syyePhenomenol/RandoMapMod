using MapChanger.Input;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using UnityEngine;

namespace RandoMapMod.UI;

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
        return $"{"Mode".L()} {ToggleModeInput.Instance.GetBindingsText()}: {MapChanger.Settings.CurrentMode().ModeName.L()}";
    }
}
