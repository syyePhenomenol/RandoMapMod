using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class AreaNamesButton : ExtraButton
    {
        public AreaNamesButton() : base(nameof(AreaNamesButton), RandoMapMod.MOD)
        {

        }

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
            string text = $"{"Show area\nnames".L()}: ";

            if (RandoMapMod.GS.ShowAreaNames)
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
