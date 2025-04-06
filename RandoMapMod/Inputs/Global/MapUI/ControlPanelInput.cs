namespace RandoMapMod.Input;

internal class ControlPanelInput : MapUIKeyInput
{
    internal ControlPanelInput()
        : base("Toggle Control Panel", UnityEngine.KeyCode.H)
    {
        Instance = this;
    }

    internal static ControlPanelInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.ToggleControlPanel();
        base.DoAction();
    }
}
