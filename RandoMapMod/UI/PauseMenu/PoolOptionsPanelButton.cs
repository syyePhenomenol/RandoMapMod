using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PoolOptionsPanelButton : MainButton
    {
        public PoolOptionsPanelButton() : base(nameof(PoolOptionsPanelButton), RandoMapMod.MOD, 1, 3)
        {

        }

        protected override void OnClick()
        {
            PoolOptionsPanel.Instance.Toggle();
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
