using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class PinShapeButton() : MainButton(nameof(PinShapeButton), RandoMapMod.MOD, 1, 1)
    {
        protected override void OnClick()
        {
            RandoMapMod.GS.TogglePinShape();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Toggle the shape of the pins.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{"Pin Shape".L()}:\n";

            switch (RandoMapMod.GS.PinShapes)
            {
                case PinShapeSetting.Mixed:
                    text += "mixed".L();
                    break;

                case PinShapeSetting.All_Circle:
                    text += "circles".L();
                    break;

                case PinShapeSetting.All_Diamond:
                    text += "diamonds".L();
                    break;

                case PinShapeSetting.All_Square:
                    text += "squares".L();
                    break;

                case PinShapeSetting.All_Pentagon:
                    text += "pentagons".L();
                    break;

                case PinShapeSetting.All_Hexagon:
                    text += "hexagons".L();
                    break;
                
                case PinShapeSetting.No_Border:
                    text += "no borders".L();
                    break;
            }

            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            Button.Content = text;
        }
    }
}
