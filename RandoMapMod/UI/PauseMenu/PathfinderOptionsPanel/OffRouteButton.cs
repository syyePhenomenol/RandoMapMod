using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class OffRouteButton() : ExtraButton(nameof(OffRouteButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleWhenOffRoute();
            MapUILayerUpdater.Update();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "When going off route, how the pathfinder route is updated.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Off route".L()}:\n";

            switch (RandoMapMod.GS.WhenOffRoute)
            {
                case Settings.OffRouteBehaviour.Cancel:
                    text += "cancel route".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case Settings.OffRouteBehaviour.Keep:
                    text += "keep route".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case Settings.OffRouteBehaviour.Reevaluate:
                    text += "reevaluate".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }

            Button.Content = text;
        }
    }
}
