using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions;

internal class DreamgateAction() : StartJumpAction, IInstruction
{
    internal DreamgateTracker Dgt => RmmPathfinder.Dgt;

    // Used for figuring out where Dreamgate goes to during the pathfinder search
    public override Term Target => Dgt?.DreamgateLinkedPosition;
    public override float Cost => 1f;

    // Used for checking the route
    private Term _lastSearchTarget;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
    {
        if (Dgt.DreamgateLinkedPosition is null)
        {
            satisfiableStates = null;
            return false;
        }

        // Lock dreamgate destination to current one
        _lastSearchTarget = Dgt.DreamgateLinkedPosition;

        List<State> states = [];
        foreach (var sb in node.Current.States.Select(s => new StateBuilder(s)))
        {
            _ = sb.TrySetStateBool(pm.lm.StateManager, "NOFLOWER", true);
            states.Add(new(sb));
        }

        satisfiableStates = new(states);
        return true;
    }

    string IInstruction.SourceText => "Dreamgate";
    string IInstruction.TargetText => null;

    bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
    {
        return lastTransition.GateName is "dreamGate"
            && Dgt.DreamgateLinkedPosition is not null
            && Dgt.DreamgateLinkedPosition == _lastSearchTarget;
    }

    string IInstruction.GetCompassObjectPath(string scene)
    {
        return null;
    }
}
