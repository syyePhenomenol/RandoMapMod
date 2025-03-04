using System.Collections.ObjectModel;
using RandoMapMod.Pathfinder.Actions;
using RandoMapMod.Transition;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RCPathfinder;
using RCPathfinder.Actions;
using JU = RandomizerCore.Json.JsonUtil;

namespace RandoMapMod.Pathfinder
{
    internal class RmmSearchData : SearchData
    {
        internal const string BENCHWARP = "Benchwarp";
        internal const string STARTWARP = "Start Warp";
        internal const string DREAMGATE = "Dreamgate";

        private static readonly string[] _infectionTransitions =
        [
            "Crossroads_03[bot1]",
            "Crossroads_06[right1]",
            "Crossroads_10[left1]",
            "Crossroads_19[top1]"
        ];
        
        internal SearchDataUpdater Updater { get; }

        // Only for terms with a single defined scene. Should not be using these to look up terms like
        // Can_Stag or Lower_Tram
        internal ReadOnlyDictionary<string, ReadOnlyCollection<Term>> PositionsByScene { get; }

        internal RmmSearchData(ProgressionManager reference) : base(reference)
        {            
            Dictionary<string, HashSet<Term>> positionsByScene = [];
            foreach (Term term in GetAllStateTerms())
            {
                if (TransitionData.TryGetScene(term.Name, out string scene))
                {
                    if (positionsByScene.TryGetValue(scene, out var positions))
                    {
                        positions.Add(term);
                        continue;
                    }

                    positionsByScene[scene] = [term];
                }
            }
            PositionsByScene = new(positionsByScene.ToDictionary(kvp => kvp.Key, kvp => new ReadOnlyCollection<Term>([..kvp.Value])));
            
            Updater = new(this);
            Updater.Update();
        }

        public override void UpdateProgression()
        {
            base.UpdateProgression();
            Updater.Update();
        }

