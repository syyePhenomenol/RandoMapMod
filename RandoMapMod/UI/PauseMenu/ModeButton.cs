using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ModeButton : MainButton
    {
        public static ModeButton Instance { get; private set; }

        public ModeButton() : base("Mode", RandoMapMod.MOD, 1, 0)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            MapChanger.Settings.ToggleMode();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Mode")}:\n";

            MapMode mode = MapChanger.Settings.CurrentMode();

            if (mode is FullMapMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("Full Map");
            }

            if (mode is AllPinsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("All Pins");
            }

            if (mode is PinsOverMapMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("Pins Over Map");
            }

            if (mode is TransitionNormalMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += L.Localize("Transition") + " 1";
            }

            if (mode is TransitionVisitedOnlyMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += L.Localize("Transition") + " 2";
            }

            if (mode is TransitionAllRoomsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += L.Localize("Transition") + " 3";
            }

            Button.Content = text;
        }
    }
}
