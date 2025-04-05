using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class DefaultSettingsButton() : BorderlessExtraButton(nameof(DefaultSettingsButton))
{
    protected override void OnClick()
    {
        RandoMapMod.ResetToDefaultSettings();
        MapUILayerUpdater.Update();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Resets all global settings of RandoMapMod.".L();
    }

    public override void Update()
    {
        Button.Content = "Reset global\nsettings".L();

        Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
    }
}
