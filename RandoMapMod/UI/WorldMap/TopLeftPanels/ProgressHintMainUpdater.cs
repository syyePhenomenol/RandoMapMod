using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.Settings;
using static RandomizerMod.Settings.TrackerData;

namespace RandoMapMod.UI;

internal class ProgressHintMainUpdater : MainUpdater
{
    private readonly bool _terminateOnNewProgress = false;

    public ProgressHintMainUpdater(TrackerData td, ProgressionManager newPM)
        : base(td.lm, newPM)
    {
        // To avoid modifying TrackerData, keep a local copy to track what OOL stuff is added back in
        HashSet<int> localOOLObtainedItems = new(td.outOfLogicObtainedItems);
        HashSet<string> localOOLVisitedTransitions = new(td.outOfLogicVisitedTransitions);

        foreach (var w in td.lm.Waypoints)
        {
            if (w.term.Type == TermType.State)
            {
                AddEntry(new TrackedStateUpdateEntry(this, w.term, w.logic));
            }
            else
            {
                AddEntry(new TrackedPreplacedItemUpdateEntry(this, w, w));
            }
        }

        foreach (var t in newPM.lm.TransitionLookup.Values)
        {
            if (t.term.Type == TermType.State)
            {
                AddEntry(new TrackedStateUpdateEntry(this, t.term, t.logic));
            }
        }

        AddEntries(
            td.ctx.Vanilla.Select(v => new DelegateUpdateEntry(
                v.Location,
                pm =>
                {
                    pm.Add(v.Item, v.Location);

                    if (v.Location is ILocationWaypoint ilw)
                    {
                        // RandoMapMod.Instance.LogDebug($"Adding vanilla reachable effect: {v.Location.Name}");
                        pm.Add(ilw.GetReachableEffect());
                    }
                }
            ))
        );

        AddEntries(
            td.ctx.itemPlacements.Select(
                (p, id) =>
                    new DelegateUpdateEntry(
                        p.Location.logic,
                        (pm) =>
                        {
                            if (NewProgressFound)
                                return;

                            if (
                                _terminateOnNewProgress
                                && !td.clearedLocations.Contains(p.Location.Name)
                                && !td.previewedLocations.Contains(p.Location.Name)
                                && !td.uncheckedReachableLocations.Contains(p.Location.Name)
                            )
                            {
                                // RandoMapMod.Instance.LogDebug($"Unlocked new location: {p.Location.Name}");
                                NewProgressFound = true;
                                return;
                            }

                            (RandoItem item, RandoLocation location) = td.ctx.itemPlacements[id];
                            if (location is ILocationWaypoint ilw)
                            {
                                // RandoMapMod.Instance.LogDebug($"Adding reachable effect: {location.Name}");
                                pm.Add(ilw.GetReachableEffect());
                            }

                            if (localOOLObtainedItems.Remove(id))
                            {
                                // RandoMapMod.Instance.LogDebug($"Adding from OOL: {item.Name} at {location.Name}");
                                pm.Add(item, location);
                            }
                        }
                    )
            )
        );

        AddEntries(
            td.ctx.transitionPlacements.Select(
                (p, id) =>
                    new DelegateUpdateEntry(
                        p.Source,
                        (pm) =>
                        {
                            if (NewProgressFound)
                            {
                                return;
                            }

                            (RandoTransition target, RandoTransition source) = td.ctx.transitionPlacements[id];

                            if (
                                _terminateOnNewProgress
                                && !td.visitedTransitions.ContainsKey(source.Name)
                                && !td.uncheckedReachableTransitions.Contains(source.Name)
                            )
                            {
                                // RandoMapMod.Instance.LogDebug($"Unlocked new transition: {source.Name}");
                                NewProgressFound = true;
                                return;
                            }

                            if (!pm.Has(source.lt.term))
                            {
                                // RandoMapMod.Instance.LogDebug($"Adding transition reachable effect: {target.Name} at {source.Name}");
                                pm.Add(source.GetReachableEffect());
                            }

                            if (localOOLVisitedTransitions.Remove(source.Name))
                            {
                                // RandoMapMod.Instance.LogDebug($"Adding from OOL: {target.Name} at {source.Name}");
                                pm.Add(target, source);
                            }
                        }
                    )
            )
        );

        StartUpdating();

        foreach (var i in td.obtainedItems)
        {
            if (localOOLObtainedItems.Contains(i))
            {
                continue;
            }

            (var item, var loc) = td.ctx.itemPlacements[i];
            newPM.Add(item, loc);
        }

        foreach (var kvp in td.visitedTransitions)
        {
            if (localOOLVisitedTransitions.Contains(kvp.Key))
            {
                continue;
            }

            var tt = td.lm.GetTransitionStrict(kvp.Value);
            var st = td.lm.GetTransitionStrict(kvp.Key);

            if (!newPM.Has(st.term))
            {
                newPM.Add(st.GetReachableEffect());
            }

            newPM.Add(tt, st);
        }

        NewProgressFound = false;
        _terminateOnNewProgress = true;
    }

    public bool NewProgressFound { get; private set; } = false;

    private class TrackedStateUpdateEntry(ProgressHintMainUpdater mu, Term term, StateLogicDef logic)
        : StateUpdateEntry(term, logic)
    {
        private readonly List<State> _stateAccumulator = [];

        public override void Update(ProgressionManager pm, int updateTerm)
        {
            var state = pm.GetState(term);
            _stateAccumulator.Clear();
            if (logic.CheckForUpdatedState(pm, state, _stateAccumulator, updateTerm, out var newState))
            {
                stateSetter.value = newState;
                pm.Add(stateSetter);
                mu.NewProgressFound = true;
            }
        }

        public override void Update(ProgressionManager pm)
        {
            var state = pm.GetState(term);
            _stateAccumulator.Clear();
            if (logic.CheckForUpdatedState(pm, state, _stateAccumulator, out var newState))
            {
                stateSetter.value = newState;
                pm.Add(stateSetter);
                mu.NewProgressFound = true;
            }
            else if (state is null && logic.CanGet(pm))
            {
                stateSetter.value = pm.lm.StateManager.Empty;
                pm.Add(stateSetter);
                mu.NewProgressFound = true;
            }
        }
    }

    private class TrackedPreplacedItemUpdateEntry(ProgressHintMainUpdater mu, ILogicItem item, ILogicDef location)
        : PrePlacedItemUpdateEntry(item, location)
    {
        public override void OnAdd(ProgressionManager pm)
        {
            if (pm.lm.Terms.IsTerm(location.Name) && pm.Get(location.Name) is 0)
            {
                mu.NewProgressFound = true;
            }

            base.OnAdd(pm);
        }
    }
}
