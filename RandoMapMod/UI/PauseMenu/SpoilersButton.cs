using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class SpoilersButton : MainButton
    {
        internal SpoilersButton() : base(nameof(SpoilersButton), RandoMapMod.MOD, 0, 3)
        {

        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleSpoilers();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Reveals the items at each location.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
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
