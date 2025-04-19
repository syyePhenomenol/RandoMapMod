using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RandoMapMod.Pathfinder.Actions;

internal class StartWarpAction(Term term) : BenchwarpAction(term, BenchwarpInterop.StartKey), IInstruction
{
    string IInstruction.SourceText => BenchwarpInterop.BENCH_WARP_START;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
    {
        if (!RandoMapMod.GS.PathfinderBenchwarp)
        {
            satisfiableStates = null;
            return false;
        }

        List<State> states = [];

        if (RandoMapMod.Data.WarpToStartReset is StateModifier startVariable)
        {
            foreach (var sb in node.Current.States.Select(s => new StateBuilder(s)))
            {
                states.AddRange(startVariable.ModifyState(null, pm, new(sb)).Select(sb => sb.GetState()));
            }
        }

        satisfiableStates = new(states);
        return true;
    }
}
