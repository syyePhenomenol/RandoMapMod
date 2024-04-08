using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI
{
    internal class DefaultTransitionModeButton : ExtraButton
    {
        public DefaultTransitionModeButton() : base(nameof(DefaultTransitionModeButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.ToggleDefaultTransitionRandoMode();
            OnHover();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = $"{"Default map mode when starting a new transition rando save".L()}: {RandoMapMod.GS.DefaultTransitionRandoMode.ToString().ToCleanName().L()}";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Def. transition\nmode".L()}: ";

            RmmColorSetting colorSetting = RmmColorSetting.UI_Neutral;

            switch (RandoMapMod.GS.DefaultTransitionRandoMode)
            {
                case RmmMode.Full_Map:
                    colorSetting = RmmColorSetting.UI_On;
                    text += "FM".L();
                    break;
                case RmmMode.All_Pins:
                    text += "AP".L();
                    break;
                case RmmMode.Pins_Over_Area:
                    text += "POA".L();
                    break;
                case RmmMode.Pins_Over_Room:
                    text += "POR".L();
                    break;
                case RmmMode.Transition_Normal:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += "T1".L();
                    break;
                case RmmMode.Transition_Visited_Only:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += "T2".L();
                    break;
                case RmmMode.Transition_All_Rooms:
                    colorSetting = RmmColorSetting.UI_Special;
                    text += "T3".L();
                    break;
                default:
                    break;
            }

            Button.ContentColor = RmmColors.GetColor(colorSetting);
            Button.Content = text;
        }
    }
}
