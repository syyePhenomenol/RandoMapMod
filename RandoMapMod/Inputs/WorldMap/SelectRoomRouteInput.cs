using MapChanger.MonoBehaviours;
using RandoMapMod.Pathfinder;
using RandoMapMod.Rooms;
using RandoMapMod.UI;

namespace RandoMapMod.Input;

internal class SelectRoomRouteInput : RmmWorldMapInput
{
    internal SelectRoomRouteInput()
        : base("Select Pathfinder Route", () => InputHandler.Instance.inputActions.menuSubmit)
    {
        Instance = this;
    }

    internal static SelectRoomRouteInput Instance { get; private set; }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && RandoMapMod.GS.RoomSelectionOn;
    }

    public override void DoAction()
    {
        if (TransitionRoomSelector.Instance?.SelectedObject is ISelectable obj)
        {
            _ = RmmPathfinder.RM.TryGetNextRouteTo(obj.Key);

            RouteText.Instance.Update();
            RouteSummaryText.Instance.Update();
            RoomSelectionPanel.Instance.Update();
        }
    }
}
