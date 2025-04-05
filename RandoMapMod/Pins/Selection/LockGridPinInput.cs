using RandoMapMod.UI;

namespace RandoMapMod.Pins;

internal class LockGridPinInput() : RmmMapInput("Lock Grid Pin", InputHandler.Instance.inputActions.dreamNail)
{
    public override void DoAction()
    {
        if (PinSelector.Instance.SelectedObject is GridPin)
        {
            PinSelector.Instance.ToggleLockSelection();
            PinSelectionPanel.Instance.Update();
        }
    }
}
