using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class RouteTextButton() : ExtraButton(nameof(RouteTextButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleRouteTextInGame();
            MapUILayerUpdater.Update();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "How the route text is displayed during gameplay.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Route text".L()}:\n";

            switch (RandoMapMod.GS.RouteTextInGame)
            {
                case Settings.RouteTextInGame.Hide:
                    text += "hide".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case Settings.RouteTextInGame.NextTransitionOnly:
                    text += "next transition".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
                    
                case Settings.RouteTextInGame.Show:
                    text += "all transitions".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }

            Button.Content = text;
        }
    }
}
