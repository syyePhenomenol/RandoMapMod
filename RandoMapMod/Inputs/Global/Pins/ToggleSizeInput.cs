namespace RandoMapMod.Input;

internal class ToggleSizeInput : PinHotkeyInput
{
    internal ToggleSizeInput()
        : base("Toggle Pin Size", UnityEngine.KeyCode.Alpha5)
    {
        Instance = this;
    }

    internal static ToggleSizeInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.TogglePinSize();
        base.DoAction();
    }
}
