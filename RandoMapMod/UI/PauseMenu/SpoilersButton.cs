using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

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
            RmmTitle.Instance.HoveredText = "Reveals the items at each location.".L();
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
                Button.Content = $"{"Spoilers".L()}:\n{"on".L()}";
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                Button.Content = $"{"Spoilers".L()}:\n{"off".L()}";
            }
        }
    }
}
