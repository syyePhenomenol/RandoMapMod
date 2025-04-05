using RandoMapMod.UI;

namespace RandoMapMod.Pins;

internal class TogglePinClusterInput()
    : RmmMapInput("Toggle Overlapping Pins", InputHandler.Instance.inputActions.dreamNail)
{
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
