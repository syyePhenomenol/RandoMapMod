using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class PinOptionsPanelButton() : MainButton(nameof(PinOptionsPanelButton), nameof(RandoMapMod), 2, 0)
{
    protected override void OnClick()
    {
        PinOptionsPanel.Instance.Toggle();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "More options for pin behavior.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        if (PinOptionsPanel.Instance.Grid.Visibility == MagicUI.Core.Visibility.Visible)
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        Button.Content = "More Pin\nOptions".L();
    }
}
