using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class QuillButton() : ExtraButton(nameof(QuillButton), RandoMapMod.MOD)
    {
        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleAlwaysHaveQuill();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Doesn't affect Full Map and Transition modes.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            this.SetButtonBoolToggle($"{"Always have\nQuill".L()}: ", RandoMapMod.GS.AlwaysHaveQuill);
        }
    }
}
