using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class NextAreasButton : MainButton
    {
        public NextAreasButton() : base("Next Areas Button", RandoMapMod.MOD, 2, 2)
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

        public override void Update()
        {
            base.Update();

            Button.Visibility = MiscPanel.Instance.ExtraButtonsGrid.Visibility;

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
