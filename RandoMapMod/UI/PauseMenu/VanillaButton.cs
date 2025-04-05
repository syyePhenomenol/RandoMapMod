using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class VanillaButton() : MainButton(nameof(VanillaButton), nameof(RandoMapMod), 0, 2)
{
    protected override void OnClick()
    {
        RandoMapMod.LS.ToggleVanilla();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Toggle pins for vanilla locations on/off.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        var text = $"{"Vanilla".L()}:\n";

        if (RandoMapMod.LS.VanillaOn)
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "On".L();
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "Off".L();
        }

        if (RandoMapMod.LS.IsVanillaCustom())
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            text += $" ({"Custom".L()})";
        }

        Button.Content = text;
    }
}
