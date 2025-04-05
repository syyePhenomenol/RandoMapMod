using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class PathfinderOptionsPanelButton()
    : MainButton(nameof(PathfinderOptionsPanelButton), nameof(RandoMapMod), 2, 1)
{
    protected override void OnClick()
    {
        PathfinderOptionsPanel.Instance.Toggle();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Pathfinder options.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        base.Update();

        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        if (PathfinderOptionsPanel.Instance.Grid.Visibility == MagicUI.Core.Visibility.Visible)
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        }

        Button.Content = "Pathfinder\nOptions".L();
    }
}
