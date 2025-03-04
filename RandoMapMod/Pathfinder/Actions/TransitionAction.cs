using RandoMapMod.Transition;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions
{
    internal class TransitionAction(Term sourceTerm, Term targetTerm, Dictionary<string, string> compassObjects) : PlacementAction(sourceTerm, targetTerm), IInstruction
    {
        public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
        {
            // Never do two transitions in a row (to prevent false positives where re-entry in the transition isn't possible)
            if (node.Depth is 0 || (node.Actions.Last() is not PlacementAction && TransitionData.IsVisitedTransition(node.Term.Name)))
            {
                return base.TryDo(node, pm, out satisfiableStates);
            }
            
            satisfiableStates = default;
            return false;
        }

        string IInstruction.SourceText => Source.Name;
        string IInstruction.TargetText => Target.Name;
        
        Dictionary<string, string> IInstruction.CompassObjectPaths => compassObjects is not null ? new(compassObjects) : null;

        bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
        {
            // Fix for big mantis village transition
            return lastTransition.ToString() switch
            {
                "Fungus2_15[top2]" => Target.Name is "Fungus2_15[top3]",
                "Fungus2_14[bot1]" => Target.Name is "Fungus2_14[bot3]",
                _ => Target.Name == lastTransition.ToString()
            };
        }
    }

    internal class InfectionTransitionAction(TransitionAction ta) : TransitionAction(ta.Source, ta.Target, ((IInstruction)ta).CompassObjectPaths)
    {
        public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
        {
            if (pm.Get("RMM_Infected") is 0)
            {
                return base.TryDo(node, pm, out satisfiableStates);
            }

            satisfiableStates = default;
            return false;
        }
    }
}