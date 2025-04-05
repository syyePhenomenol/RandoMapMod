using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class RouteCompassButton() : BorderlessExtraButton(nameof(RouteCompassButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleRouteCompassEnabled();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Point compass to next transition in the pathfinder route.".L();
    }

    public override void Update()
    {
        this.SetButtonBoolToggle($"{"Route compass".L()}:\n", RandoMapMod.GS.ShowRouteCompass);
    }
}
