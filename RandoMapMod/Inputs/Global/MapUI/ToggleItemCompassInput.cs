namespace RandoMapMod.Input;

internal class ToggleItemCompassInput : MapUIKeyInput
{
    internal ToggleItemCompassInput()
        : base("Toggle Item Compass", UnityEngine.KeyCode.C)
    {
        Instance = this;
    }

    internal static ToggleItemCompassInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.ToggleItemCompass();
        base.DoAction();
    }
}
