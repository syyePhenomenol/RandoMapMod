using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

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
            MapUILayerUpdater.Update();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Resets all global settings of RandoMapMod.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            Button.Content = "Reset global\nsettings".L();

            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
        }
    }
}