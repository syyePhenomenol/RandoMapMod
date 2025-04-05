using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class ItemCompassModeButton() : BorderlessExtraButton(nameof(ItemCompassModeButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleItemCompassMode();
        ItemCompass.Info.UpdateCompassTargets();
        OnHover();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText =
            "Item compass will point to: ".L()
            + RandoMapMod.GS.ItemCompassMode switch
            {
                ItemCompassMode.Reachable => "Reachable items".L(),
                ItemCompassMode.ReachableOutOfLogic => "Reachable items including sequence break".L(),
                ItemCompassMode.All => "All items".L(),
                _ => "",
            };
    }

    public override void Update()
    {
        Button.Content =
            $"{"Item compass\nmode".L()}: "
            + RandoMapMod.GS.ItemCompassMode switch
            {
                ItemCompassMode.Reachable => "Reachable".L(),
                ItemCompassMode.ReachableOutOfLogic => "OoL".L(),
                ItemCompassMode.All => "All".L(),
                _ => "",
            };

        Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }
}
