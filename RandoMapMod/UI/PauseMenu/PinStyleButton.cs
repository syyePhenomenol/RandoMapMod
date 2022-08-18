using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinStyleButton : MainButton
    {
        internal static PinStyleButton Instance { get; private set; }

        internal PinStyleButton() : base("Pin Style", RandoMapMod.MOD, 1, 1)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.TogglePinStyle();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Pin Style")}:\n";

            switch (RandoMapMod.GS.PinStyle)
            {
                case PinStyle.Normal:
                    text += L.Localize("normal");
                    break;

                case PinStyle.Q_Marks_1:
                    text += $"{L.Localize("q marks")} 1";
                    break;

                case PinStyle.Q_Marks_2:
                    text += $"{L.Localize("q marks")} 2";
                    break;

                case PinStyle.Q_Marks_3:
                    text += $"{L.Localize("q marks")} 3";
                    break;
            }

            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            Button.Content = text;
        }
    }
}
