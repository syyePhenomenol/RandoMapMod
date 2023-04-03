using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

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
            RmmTitle.Instance.HoveredText = "Show next area indicators (text/arrow) on the quick map.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{L.Localize("Show next\nareas")}: ";

            switch (RandoMapMod.GS.ShowNextAreas)
            {
                case NextAreaSetting.Off:
                    text += L.Localize("Off");
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                    break;

                case NextAreaSetting.Arrows:
                    text += L.Localize("Arrows");
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;

                case NextAreaSetting.Full:
                    text += L.Localize("Full");
                    Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                    break;
            }

            Button.Content = text;
        }
    }
}
