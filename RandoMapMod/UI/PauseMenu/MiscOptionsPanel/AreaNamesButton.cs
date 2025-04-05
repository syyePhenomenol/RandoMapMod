using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class AreaNamesButton() : BorderlessExtraButton(nameof(AreaNamesButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleAreaNames();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Show area names on the world/quick map.";
    }

    public override void Update()
    {
        this.SetButtonBoolToggle($"{"Show area\nnames".L()}: ", RandoMapMod.GS.ShowAreaNames);
    }
}
