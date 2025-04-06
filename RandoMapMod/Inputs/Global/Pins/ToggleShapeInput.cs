namespace RandoMapMod.Input;

internal class ToggleShapeInput : PinHotkeyInput
{
    internal ToggleShapeInput()
        : base("Toggle Pin Shape", UnityEngine.KeyCode.Alpha4)
    {
        Instance = this;
    }

    internal static ToggleShapeInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.TogglePinShape();
        base.DoAction();
    }
}
