using MapChanger.MonoBehaviours;
using RandoMapMod.Pathfinder;
using RandoMapMod.UI;

namespace RandoMapMod.Rooms;

internal class SelectRoomRouteInput()
    : RmmMapInput("Select Pathfinder Route", InputHandler.Instance.inputActions.menuSubmit)
{
    public override void DoAction()
    {
        if (TransitionRoomSelector.Instance.SelectedObject is ISelectable obj)
        {
            _ = RmmPathfinder.RM.TryGetNextRouteTo(obj.Key);

            RouteText.Instance.Update();
            RouteSummaryText.Instance.Update();
            RoomSelectionPanel.Instance.Update();
        }
    }
}
