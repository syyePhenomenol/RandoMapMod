using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ClearedButton : ExtraButton
    {
        internal ClearedButton() : base(nameof(ClearedButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleCleared();
        }

        public override void Update()
        {
            string text = $"{L.Localize("Cleared\nlocations")}: ";

            if (RandoMapMod.GS.ShowClearedPins)
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
