namespace RandoMapMod.Pathfinder.Instructions
{
    internal class BenchwarpInstruction : WaypointInstruction
    {
        internal RmmBenchKey BenchKey { get; }

        internal BenchwarpInstruction(string name, RmmBenchKey benchKey, string waypoint)
            : base(name, $"{benchKey.SceneName}[]", waypoint)
        {
            BenchKey = benchKey;
        }

        internal override bool IsFinished(ItemChanger.Transition lastTransition)
        {
            return lastTransition.ToString() == TargetTransition
                && PlayerData.instance.GetString("respawnMarkerName") == BenchKey.RespawnMarkerName;
        }
    }
}
