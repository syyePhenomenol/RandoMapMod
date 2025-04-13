namespace RandoMapMod.Pathfinder.Actions;

internal interface IInstruction
{
    internal string SourceText { get; }
    internal string TargetText { get; }

    internal string GetCompassObjectPath(string scene);
    internal bool IsFinished(ItemChanger.Transition lastTransition);
}
