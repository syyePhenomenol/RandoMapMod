using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class ReachablePinsButton() : ExtraButton(nameof(ReachablePinsButton), RandoMapMod.MOD)
    {
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
            RmmTitle.Instance.HoveredText = "Pins for unreachable locations are smaller/grayed out.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            this.SetButtonBoolToggle($"{"Indicate\nreachable".L()}: ", RandoMapMod.GS.ReachablePins);
        }
    }
}