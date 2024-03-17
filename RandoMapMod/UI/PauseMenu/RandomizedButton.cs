using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class RandomizedButton : MainButton
    {
        internal RandomizedButton() : base(nameof(RandomizedButton), RandoMapMod.MOD, 0, 1)
        {

        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleRandomized();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Toggle pins for randomized locations on/off.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{"Randomized".L()}:\n";

            if (RandoMapMod.LS.RandomizedOn)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += "on".L();
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += "off".L();
            }

            if (RandoMapMod.LS.IsRandomizedCustom())
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
                text += "custom".L();
            }

            Button.Content = text;
        }
    }
}
