using System.Collections.ObjectModel;
using System.Diagnostics;
using RandoMapMod.Pathfinder.Instructions;
using RandoMapMod.Transition;
using RandomizerCore.Logic;
using RCPathfinder;
using RCPathfinder.Actions;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pathfinder
{
    internal static class Testing
    {
        private static readonly Random rng = new(0);

        internal static void LogProgressionData(RmmSearchData sd)
        {
            RandoMapMod.Instance?.LogDebug($"  Logging PMs");

            foreach (Term term in sd.Positions)
            {
                RandoMapMod.Instance.LogDebug($"    {term.Name}");
                RandoMapMod.Instance.LogDebug($"      Reference: {RM.RS.TrackerData.lm.Terms.IsTerm(term.Name) && RM.RS.TrackerData.pm.Has(term)}");
                RandoMapMod.Instance.LogDebug($"      Local: {sd.LocalPM.Has(term)}");
            }
        }

        internal static void DebugActions(RmmSearchData sd)
        {
            sd.UpdateProgression();

            foreach (KeyValuePair<Term, ReadOnlyCollection<AbstractAction>> kvp in sd.ActionLookup)
            {
                RandoMapMod.Instance?.LogDebug($"  Testing actions from {kvp.Key.Name}");

                foreach (AbstractAction action in kvp.Value)
                {
                    sd.LocalPM.SetState(kvp.Key, sd.CurrentState);

                    RandoMapMod.Instance?.LogDebug($"    {action.DebugName}, {action.Cost}: {action.TryDo(sd.LocalPM, kvp.Key, sd.CurrentState, out var _, out var _)}");

                    sd.LocalPM.SetState(kvp.Key, null);
                }
            }
        }

        internal static void SingleStartDestinationTest(RmmSearchData sd)
        {
            Term[] inLogicTransitions = sd.Positions.Where(p => TransitionData.GetTransitionDef(p.Name) is not null
                    && (RM.RS.TrackerData.pm.lm.GetTerm(p.Name) is null || RM.RS.TrackerData.pm.Has(p))).ToArray();

            sd.UpdateProgression();

            SearchParams sp = new
            (
                null,
                RmmPathfinder.SD.CurrentState,
                null,
                1000f,
                TerminationConditionType.Any
            );

            RandoMapMod.Instance?.LogDebug($"Starting SingleStartDestinationTest:");

            Stopwatch globalSW = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();

            int testCount = 100;
            int successes = 0;

            for (int i = 0; i < testCount; i++)
            {
                Term start = GetRandomTerm(inLogicTransitions);
                Term destination = GetRandomTerm(inLogicTransitions);

                sp.StartPositions = new StartPosition[] { new(start.Name, start, 0f) };
                sp.Destinations = new Term[] { destination };

                SearchState ss = new(sp);

                RandoMapMod.Instance?.LogDebug($"Trying {start} -> ? -> {destination}");

                sw.Restart();

                if (DoTest(sd, sp, ss))
                {
                    foreach (Instruction instruction in InstructionData.GetInstructions(ss.NewResultNodes[0]))
                    {
                        RandoMapMod.Instance?.LogDebug($"    {instruction.ArrowedText}");
                    }

                    successes++;
                }
                else
                {
                    RandoMapMod.Instance?.LogDebug($"{start} to {destination} failed");
                }

                sw.Stop();

                float localElapsedMS = sw.ElapsedTicks * 1000f / Stopwatch.Frequency;
                RandoMapMod.Instance?.LogDebug($"Explored {ss.NodesPopped} nodes in {localElapsedMS} ms. Average nodes/ms: {ss.NodesPopped / localElapsedMS}");
            }

            globalSW.Stop();

            float globalElapsedMS = globalSW.ElapsedTicks * 1000f / Stopwatch.Frequency;

            RandoMapMod.Instance?.LogDebug($"Total computation time: {globalElapsedMS} ms");
            RandoMapMod.Instance?.LogDebug($"Total successes: {successes}/{testCount}");
            RandoMapMod.Instance?.LogDebug($"Average serarch time: {globalElapsedMS / testCount} ms");
        }

        internal static void SceneToSceneTest(RmmSearchData sd)
        {
            TransitionTracker.Update();

            sd.UpdateProgression();

            SearchParams sp = new
            (
                null,
                RmmPathfinder.SD.CurrentState,
                null,
                1000f,
                TerminationConditionType.Any
            );

            RandoMapMod.Instance?.LogDebug($"Starting SceneToSceneTest:");

            Stopwatch globalSW = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();

            int testCount = 100;

            for (int i = 0; i < testCount; i++)
            {
                HashSet<Route> routes = new();

                string startScene = TransitionTracker.InLogicScenes.ElementAt(rng.Next(TransitionTracker.InLogicScenes.Count));
                string destScene = TransitionTracker.InLogicScenes.ElementAt(rng.Next(TransitionTracker.InLogicScenes.Count));

                sp.StartPositions = GetPrunedTransitions(sd, startScene).Select(t => new StartPosition(t.Name, t, 0f)).ToArray();
                sp.Destinations = GetPrunedTransitions(sd, destScene).ToArray();

                if (!sp.StartPositions.Any() || !sp.Destinations.Any())
                {
                    RandoMapMod.Instance?.LogDebug($"Invalid scenes {startScene} or {destScene}. Skipping");
                    continue;
                }

                //List<(string, Term)> benchStartWarps = BenchwarpInterop.BenchNames.Values
                //    .Where(sd.PositionLookup.ContainsKey)
                //    .Select(b => ("benchStart", sd.PositionLookup[b])).ToList();

                //if (sd.StartTerm is not null)
                //{
                //    benchStartWarps.Add(("benchStart", sd.StartTerm));
                //}

                //sp.StartPositions = sp.StartPositions.Concat(benchStartWarps).ToArray();

                SearchState ss = new(sp);

                RandoMapMod.Instance?.LogDebug($"Trying {startScene} -> ? -> {destScene}");

                sw.Restart();

                while (DoTest(sd, sp, ss))
                {
                    Route route = new(ss.NewResultNodes[0]);
                    if (routes.Contains(route))
                    {
                        RandoMapMod.Instance?.LogDebug($"    Duplicate route detected");
                    }
                    else
                    {
                        routes.Add(route);
                        foreach (Instruction instruction in route.RemainingInstructions)
                        {
                            RandoMapMod.Instance?.LogDebug($"    {instruction.ArrowedText}");
                        }
                    }
                }

                sw.Stop();

                float localElapsedMS = sw.ElapsedTicks * 1000f / Stopwatch.Frequency;
                RandoMapMod.Instance?.LogDebug($"Explored {ss.NodesPopped} nodes in {localElapsedMS} ms. Average nodes/ms: {ss.NodesPopped / localElapsedMS}");
            }

            globalSW.Stop();

            float globalElapsedMS = globalSW.ElapsedTicks * 1000f / Stopwatch.Frequency;

            RandoMapMod.Instance?.LogDebug($"Total computation time: {globalElapsedMS} ms");
            RandoMapMod.Instance?.LogDebug($"Average serarch time: {globalElapsedMS / testCount} ms");
        }

        /// <summary>
        /// Removes transitions that are immediately accesible from another transition in the same scene.
        /// </summary>
        private static Term[] GetPrunedTransitions(RmmSearchData sd, string scene)
        {
            if (!sd.TransitionTermsByScene.TryGetValue(scene, out var transitions)) return new Term[] { };

            SearchParams sp = new
            (
                transitions.Select(t => new StartPosition(t.Name, t, 0f)).ToArray(),
                sd.CurrentState,
                transitions.ToArray(),
                1f,
                TerminationConditionType.None
            );

            SearchState ss = new(sp);

            Algorithms.DijkstraSearch(sd, sp, ss);

            List<Node> nodes = new(ss.ResultNodes.Where(n => n.Depth > 0 && n.StartPosition != n.Actions.Last().Destination));

            HashSet<Term> prunedTransitions = new(transitions);

            foreach (Term transition in transitions)
            {
                if (!prunedTransitions.Contains(transition)) continue;

                foreach (Term transition2 in new List<Term>(prunedTransitions))
                {
                    if (nodes.Any(n => n.StartPosition == transition && n.Actions.Last().Destination == transition2))
                    {
                        prunedTransitions.Remove(transition2);
                    }
                }
            }

            return prunedTransitions.ToArray();
        }

        private static bool DoTest(SearchData sd, SearchParams sp, SearchState search)
        {
            if (Algorithms.DijkstraSearch(sd, sp, search))
            {
                RandoMapMod.Instance?.LogDebug($"  Success!");
                return true;
            }

            if (search.QueueNodes.Count > 0)
            {
                RandoMapMod.Instance?.LogDebug($"  Search terminated after reaching max cost.");
            }
            else
            {
                RandoMapMod.Instance?.LogDebug($"  Search exhausted with no route found.");
            }

            return false;
        }

        private static Term GetRandomTerm(Term[] terms)
        {
            return terms[rng.Next(terms.Length)];
        }
    }
}
