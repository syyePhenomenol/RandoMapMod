using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class QuillButton() : BorderlessExtraButton(nameof(QuillButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleAlwaysHaveQuill();
    }

    protected override void OnHover()
    {
        RmmTitle.Instance.HoveredText = "Doesn't affect Full Map and Transition modes.".L();
    }

    public override void Update()
    {
        this.SetButtonBoolToggle($"{"Always have\nQuill".L()}: ", RandoMapMod.GS.AlwaysHaveQuill);
    }
}
