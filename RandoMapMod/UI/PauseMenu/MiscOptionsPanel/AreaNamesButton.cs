using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class AreaNamesButton() : ExtraButton(nameof(AreaNamesButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleAreaNames();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Show area names on the world/quick map.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            this.SetButtonBoolToggle($"{"Show area\nnames".L()}: ", RandoMapMod.GS.ShowAreaNames);
        }
    }
}
