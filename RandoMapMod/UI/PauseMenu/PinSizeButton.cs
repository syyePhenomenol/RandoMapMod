using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinSizeButton : MainButton
    {
        internal static PinSizeButton Instance { get; private set; }

        internal PinSizeButton() : base("Pin Size", RandoMapMod.MOD, 1, 2)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.TogglePinSize();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Pin Size")}:\n";

            switch (RandoMapMod.GS.PinSize)
            {
                case PinSize.Small:
                    text += L.Localize("small");
                    break;

                case PinSize.Medium:
                    text += L.Localize("medium");
                    break;

                case PinSize.Large:
                    text += L.Localize("large");
                    break;
            }

            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            Button.Content = text;
        }
    }
}
