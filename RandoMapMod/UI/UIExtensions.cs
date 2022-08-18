using MagicUI.Core;
using MagicUI.Elements;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal static class UIExtensions
    {
        //TODO: move some of these into MapChanger
        internal static TextObject TextFromEdge(LayoutRoot onLayout, string name, bool onRight)
        {
            TextObject text = BaseText(onLayout, name);

            if (onRight)
            {
                text.HorizontalAlignment = HorizontalAlignment.Right;
                text.TextAlignment = HorizontalAlignment.Right;
                text.Padding = new(0f, 20f, 20f, 0f);
            }
            else
            {
                text.Padding = new(20f, 20f, 0f, 0f);
            }

            return text;
        }

        internal static TextObject PanelText(LayoutRoot onLayout, string name)
        {
            TextObject text = BaseText(onLayout, name);
            text.VerticalAlignment = VerticalAlignment.Center;
            text.Padding = new(0f, 2f, 0f, 2f);
            return text;
        }

        private static TextObject BaseText(LayoutRoot onLayout, string name)
        {
            return new(onLayout, name)
            {
                ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = HorizontalAlignment.Left,
                Font = MagicUI.Core.UI.TrajanNormal,
                FontSize = 14
            };
        }

        internal static void SetToggleText(TextObject textObj, string baseText, bool value)
        {
            string text = baseText;

            if (value)
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("On");
            }
            else
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("Off");
            }

            textObj.Text = text;
        }
    }
}
