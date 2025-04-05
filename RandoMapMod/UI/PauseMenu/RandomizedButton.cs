using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class RandomizedButton() : MainButton(nameof(RandomizedButton), nameof(RandoMapMod), 0, 1)
{
    protected override void OnClick()
    {
        RandoMapMod.LS.ToggleRandomized();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Toggle pins for randomized locations on/off.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        var text = $"{"Randomized".L()}:\n";

        if (RandoMapMod.LS.RandomizedOn)
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "On".L();
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "Off".L();
        }

        if (RandoMapMod.LS.IsRandomizedCustom())
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            text += $" ({"Custom".L()})";
        }

        Button.Content = text;
    }
}
