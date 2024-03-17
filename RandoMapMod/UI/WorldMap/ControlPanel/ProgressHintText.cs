using RandoMapMod.Settings;
using UnityEngine;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class ProgressHintText : ControlPanelText
    {
        private protected override string Name => "Progress Hint";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn;
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.ProgressHint is not ProgressHintSetting.Off ? RmmColors.GetColor(RmmColorSetting.UI_On) : RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        private protected override string GetText()
        {
            return $"{"Toggle progress hint".L()} (Ctrl-G): " + RandoMapMod.GS.ProgressHint switch
            {
                ProgressHintSetting.Area => "Area".L(),
                ProgressHintSetting.Room => "Room".L(),
                ProgressHintSetting.Location => "Location".L(),
                _ => "Off".L()
            };
        }
    }
}
