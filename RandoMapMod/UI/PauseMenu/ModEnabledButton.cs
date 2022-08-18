using MagicUI.Core;
using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ModEnabledButton : MainButton
    {
        public static ModEnabledButton Instance { get; private set; }

        public ModEnabledButton() : base("Mod Enabled", RandoMapMod.MOD, 0, 0)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            MapChanger.Settings.ToggleModEnabled();
        }

        public override void Update()
        {
            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (MapChanger.Settings.CurrentMode().Mod is RandoMapMod.MOD)
            {
                Button.Visibility = Visibility.Visible;
            }
            else
            {
                Button.Visibility = Visibility.Hidden;
            }

            if (MapChanger.Settings.MapModEnabled())
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                Button.Content = $"{L.Localize("Map Mod")}\n{L.Localize("Enabled")}";
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Disabled);
                Button.Content = $"{L.Localize("Map Mod")}\n{L.Localize("Disabled")}";
            }
        }
    }
}
