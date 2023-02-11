using Newtonsoft.Json;

namespace RandoMapMod.Pathfinder.Instructions
{
    internal class TramInstruction : WaypointInstruction
    {
        [JsonConstructor]
        internal TramInstruction(string name, string targetTransition, string waypoint)
            : base(name, targetTransition, waypoint) { }

        internal override bool IsInProgress(ItemChanger.Transition lastTransition)
        {
            if (Waypoint is "Lower_Tram" && lastTransition.SceneName is "Room_Tram") return true;

            if (Waypoint is "Upper_Tram" && lastTransition.SceneName is "Room_Tram_RG") return true;

            return base.IsInProgress(lastTransition);
        }
    }
}
