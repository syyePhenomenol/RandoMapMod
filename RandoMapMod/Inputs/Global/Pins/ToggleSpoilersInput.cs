namespace RandoMapMod.Input;

internal class ToggleSpoilersInput : PinHotkeyInput
{
    internal ToggleSpoilersInput()
        : base("Toggle Spoilers", UnityEngine.KeyCode.Alpha1)
    {
        Instance = this;
    }

    internal static ToggleSpoilersInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.LS.ToggleSpoilers();
        base.DoAction();
    }
}
