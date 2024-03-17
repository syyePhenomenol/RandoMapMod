using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class QuillButton : ExtraButton
    {
        public QuillButton() : base(nameof(QuillButton), RandoMapMod.MOD)
        {

        }

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
            string text = $"{"Always have\nQuill".L()}: ";

            if (RandoMapMod.GS.AlwaysHaveQuill)
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
