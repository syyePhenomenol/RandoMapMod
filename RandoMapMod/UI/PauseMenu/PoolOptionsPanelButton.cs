using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class PoolOptionsPanelButton() : MainButton(nameof(PoolOptionsPanelButton), nameof(RandoMapMod), 1, 3)
{
    protected override void OnClick()
    {
        PoolOptionsPanel.Instance.Toggle();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Customize which item/location pools to display.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        if (PoolOptionsPanel.Instance.Grid.Visibility == MagicUI.Core.Visibility.Visible)
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        Button.Content = "Customize\nPools".L();
    }
}
