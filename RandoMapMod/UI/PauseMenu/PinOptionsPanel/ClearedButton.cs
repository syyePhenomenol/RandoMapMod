using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class ClearedButton() : BorderlessExtraButton(nameof(ClearedButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleShowClearedPins();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Show pins for persistent or all cleared locations.".L();
    }

    public override void Update()
    {
        var text = $"{"Show cleared".L()}:\n";

        switch (RandoMapMod.GS.ShowClearedPins)
        {
            case ClearedPinsSetting.Off:
                text += "off".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case ClearedPinsSetting.Persistent:
                text += "persistent".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;

            case ClearedPinsSetting.All:
                text += "all".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;
            default:
                break;
        }

        Button.Content = text;
    }
}
