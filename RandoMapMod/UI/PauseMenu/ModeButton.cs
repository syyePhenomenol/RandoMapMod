using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class ModeButton() : MainButton(nameof(ModeButton), RandoMapMod.MOD, 1, 0)
    {
        protected override void OnClick()
        {
            MapChanger.Settings.ToggleMode();
            OnHover();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = $"{"Current map mode".L()}: {MapChanger.Settings.CurrentMode().ModeName.ToString().ToCleanName().L()}";
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

            RmmColorSetting colorSetting = RmmColorSetting.UI_Neutral;

            switch (MapChanger.Settings.CurrentMode())
            {
                case FullMapMode:
                    colorSetting = RmmColorSetting.UI_On;
                    text += $"\n{"Full Map".L()}";
                    break;
                case AllPinsMode:
                    text += $"\n{"All Pins".L()}";
                    break;
                case PinsOverAreaMode:
                    text += $" {"Pins\nOver Area".L()}";
                    break;
                case PinsOverRoomMode:
                    text += $" {"Pins\nOver Room".L()}";
                    break;
                case TransitionNormalMode:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += $"\n{"Transition".L()} 1";
                    break;
                case TransitionVisitedOnlyMode:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += $"\n{"Transition".L()} 2";;
                    break;
                case TransitionAllRoomsMode:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += $"\n{"Transition".L()} 3";
                    break;
                default:
                    break;
            }

            Button.ContentColor = RmmColors.GetColor(colorSetting);
            Button.Content = text;
        }
    }
}
