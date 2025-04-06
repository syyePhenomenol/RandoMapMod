using RandoMapMod.UI;

namespace RandoMapMod.Input;

internal class LocationHintInput : RmmWorldMapInput
{
    internal LocationHintInput()
        : base("Show Location Hint", () => InputHandler.Instance.inputActions.quickCast)
    {
        Instance = this;
    }

    internal static LocationHintInput Instance { get; private set; }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && RandoMapMod.GS.PinSelectionOn;
    }

    public override void DoAction()
    {
        PinSelectionPanel.Instance.RevealHint();
        PinSelectionPanel.Instance.Update();
    }
}
