namespace RandoMapMod.Input;

internal class RoomPanelInput : MapUIKeyInput
{
    internal RoomPanelInput()
        : base("Toggle Room Panel", UnityEngine.KeyCode.R)
    {
        Instance = this;
    }

    internal static RoomPanelInput Instance { get; private set; }

    public override void DoAction()
    {
        RandoMapMod.GS.ToggleRoomSelection();
        base.DoAction();
    }
}
