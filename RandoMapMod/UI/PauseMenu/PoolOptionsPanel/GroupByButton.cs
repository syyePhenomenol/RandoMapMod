using MagicUI.Elements;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class GroupByButton() : BorderlessExtraButton(nameof(GroupByButton))
{
    protected override void OnClick()
    {
        RandoMapMod.LS.ToggleGroupBy();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Group pools by either location (normal) or by item (spoilers).".L();
    }

    public override void Update()
    {
        var text = $"{"Group by".L()}:\n";

        switch (RandoMapMod.LS.GroupBy)
        {
            case GroupBySetting.Location:
                text += "Location".L();
                break;

            case GroupBySetting.Item:
                text += "Item".L();
                break;
            default:
                break;
        }

        Button.Content = text;
        Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
    }
}
