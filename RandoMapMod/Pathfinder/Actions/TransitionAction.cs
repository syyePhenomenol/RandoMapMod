using RandoMapMod.Transition;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions;

internal class TransitionAction(Term sourceTerm, Term targetTerm, Dictionary<string, string> compassObjects)
    : PlacementAction(sourceTerm, targetTerm),
        IInstruction
{
    string IInstruction.SourceText => Source.Name;
    string IInstruction.TargetText => Target.Name;

    Dictionary<string, string> IInstruction.CompassObjectPaths =>
        compassObjects is not null ? new(compassObjects) : null;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
    {
        if (IsInvalidTransition(node, pm))
        {
            satisfiableStates = default;
            return false;
        }

        return base.TryDo(node, pm, out satisfiableStates);
    }

    public override bool TryDoStateless(Node node, ProgressionManager pm)
    {
        if (IsInvalidTransition(node, pm))
        {
            return false;
        }

        return base.TryDoStateless(node, pm);
    }

    private protected virtual bool IsInvalidTransition(Node node, ProgressionManager pm)
    {
        return !TransitionData.IsVisitedTransition(node.Term.Name);
    }

    bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
    {
        // Fix for big mantis village transition
        return lastTransition.ToString() switch
        {
            "Fungus2_15[top2]" => Target.Name is "Fungus2_15[top3]",
            "Fungus2_14[bot1]" => Target.Name is "Fungus2_14[bot3]",
            _ => Target.Name == lastTransition.ToString(),
        };
    }
}

internal class TopFallTransitionAction(TransitionAction ta, LogicDef logic)
    : TransitionAction(ta.Source, ta.Target, ((IInstruction)ta).CompassObjectPaths)
{
    internal LogicDef Logic { get; } = logic;

    private protected override bool IsInvalidTransition(Node node, ProgressionManager pm)
    {
        return base.IsInvalidTransition(node, pm)
            || ((node.Depth is 0 || node.Actions.Last() is TransitionAction) && !Logic.CanGet(pm));
    }
}

internal class InfectionTransitionAction(TransitionAction ta)
    : TransitionAction(ta.Source, ta.Target, ((IInstruction)ta).CompassObjectPaths)
{
    private protected override bool IsInvalidTransition(Node node, ProgressionManager pm)
    {
        return base.IsInvalidTransition(node, pm) || pm.Get("RMM_Infected") > 0;
    }
}
