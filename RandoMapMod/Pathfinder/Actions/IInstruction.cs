namespace RandoMapMod.Pathfinder.Actions
{
    internal interface IInstruction
    {
        internal string SourceText { get; }
        internal string TargetText { get; }
        internal Dictionary<string, string> CompassObjectPaths { get; }
        internal bool IsFinished(ItemChanger.Transition lastTransition);
    }
}