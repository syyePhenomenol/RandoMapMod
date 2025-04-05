using RandoMapMod.Localization;

namespace RandoMapMod.UI;

internal class ReachablePinsButton() : BorderlessExtraButton(nameof(ReachablePinsButton))
{
    protected override void OnClick()
    {
        RandoMapMod.GS.ToggleReachablePins();
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }

    public override void Update()
    {
        this.SetButtonBoolToggle($"{"Indicate\nreachable".L()}: ", RandoMapMod.GS.ReachablePins);
    }
}
