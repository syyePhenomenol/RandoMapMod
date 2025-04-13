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
    private readonly Dictionary<string, string> _compassObjects = compassObjects;

    string IInstruction.SourceText => text;
    string IInstruction.TargetText => null;

    bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
    {
        return lastTransition.ToString() == Target.Name;
    }

    string IInstruction.GetCompassObjectPath(string scene)
    {
        if (_compassObjects is not null && _compassObjects.TryGetValue(scene, out var path))
        {
            return path;
        }

        return null;
    }
}
