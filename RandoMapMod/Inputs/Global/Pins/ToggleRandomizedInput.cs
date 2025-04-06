namespace RandoMapMod.Input;

internal class ToggleRandomizedInput : PinHotkeyInput
{
    internal ToggleRandomizedInput()
        : base("Toggle Randomized Pins", UnityEngine.KeyCode.Alpha2)
    {
        Instance = this;
    }

    internal static ToggleRandomizedInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.LS.ToggleRandomized();
        base.DoAction();
    }
}
