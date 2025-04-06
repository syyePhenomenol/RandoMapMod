using RandoMapMod.Pins;
using RandoMapMod.UI;

namespace RandoMapMod.Input;

internal class LockGridPinInput : RmmWorldMapInput
{
    internal LockGridPinInput()
        : base("Lock Grid Pin", () => InputHandler.Instance.inputActions.dreamNail)
    {
        Instance = this;
    }

    internal static LockGridPinInput Instance { get; private set; }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && RandoMapMod.GS.PinSelectionOn;
    }

    public override void DoAction()
    {
        if (PinSelector.Instance?.SelectedObject is GridPin)
        {
            PinSelector.Instance.ToggleLockSelection();
            PinSelectionPanel.Instance.Update();
        }
    }
}
