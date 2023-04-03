using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class DefaultSettingsButton : ExtraButton
    {
        public DefaultSettingsButton() : base(nameof(DefaultSettingsButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            GlobalSettings.ResetToDefaultSettings();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Resets all global settings of RandoMapMod.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{L.Localize("Reset global\nsettings")}";

            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);

            Button.Content = text;
        }
    }
}