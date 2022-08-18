using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class SpoilersButton : MainButton
    {
        internal static SpoilersButton Instance { get; private set; }

        internal SpoilersButton() : base("Spoilers", RandoMapMod.MOD, 0, 3)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleSpoilers();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (RandoMapMod.LS.SpoilerOn)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                Button.Content = $"{L.Localize("Spoilers")}:\n{L.Localize("on")}";
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                Button.Content = $"{L.Localize("Spoilers")}:\n{L.Localize("off")}";
            }
        }
    }
}
