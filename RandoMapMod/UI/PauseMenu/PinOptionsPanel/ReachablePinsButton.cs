using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ReachablePinsButton : ExtraButton
    {
        public ReachablePinsButton() : base(nameof(ReachablePinsButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleReachablePins();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Pins for unreachable locations are smaller/grayed out.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{L.Localize("Indicate\nreachable")}: ";

            if (RandoMapMod.GS.ReachablePins)
            {
                text += L.Localize("On");
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            }
            else
            {
                text += L.Localize("Off");
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = text;
        }
    }
}