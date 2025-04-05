using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions;

internal class BenchwarpAction(Term term, RmmBenchKey benchKey) : StartJumpAction, IInstruction
{
    private readonly string _benchText = term.Name;

    public override Term Target => term;
    public override float Cost => 1f;

    internal RmmBenchKey BenchKey { get; } = benchKey;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
    {
        if (!RandoMapMod.GS.PathfinderBenchwarp || !BenchwarpInterop.GetVisitedBenchKeys().Contains(BenchKey))
        {
            satisfiableStates = null;
            return false;
        }

        List<State> states = [];
        var benchVariable = (WarpToBenchResetVariable)pm.lm.GetVariableStrict(WarpToBenchResetVariable.Prefix);
        foreach (var sb in node.Current.States.Select(s => new StateBuilder(s)))
        {
            states.AddRange(benchVariable.ModifyState(null, pm, new(sb)).Select(sb => sb.GetState()));
        }

        satisfiableStates = new(states);
        return true;
    }

    string IInstruction.SourceText => _benchText;
    string IInstruction.TargetText => null;

    Dictionary<string, string> IInstruction.CompassObjectPaths => null;

    bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
    {
        return lastTransition.ToString() == $"{BenchKey.SceneName}[]"
            && PlayerData.instance.GetString("respawnMarkerName") == BenchKey.RespawnMarkerName;
    }
}
