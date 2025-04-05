using MagicUI.Core;
using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class ModEnabledButton() : MainButton(nameof(ModEnabledButton), nameof(RandoMapMod), 0, 0)
{
    protected override void OnClick()
    {
        MapChanger.Settings.ToggleModEnabled();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Toggle all map mod behavior on/off.".L();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        if (MapChanger.Settings.CurrentMode().Mod is nameof(RandoMapMod))
        {
            Button.Visibility = Visibility.Visible;
        }
        else
        {
            Button.Visibility = Visibility.Hidden;
        }

        if (MapChanger.Settings.MapModEnabled())
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            Button.Content = $"{"Map Mod".L()}\n{"Enabled".L()}";
        }
        else
        {
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Disabled);
            Button.Content = $"{"Map Mod".L()}\n{"Disabled".L()}";
        }
    }
}
