using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PoolOptionsPanelButton : MainButton
    {
        internal static PoolOptionsPanelButton Instance { get; private set; }

        public PoolOptionsPanelButton() : base(nameof(PoolOptionsPanelButton), RandoMapMod.MOD, 1, 3)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            PoolOptionsPanel.Instance.Toggle();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Customize which item/location pools to display.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (PoolOptionsPanel.Instance.ExtraButtonsGrid.Visibility == MagicUI.Core.Visibility.Visible)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = $"{L.Localize("Customize")}\n{L.Localize("Pools")}";
        }
    }
}
