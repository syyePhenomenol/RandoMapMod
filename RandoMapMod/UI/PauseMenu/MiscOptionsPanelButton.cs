using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class MiscOptionsPanelButton() : MainButton(nameof(MiscOptionsPanelButton), RandoMapMod.MOD, 2, 2)
    {
        protected override void OnClick()
        {
            MiscOptionsPanel.Instance.Toggle();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Some miscenalleous options.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (MiscOptionsPanel.Instance.ExtraButtonsGrid.Visibility == MagicUI.Core.Visibility.Visible)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = "Misc.\nOptions".L();
        }
    }
}
