using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class NextAreasButton : ExtraButton
    {
        public NextAreasButton() : base(nameof(NextAreasButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleNextAreas();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Show next area indicators (text/arrow) on the quick map.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Show next\nareas".L()}: ";

            switch (RandoMapMod.GS.ShowNextAreas)
            {
                case NextAreaSetting.Off:
                    text += "Off".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case NextAreaSetting.Arrows:
                    text += "Arrows".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;

                case NextAreaSetting.Full:
                    text += "Full".L();
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }

            Button.Content = text;
        }
    }
}
