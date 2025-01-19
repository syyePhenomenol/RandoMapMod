using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class ClearedButton() : ExtraButton(nameof(ClearedButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleShowClearedPins();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Show pins for persistent or all cleared locations.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Show cleared".L()}:\n";

            switch (RandoMapMod.GS.ShowClearedPins)
            {
                case ClearedPinsSetting.Off:
                    text += "off".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case ClearedPinsSetting.Persistent:
                    text += "persistent".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;

                case ClearedPinsSetting.All:
                    text += "all".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }


            Button.Content = text;
        }
    }
}
