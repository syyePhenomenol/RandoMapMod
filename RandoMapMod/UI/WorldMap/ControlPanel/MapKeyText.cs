using RandoMapMod.Input;
using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.UI;

internal class MapKeyText : ControlPanelText
{
    private protected override string Name => "Map Key";

    private protected override bool ActiveCondition()
    {
        return RandoMapMod.GS.ControlPanelOn;
    }

    private protected override Vector4 GetColor()
    {
        return RandoMapMod.GS.MapKeyOn
            ? RmmColors.GetColor(RmmColorSetting.UI_On)
            : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }

    private protected override string GetText()
    {
        return $"{"Toggle map key".L()} {MapKeyInput.Instance.GetBindingsText()}: {(RandoMapMod.GS.MapKeyOn ? "On" : "Off").L()}";
    }
}
