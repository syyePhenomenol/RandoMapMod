using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class NextAreasButton() : BorderlessExtraButton(nameof(NextAreasButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleNextAreas();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Show next area indicators (text/arrow) on the quick map.".L();
    }

    public override void Update()
    {
        var text = $"{"Show next\nareas".L()}: ";

        switch (RandoMapMod.GS.ShowNextAreas)
        {
            case NextAreaSetting.Off:
                text += "Off".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case NextAreaSetting.Arrows:
                text += "Arrows".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;

            case NextAreaSetting.Full:
                text += "Full".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;
            default:
                break;
        }

        Button.Content = text;
    }
}
