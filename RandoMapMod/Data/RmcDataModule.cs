using ItemChanger;
using MapChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Data;

public abstract class RmcDataModule : HookModule
{
    public abstract bool IsCorrectSaveType { get; }

    public abstract IReadOnlyDictionary<string, RmcTransitionDef> RandomizedTransitions { get; }
    public abstract IReadOnlyDictionary<string, RmcTransitionDef> VanillaTransitions { get; }
    public abstract IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> RandomizedTransitionPlacements { get; }
    public abstract IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> VanillaTransitionPlacements { get; }
    public abstract bool IsCoupledRando { get; }

    public abstract IReadOnlyDictionary<string, RmcLocationDef> RandomizedLocations { get; }
    public abstract IReadOnlyDictionary<string, RmcLocationDef> VanillaLocations { get; }

    public abstract ProgressionManager PM { get; }
    public abstract ProgressionManager PMNoSequenceBreak { get; }
    public abstract Term StartTerm { get; }
    public abstract IReadOnlyCollection<Term> StartStateLinkedTerms { get; }
    public abstract StateModifier WarpToBenchReset { get; }
    public abstract StateModifier WarpToStartReset { get; }

    public abstract IReadOnlyCollection<string> UncheckedReachableTransitions { get; }
    public abstract IReadOnlyCollection<string> UncheckedReachableTransitionsNoSequenceBreak { get; }
    public abstract IReadOnlyDictionary<string, string> VisitedTransitions { get; }
    public abstract IReadOnlyCollection<string> OutOfLogicVisitedTransitions { get; }

    public abstract RandoContext Context { get; }
    public abstract IEnumerable<RandoPlacement> RandomizedPlacements { get; }
    public abstract IEnumerable<RandoPlacement> OolObtainedPlacements { get; }
    public abstract IEnumerable<GeneralizedPlacement> VanillaPlacements { get; }

    public abstract string Localize(string text);
    public abstract string GetMapArea(string scene);

    public abstract RandoPlacement GetItemRandoPlacement(AbstractItem item);
}
