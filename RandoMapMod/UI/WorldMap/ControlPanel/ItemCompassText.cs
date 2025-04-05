using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class ItemCompassText : ControlPanelText
{
    private protected override string Name => "Item Compass";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn;
    }

    private protected override Vector4 GetColor()
    {
        return RandoMapMod.GS.ItemCompassOn
            ? RmmColors.GetColor(RmmColorSetting.UI_On)
            : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        return $"{"Toggle item compass".L()} (Ctrl-C): {(RandoMapMod.GS.ItemCompassOn ? "On" : "Off").L()}";
    }
}
