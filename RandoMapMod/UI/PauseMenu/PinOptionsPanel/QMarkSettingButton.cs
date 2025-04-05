using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class QMarkSettingButton() : BorderlessExtraButton(nameof(QMarkSettingButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleQMarkSetting();
        ItemCompass.Info.UpdateCurrentCompassTargets();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Toggle question mark sprites on/off.".L();
    }

    public override void Update()
    {
        var text = $"{"Question\nmarks".L()}: ";

        switch (RandoMapMod.GS.QMarks)
        {
            case QMarkSetting.Off:
                text += "off".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case QMarkSetting.Red:
                text += "red".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;

            case QMarkSetting.Mix:
                text += "mixed".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;
            default:
                break;
        }

        Button.Content = text;
    }
}
