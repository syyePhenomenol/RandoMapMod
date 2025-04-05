﻿using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class ShiftPanText : ControlPanelText
{
    private protected override string Name => "Shift Pan";

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
        return $"{"Hold".L()} Shift: {"Pan faster".L()}";
    }
}
