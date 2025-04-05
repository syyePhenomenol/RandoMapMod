using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class SpoilersButton() : MainButton(nameof(SpoilersButton), nameof(RandoMapMod), 0, 3)
{
    protected override void OnClick()
    {
        RandoMapMod.LS.ToggleSpoilers();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Reveals the items at each location.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        this.SetButtonBoolToggle($"{"Spoilers".L()}:\n", RandoMapMod.LS.SpoilerOn);
    }
}
