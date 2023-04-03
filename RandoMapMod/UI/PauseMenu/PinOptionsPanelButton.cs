using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinOptionsPanelButton : MainButton
    {
        internal static PinOptionsPanelButton Instance { get; private set; }

        public PinOptionsPanelButton() : base(nameof(PinOptionsPanelButton), RandoMapMod.MOD, 2, 0)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            PinOptionsPanel.Instance.Toggle();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "More options for pin behavior.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
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