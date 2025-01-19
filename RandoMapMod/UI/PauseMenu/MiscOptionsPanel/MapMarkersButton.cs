using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class MapMarkersButton() : ExtraButton(nameof(MapMarkersButton), RandoMapMod.MOD)
    {
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
            this.SetButtonBoolToggle($"{"Show map\nmarkers".L()}: ", RandoMapMod.GS.ShowMapMarkers);
        }
    }
}