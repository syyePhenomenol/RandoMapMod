using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class PoolButton(string poolGroup) : ExtraButton(poolGroup, nameof(RandoMapMod))
{
    internal string PoolGroup { get; init; } = poolGroup;

    protected override void OnClick()
    {
        RandoMapMod.LS.TogglePoolGroupSetting(PoolGroup);
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = $"{"Toggle".L()} {PoolGroup.L()} {"on/off".L()}.";
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        Button.Content = PoolGroup.L().Replace(" ", "\n");

        Button.ContentColor = RandoMapMod.LS.GetPoolGroupSetting(PoolGroup) switch
        {
            PoolState.On => RmmColors.GetColor(RmmColorSetting.UI_On),
            PoolState.Off => RmmColors.GetColor(RmmColorSetting.UI_Neutral),
            PoolState.Mixed => RmmColors.GetColor(RmmColorSetting.UI_Custom),
            _ => RmmColors.GetColor(RmmColorSetting.UI_On),
        };
    }
}
