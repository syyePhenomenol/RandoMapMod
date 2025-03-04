using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;
using RCPathfinder;

namespace RandoMapMod.Pathfinder.Actions
{
    // TODO: REDO ACTION HIERARCHY, MAKE BENCH/START WARPS GO FROM ARBITRARY STATE
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
            var benchVariable = (WarpToStartResetVariable)pm.lm.GetVariableStrict(WarpToStartResetVariable.Prefix);
            foreach (StateBuilder sb in node.Current.States.Select(s => new StateBuilder(s)))
            {
                states.AddRange(benchVariable.ModifyState(null, pm, new(sb)).Select(sb => sb.GetState()));
            }
            
            satisfiableStates = new(states);
            return true;
        }
    }
}