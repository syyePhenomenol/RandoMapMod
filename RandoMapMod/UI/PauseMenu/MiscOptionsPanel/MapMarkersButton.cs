using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class MapMarkersButton : ExtraButton
    {
        public MapMarkersButton() : base(nameof(MapMarkersButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleMapMarkers();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Enable vanilla map marker behavior.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Show map\nmarkers".L()}: ";

            if (RandoMapMod.GS.ShowMapMarkers)
            {
                text += "On".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            }
            else
            {
                text += "Off".L();
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = text;
        }
    }
}