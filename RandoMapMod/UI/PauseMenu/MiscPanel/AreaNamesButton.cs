using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class AreaNamesButton : MainButton
    {
        public AreaNamesButton() : base("Area Name Button", RandoMapMod.MOD, 2, 1)
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

        public override void Update()
        {
            base.Update();

            Button.Visibility = MiscPanel.Instance.ExtraButtonsGrid.Visibility;

            string text = $"{L.Localize("Show area\nnames")}: ";

            if (RandoMapMod.GS.ShowAreaNames)
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
