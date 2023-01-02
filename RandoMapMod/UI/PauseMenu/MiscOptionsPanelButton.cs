using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class MiscOptionsPanelButton : MainButton
    {
        public MiscOptionsPanelButton() : base(nameof(MiscOptionsPanelButton), RandoMapMod.MOD, 2, 1)
        {

        }

        protected override void OnClick()
        {
            MiscOptionsPanel.Instance.Toggle();
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

            Button.Content = $"{L.Localize("Misc.")}\n{L.Localize("Options")}";
        }
    }
}
