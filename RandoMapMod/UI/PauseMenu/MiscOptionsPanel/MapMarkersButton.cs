using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

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
            RmmTitle.Instance.HoveredText = "Enable vanilla map marker behaviour.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{L.Localize("Show map\nmarkers")}: ";

            if (RandoMapMod.GS.ShowMapMarkers)
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