        protected override LogicManagerBuilder MakeLocalLM(LogicManagerBuilder lmb)
        {
            lmb = base.MakeLocalLM(lmb);

            if (ReferencePM.ctx?.InitialProgression is not ProgressionInitializer pi || pi.StartStateTerm is not Term startTerm)
            {
                throw new NullReferenceException();
            }

            // Inject new terms and custom logic
            ILogicFormat fmt = new JsonLogicFormat();

            foreach (RawLogicDef rld in JU.DeserializeFromEmbeddedResource<RawLogicDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Logic.transitions.json"))
            {
                if (!lmb.IsTerm(rld.name)) lmb.AddTransition(rld);
            }

            lmb.DeserializeFile(LogicFileType.Waypoints, fmt, RandoMapMod.Assembly.GetManifestResourceStream("RandoMapMod.Resources.Pathfinder.Logic.waypoints.json"));

            foreach (RawLogicDef rld in JU.DeserializeFromEmbeddedResource<RawLogicDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Logic.edits.json"))
            {
                if (lmb.LogicLookup.ContainsKey(rld.name)) lmb.DoLogicEdit(rld);
            }
            foreach (RawSubstDef rsd in JU.DeserializeFromEmbeddedResource<RawSubstDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Logic.substitutions.json"))
            {
                if (lmb.LogicLookup.ContainsKey(rsd.name)) lmb.DoSubst(rsd);
            }

            // Remove Start_State from existing logic
            foreach (Term term in lmb.Terms)
            {
                if (lmb.LogicLookup.ContainsKey(term.Name)) lmb.DoSubst(new(term.Name, startTerm.Name, "NONE"));
            }

            // Link Start_State with start terms
            foreach (Term term in pi.StartStateLinkedTerms)
            {
                if (lmb.LogicLookup.ContainsKey(term.Name)) lmb.DoLogicEdit(new(term.Name, $"ORIG | {startTerm.Name}"));
            }

            return lmb;
        }

        protected override Dictionary<Term, List<StandardAction>> MakeStandardActions()
        {
            Dictionary<Term, List<StandardAction>> actions = [];

            var waypointActions = JU.DeserializeFromEmbeddedResource<WaypointActionDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.waypointActions.json")
                .ToDictionary(wad => (wad.Start, wad.Destination), wad => wad.Text);

            var _routeCompassOverrides = MapChanger.JsonUtil.DeserializeFromAssembly<Dictionary<string, Dictionary<string, string>>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Compass.routeCompassOverrides.json");

            // Logic-defined actions (both in-scene and waypoint jumps)
            foreach (var destination in GetAllStateTerms())
            {
                var logic = (DNFLogicDef)LocalPM.lm.GetLogicDefStrict(destination.Name);

                foreach (var start in logic.GetTerms().Where(t => t.Type is TermType.State))
                {
                    if (waypointActions.TryGetValue((start.Name, destination.Name), out string text))
                    {
                        _routeCompassOverrides.TryGetValue(start.Name, out Dictionary<string, string> compassObjects);
                        AddAction(new WaypointAction(start, destination, logic, text ?? start.Name, compassObjects));
                        continue;
                    }

                    AddAction(new InSceneAction(start, destination, logic));
                }
            }

            if (LocalPM.ctx is null) return actions;

            bool vanillaInfectionTransitions = _infectionTransitions.All(TransitionData.IsVanillaTransition);

            // Transitions
            foreach ((TransitionDef sourceDef, TransitionDef targetDef) in TransitionData.GetPlacements())
            {
                if (!StateTermLookup.TryGetValue(sourceDef.Name, out Term sourceTerm)
                    || !StateTermLookup.TryGetValue(targetDef.Name, out Term targetTerm))
                {
                    RandoMapMod.Instance.LogWarn($"One of the terms {sourceDef.Name} and/or {targetDef.Name} does not exist in logic");
                    continue;
                }

                TransitionAction ta;
                if (_routeCompassOverrides.TryGetValue(sourceDef.Name, out Dictionary<string, string> compassObjects))
                {
                    ta = new(sourceTerm, targetTerm, compassObjects);
                }
                else
                {
                    ta = new(sourceTerm, targetTerm, new() { { sourceDef.SceneName, sourceDef.DoorName } });
                }

                if (vanillaInfectionTransitions && _infectionTransitions.Contains(ta.Source.Name))
                {
                    ta = new InfectionTransitionAction(ta);
                }

                AddAction(ta);
            }

            return actions;

            void AddAction(StandardAction a)
            {
                if (actions.TryGetValue(a.Source, out var actionList))
                {
                    actionList.Add(a);
                    return;
                }

                actions[a.Source] = [a];
            }
        }

        protected override IEnumerable<StartJumpAction> MakeStartJumpActions()
        {
            List<StartJumpAction> actions = [];

            if (Interop.HasBenchwarp)
            {
                if (LocalPM.ctx?.InitialProgression is ProgressionInitializer pi && pi.StartStateTerm is Term startTerm)
                {
                    actions.Add(new StartWarpAction(startTerm));
                }

                foreach (KeyValuePair<string, RmmBenchKey> kvp in BenchwarpInterop.BenchKeys)
                {
                    if (StateTermLookup.TryGetValue(kvp.Key, out Term benchTerm))
                    {
                        actions.Add(new BenchwarpAction(benchTerm, kvp.Value));
                    }
                }
            }

            actions.Add(new DreamgateAction(this));

            return actions;
        }

        internal IEnumerable<Term> GetPrunedPositionsFromScene(string scene)
        {
            if (!TryGetInLogicPositionsFromScene(scene, out IEnumerable<Term> inLogicPositions) || !inLogicPositions.Any())
            {
                return [];
            }

            // Prune positions that are reachable from another in the same scene (two-way).
            SearchParams sp = new()
            {
                StartPositions = inLogicPositions.Select(GetNormalStartPosition),
                Destinations = inLogicPositions,
                MaxCost = 0f,
                MaxDepth = 10,
                MaxTime = 1000f,
                DisallowBacktracking = false,
            };

            SearchState ss = new(sp);
            Algorithms.DijkstraSearch(RmmPathfinder.SD, sp, ss);
            IEnumerable<Node> resultNodes = ss.ResultNodes.Where(n => n.Depth > 0 && n.Start.Term != n.Current.Term);

            List<Term> newPositions = new(inLogicPositions);

            foreach (Node node in resultNodes)
            {
                if (!newPositions.Contains(node.Start.Term) || !newPositions.Contains(node.Current.Term))
                {
                    continue;
                }

                if (resultNodes.Any(n => node.Start.Term == n.Current.Term
                    && node.Current.Term == n.Start.Term
                    && StateUnion.IsProgressivelyLE(node.Start.States, node.Current.States)
                    && StateUnion.IsProgressivelyLE(n.Start.States, n.Current.States)))
                {
                    RandoMapMod.Instance.LogFine($"Pruning {node.Current.Term} which is equivalent to {node.Start.Term}");
                    newPositions.Remove(node.Current.Term);
                }
            }

            RandoMapMod.Instance.LogFine($"Remaining: {string.Join(", ", newPositions.Select(p => p.Name))}");

            return newPositions;
        }

        internal bool TryGetInLogicPositionsFromScene(string scene, out IEnumerable<Term> inLogicPositions)
        {
            if (PositionsByScene.TryGetValue(scene, out ReadOnlyCollection<Term> positions))
            {
                inLogicPositions = positions.Where(p => LocalPM.Has(p));
                return true;
            }

            inLogicPositions = default;
            return false;
        }

        internal Position GetNormalStartPosition(Term term)
        {
            return new Position(term, Updater.CurrentState, 0f);
        }
    }
}
