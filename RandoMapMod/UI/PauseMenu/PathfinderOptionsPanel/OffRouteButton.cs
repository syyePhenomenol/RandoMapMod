using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class OffRouteButton() : BorderlessExtraButton(nameof(OffRouteButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleWhenOffRoute();
        MapUILayerUpdater.Update();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "When going off route, how the pathfinder route is updated.".L();
    }

    public override void Update()
    {
        var text = $"{"Off route".L()}:\n";

        switch (RandoMapMod.GS.WhenOffRoute)
        {
            case Settings.OffRouteBehaviour.Cancel:
                text += "cancel route".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case Settings.OffRouteBehaviour.Keep:
                text += "keep route".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case Settings.OffRouteBehaviour.Reevaluate:
                text += "reevaluate".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;
            default:
                break;
        }

        Button.Content = text;
    }
}
