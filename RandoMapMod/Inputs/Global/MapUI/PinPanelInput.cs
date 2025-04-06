namespace RandoMapMod.Input;

internal class PinPanelInput : MapUIKeyInput
{
    internal PinPanelInput()
        : base("Toggle Pin Panel", UnityEngine.KeyCode.P)
    {
        Instance = this;
    }

    internal static PinPanelInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.TogglePinSelection();
        base.DoAction();
    }
}
