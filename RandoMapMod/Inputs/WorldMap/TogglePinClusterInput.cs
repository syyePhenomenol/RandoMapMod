using RandoMapMod.Pins;
using RandoMapMod.UI;

namespace RandoMapMod.Input;

internal class TogglePinClusterInput : RmmWorldMapInput
{
    internal TogglePinClusterInput()
        : base("Toggle Overlapping Pins", () => InputHandler.Instance.inputActions.dreamNail)
    {
        Instance = this;
    }

    internal static TogglePinClusterInput Instance { get; private set; }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && RandoMapMod.GS.PinSelectionOn;
    }

    public override void DoAction()
    {
        if (PinSelector.Instance.SelectedObject is PinCluster pinCluster)
        {
            pinCluster.ToggleSelectedPin();
            PinSelectionPanel.Instance.HideHint();
            PinSelectionPanel.Instance.Update();
        }
    }
}
