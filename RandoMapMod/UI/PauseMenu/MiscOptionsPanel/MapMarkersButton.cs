using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class MapMarkersButton() : BorderlessExtraButton(nameof(MapMarkersButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleMapMarkers();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Enable vanilla map marker behavior.".L();
    }

    public override void Update()
    {
        this.SetButtonBoolToggle($"{"Show map\nmarkers".L()}: ", RandoMapMod.GS.ShowMapMarkers);
    }
}
