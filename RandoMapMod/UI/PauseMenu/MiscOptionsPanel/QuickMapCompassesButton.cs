using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class QuickMapCompassesButton() : BorderlessExtraButton(nameof(QuickMapCompassesButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleQuickMapCompasses();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Show compasses pointing to transitions in room".L();
    }

    public override void Update()
    {
        var text = $"{"Quick map\n compass".L()}: ";

        switch (RandoMapMod.GS.ShowQuickMapCompasses)
        {
            case QuickMapCompassSetting.Unchecked:
                text += "Unchecked".L();
                break;
            case QuickMapCompassSetting.All:
                text += "All".L();
                break;
            default:
                break;
        }

        Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        Button.Content = text;
    }
}
