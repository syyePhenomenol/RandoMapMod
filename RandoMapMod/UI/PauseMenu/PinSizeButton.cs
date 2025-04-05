using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class PinSizeButton() : MainButton(nameof(PinSizeButton), nameof(RandoMapMod), 1, 2)
{
    protected override void OnClick()
    {
        RandoMapMod.GS.TogglePinSize();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Toggle overall size of pins.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        var text = $"{"Pin Size".L()}:\n";

        switch (RandoMapMod.GS.PinSize)
        {
            case PinSize.Tiny:
                text += "tiny".L();
                break;

            case PinSize.Small:
                text += "small".L();
                break;

            case PinSize.Medium:
                text += "medium".L();
                break;

            case PinSize.Large:
                text += "large".L();
                break;

            case PinSize.Huge:
                text += "huge".L();
                break;
            default:
                break;
        }

        Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        Button.Content = text;
    }
}
