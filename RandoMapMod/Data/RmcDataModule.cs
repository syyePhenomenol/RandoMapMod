using ItemChanger;
using MapChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Data;

public abstract class RmcDataModule : HookModule
{
    /// <summary>
    /// Used to determined if this data module should be loaded for the current save.
    /// </summary>
    public abstract bool IsCorrectSaveType { get; }

    public abstract IReadOnlyDictionary<string, RmcTransitionDef> RandomizedTransitions { get; }
    public abstract IReadOnlyDictionary<string, RmcTransitionDef> VanillaTransitions { get; }
    public abstract IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> RandomizedTransitionPlacements { get; }
    public abstract IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> VanillaTransitionPlacements { get; }

    /// <summary>
    /// Is coupled transition rando (randomized transition placements go the same way back and forth).
    /// </summary>
    public abstract bool IsCoupledRando { get; }

    public abstract IReadOnlyDictionary<string, RmcLocationDef> RandomizedLocations { get; }
    public abstract IReadOnlyDictionary<string, RmcLocationDef> VanillaLocations { get; }

    /// <summary>
    /// The ProgressionManager for the save that includes sequence breaks.
    /// </summary>
    public abstract ProgressionManager PM { get; }

    /// <summary>
    /// The ProgressionManager for the save that excludes sequence breaks.
    /// </summary>
    public abstract ProgressionManager PMNoSequenceBreak { get; }

    /// <summary>
    /// A state-valued term that corresponds to the start position.
    /// </summary>
    public abstract Term StartTerm { get; }

    /// <summary>
    /// The state-valued terms that are logically equivalent to the start term.
    /// </summary>
    public abstract IReadOnlyCollection<Term> StartStateLinkedTerms { get; }

    /// <summary>
    /// The state modifier for warping to a bench.
    /// </summary>
    public abstract StateModifier WarpToBenchReset { get; }

    /// <summary>
    /// The state modifier for warping to the start position.
    /// </summary>
    public abstract StateModifier WarpToStartReset { get; }

    /// <summary>
    /// Unchecked reachable transitions, based on all the acquired progression.
    /// Is a superset of UncheckedReachableTransitionsNoSequenceBreak.
    /// </summary>
    public abstract IReadOnlyCollection<string> UncheckedReachableTransitions { get; }

    /// <summary>
    /// Unchecked reachable transitions, based on only the progression that doesn't involve logical sequence breaks.
    /// Is a subset of UncheckedReachableTransitions.
    /// </summary>
    public abstract IReadOnlyCollection<string> UncheckedReachableTransitionsNoSequenceBreak { get; }

    /// <summary>
    /// All randomized transition placements that have been checked.
    /// Is a superset of OutOfLogicVisitedTransitions.
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> VisitedTransitions { get; }

    /// <summary>
    /// The randomized transition sources that have been checked, but cannot be reached without sequence breaking.
    /// Is a subset of VisitedTransitions.
    /// </summary>
    public abstract IReadOnlyCollection<string> OutOfLogicVisitedTransitions { get; }

    public abstract RandoContext Context { get; }
    public abstract IEnumerable<RandoPlacement> RandomizedPlacements { get; }
    public abstract IEnumerable<GeneralizedPlacement> VanillaPlacements { get; }

    /// <summary>
    /// Translates the input text into a target language.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public abstract string Localize(string text);

    public abstract string GetMapArea(string scene);

    /// <summary>
    /// Fetches the RandomizerCore placement from the tags of the AbstractItem.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public abstract RandoPlacement GetItemRandoPlacement(AbstractItem item);
}
