using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class MiscPanelButton : MainButton
    {
        public MiscPanelButton() : base("Misc Panel Button", RandoMapMod.MOD, 2, 0)
        {

        }

        protected override void OnClick()
        {
            MiscPanel.Instance.Toggle();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (MiscPanel.Instance.ExtraButtonsGrid.Visibility == MagicUI.Core.Visibility.Visible)
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
