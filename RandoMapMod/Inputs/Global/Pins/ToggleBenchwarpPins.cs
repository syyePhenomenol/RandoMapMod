namespace RandoMapMod.Input;

internal class ToggleBenchwarpPinsInput : MapUIKeyInput
{
    internal ToggleBenchwarpPinsInput()
        : base("Toggle Benchwarp Pins", UnityEngine.KeyCode.W)
    {
        Instance = this;
    }

    internal static ToggleBenchwarpPinsInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.ToggleBenchwarpPins();
        base.DoAction();
    }
}
