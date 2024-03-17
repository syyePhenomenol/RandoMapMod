using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PoolButton : ExtraButton
    {
        internal string PoolGroup { get; init; }

        public PoolButton(string poolGroup) : base(poolGroup, RandoMapMod.MOD)
        {
            PoolGroup = poolGroup;
        }

        protected override void OnClick()
        {
            RandoMapMod.LS.TogglePoolGroupSetting(PoolGroup);
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = $"Toggle {PoolGroup} on/off.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            Button.Content = L.Localize(PoolGroup).Replace(" ", "\n");

            Button.ContentColor = RandoMapMod.LS.GetPoolGroupSetting(PoolGroup) switch
            {
                PoolState.On => RmmColors.GetColor(RmmColorSetting.UI_On),
                PoolState.Off => RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                PoolState.Mixed => RmmColors.GetColor(RmmColorSetting.UI_Custom),
                _ => RmmColors.GetColor(RmmColorSetting.UI_On)
            };
        }
    }
}
