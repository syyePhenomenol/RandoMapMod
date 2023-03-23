using Newtonsoft.Json;

namespace RandoMapMod.Pathfinder.Instructions
{
    internal class WaypointInstruction : Instruction
    {
        internal string Waypoint { get; }

        [JsonConstructor]
        internal WaypointInstruction(string name, string targetTransition, string waypoint) : base(name, targetTransition)
        {
            Waypoint = waypoint;
        }
    }
}
