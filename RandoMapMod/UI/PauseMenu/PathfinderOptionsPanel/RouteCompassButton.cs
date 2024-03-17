using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class RouteCompassButton : ExtraButton
    {
        internal RouteCompassButton() : base(nameof(RouteCompassButton), RandoMapMod.MOD)
        {

        }

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
            string text = $"{"Route compass".L()}:\n";

            if (RandoMapMod.GS.ShowRouteCompass)
            {
                text += "on".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            }
            else
            {
                text += "off".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = text;
        }
    }
}
