using MagicUI.Core;
using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal static class UIExtensions
{
    internal static TextObject TextFromEdge(LayoutRoot onLayout, string name, bool onRight)
    {
        TextObject text =
            new(onLayout, name)
            {
                ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = HorizontalAlignment.Left,
                Font = MagicUI.Core.UI.TrajanNormal,
                FontSize = 14,
            };

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

    internal static void SetButtonBoolToggle(this ButtonWrapper bw, string baseText, bool value)
    {
        var text = baseText;

        if (value)
        {
            bw.Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "On".L();
        }
        else
        {
            bw.Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "Off".L();
        }

        bw.Button.Content = text;
    }
}
