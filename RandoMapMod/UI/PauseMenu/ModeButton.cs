using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Localization;

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
            RmmTitle.Instance.HoveredText = "Toggle to the next map mode.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{"Mode".L()}:";

            MapMode mode = MapChanger.Settings.CurrentMode();

            if (mode is FullMapMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += $"\n{"Full Map".L()}";
            }

            if (mode is AllPinsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += $"\n{"All Pins".L()}";
            }

            if (mode is PinsOverAreaMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += $" {"Pins\nOver Area".L()}";
            }

            if (mode is PinsOverRoomMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += $" {"Pins\nOver Room".L()}";
            }

            if (mode is TransitionNormalMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += $"\n{"Transition".L()} 1";
            }

            if (mode is TransitionVisitedOnlyMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += $"\n{"Transition".L()} 2";
            }

            if (mode is TransitionAllRoomsMode)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
                text += $"\n{"Transition".L()} 3";
            }

            Button.Content = text;
        }
    }
}
