using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class RouteCompassButton() : ExtraButton(nameof(RouteCompassButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleRouteCompassEnabled();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Point compass to next transition in the pathfinder route.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            this.SetButtonBoolToggle($"{"Route compass".L()}:\n", RandoMapMod.GS.ShowRouteCompass);
        }
    }
}
