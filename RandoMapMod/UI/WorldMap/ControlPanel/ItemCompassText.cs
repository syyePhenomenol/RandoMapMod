using RandoMapMod.Settings;
using UnityEngine;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class ItemCompassText : ControlPanelText
    {
        private protected override string Name => "Item Compass";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.ShowItemCompass is not ItemCompassSetting.Off ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            return $"{"Toggle item compass".L()} (Ctrl-C): " + RandoMapMod.GS.ShowItemCompass switch
            {
                ItemCompassSetting.Reachable => "Reachable".L(),
                ItemCompassSetting.ReachableOutOfLogic => "Reachable sequence break".L(),
                ItemCompassSetting.All => "All items".L(),
                _ => "Off".L()
            };
        }
    }
}
