using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class QMarkSettingButton() : ExtraButton(nameof(QMarkSettingButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleQMarkSetting();
            ItemCompass.Info.UpdateCurrentCompassTargets();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Toggle question mark sprites on/off.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Question\nmarks".L()}: ";

            switch (RandoMapMod.GS.QMarks)
            {
                case QMarkSetting.Off:
                    text += "off".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case QMarkSetting.Red:
                    text += "red".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;

                case QMarkSetting.Mix:
                    text += "mixed".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }

            Button.Content = text;
        }
    }
}
