using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinOptionsPanelButton : MainButton
    {
        public PinOptionsPanelButton() : base(nameof(PinOptionsPanelButton), RandoMapMod.MOD, 2, 0)
        {

        }

        protected override void OnClick()
        {
            PinOptionsPanel.Instance.Toggle();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (PinOptionsPanel.Instance.ExtraButtonsGrid.Visibility == MagicUI.Core.Visibility.Visible)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = $"{L.Localize("More Pin\nOptions")}";
        }
    }
}