using RandomizerCore.Logic;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions;

internal class WaypointAction(
    Term start,
    Term destination,
    DNFLogicDef logic,
    string text,
    Dictionary<string, string> compassObjects
) : StateLogicAction(start, destination, logic), IInstruction
{
    string IInstruction.SourceText => text;
    string IInstruction.TargetText => null;

    Dictionary<string, string> IInstruction.CompassObjectPaths =>
        compassObjects is not null ? new(compassObjects) : null;

    bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
    {
        return lastTransition.ToString() == Target.Name;
    }
}
