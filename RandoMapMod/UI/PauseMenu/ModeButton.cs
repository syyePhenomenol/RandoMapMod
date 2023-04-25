using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ModeButton : MainButton
    {
        public ModeButton() : base(nameof(ModeButton), RandoMapMod.MOD, 1, 0)
        {

        }

        protected override void OnClick()
        {
            MapChanger.Settings.ToggleMode();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Toggle to the next map mode.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Mode")}:";

            MapMode mode = MapChanger.Settings.CurrentMode();

            if (mode is FullMapMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += '\n' + L.Localize("Full Map");
            }

            if (mode is AllPinsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += '\n' + L.Localize("All Pins");
            }

            if (mode is PinsOverAreaMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize(" Pins\nOver Area");
            }

            if (mode is PinsOverRoomMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize(" Pins\nOver Room");
            }

            if (mode is TransitionNormalMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += '\n' + L.Localize("Transition") + " 1";
            }

            if (mode is TransitionVisitedOnlyMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += '\n' + L.Localize("Transition") + " 2";
            }

            if (mode is TransitionAllRoomsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += '\n' + L.Localize("Transition") + " 3";
            }

            Button.Content = text;
        }
    }
}
