using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class RouteTextButton() : BorderlessExtraButton(nameof(RouteTextButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleRouteTextInGame();
        MapUILayerUpdater.Update();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "How the route text is displayed during gameplay.".L();
    }

    public override void Update()
    {
        var text = $"{"Route text".L()}:\n";

        switch (RandoMapMod.GS.RouteTextInGame)
        {
            case Settings.RouteTextInGame.Hide:
                text += "hide".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                break;

            case Settings.RouteTextInGame.NextTransitionOnly:
                text += "next transition".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;

            case Settings.RouteTextInGame.Show:
                text += "all transitions".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                break;
            default:
                break;
        }

        Button.Content = text;
    }
}
