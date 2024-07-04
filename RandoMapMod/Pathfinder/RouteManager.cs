using MapChanger;
using RandoMapMod.Pathfinder.Instructions;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandoMapMod.UI;
using RandomizerCore.Logic;
using RCPathfinder;

namespace RandoMapMod.Pathfinder
{
    internal class RouteManager : HookModule
    {
        private static SearchParams _sp;
        private static SearchState _ss;
        private static HashSet<Route> _routes;

        internal static Route CurrentRoute { get; private set; }
        internal static string StartScene { get; private set; }
        internal static string FinalScene { get; private set; }
        internal static bool Reevaluated { get; private set; }

        public override void OnEnterGame()
        {
            ItemChanger.Events.OnBeginSceneTransition += CheckRoute;
            MapChanger.Settings.OnSettingChanged += ResetRoute;
        }

        public override void OnQuitToMenu()
        {
            ResetRoute();

            ItemChanger.Events.OnBeginSceneTransition -= CheckRoute;
            MapChanger.Settings.OnSettingChanged -= ResetRoute;
        }

        internal static bool TryGetNextRouteTo(string scene)
        {
            RmmPathfinder.SD.UpdateProgression();

            // Reset
            if (!CanCycleRoute(scene))
            {
                StartScene = Utils.CurrentScene();
                FinalScene = scene;

                _sp = new()
                {
                    StartPositions = RmmPathfinder.SD.GetPrunedStartTerms(StartScene),
                    StartState = RmmPathfinder.SD.CurrentState,
                    Destinations = RmmPathfinder.SD.GetTransitionTerms(FinalScene),
                    MaxCost = 1000f,
                    MaxTime = 1000f,
                    TerminationCondition = TerminationConditionType.AnyUniqueStartAndDestination,
                    Stateless = _ss is not null && _ss.HasTimedOut,
                    DisallowBacktracking = false
                };

                if (Interop.HasBenchwarp() && RandoMapMod.GS.PathfinderBenchwarp)
                {
                    _sp.StartPositions = [.. _sp.StartPositions, .. GetBenchStartWarps()];
                }

                if (TryGetDreamGateStart(out var dreamGateStart))
                {
                    _sp.StartPositions = [.. _sp.StartPositions, dreamGateStart];
                }

                if (!_sp.StartPositions.Any() || !_sp.Destinations.Any())
                {
                    ResetRoute();
                    return false;
                }

                _ss = new(_sp);
                _routes = [];
            }

            Reevaluated = false;

            while (Algorithms.DijkstraSearch(RmmPathfinder.SD, _sp, _ss))
            {
                Route route = new(_ss.NewResultNodes[0]);

                if (!route.RemainingInstructions.Any() 
                    || route.RemainingInstructions.First().Text == route.RemainingInstructions.Last().TargetTransition 
                    || _routes.Contains(route)) continue;

                LogRouteResults(route);

                _routes.Add(route);
                CurrentRoute = route;
                RouteCompass.Update();
                UpdateRouteUI();
                return true;
            }

            // Search exhausted, clear search state and reset
            ResetRoute();
            return false;
        }

        internal static bool TryReevaluateRoute(ItemChanger.Transition transition)
        {
            RmmPathfinder.SD.UpdateProgression();

            StartScene = transition.SceneName;
            Term destination = CurrentRoute.Destination;

            _sp = new()
            {
                StartPositions = null,
                StartState = RmmPathfinder.SD.CurrentState,
                Destinations = [destination],
                MaxCost = 1000f,
                MaxTime = 1000f,
                TerminationCondition = TerminationConditionType.Any,
                DisallowBacktracking = false,
            };

            if (TransitionData.GetTransitionDef(transition.ToString()) is RmmTransitionDef td
                && RmmPathfinder.SD.PositionLookup.TryGetValue(td.Name, out Term start))
            {
                _sp.StartPositions = [new(start.Name, start, 0f)];
            }
            else
            {
                _sp.StartPositions = RmmPathfinder.SD.GetPrunedStartTerms(StartScene);
            }

            if (Interop.HasBenchwarp() && RandoMapMod.GS.PathfinderBenchwarp)
            {
                _sp.StartPositions = [.. _sp.StartPositions, .. GetBenchStartWarps(true)];
            }

            if (TryGetDreamGateStart(out var dreamGateStart, transition))
            {
                _sp.StartPositions = [.. _sp.StartPositions, dreamGateStart];
            }

            if (!_sp.StartPositions.Any() || !_sp.Destinations.Any())
            {
                ResetRoute();
                return false;
            }

            _ss = new(_sp);
            
            if (Algorithms.DijkstraSearch(RmmPathfinder.SD, _sp, _ss))
            {
                Route route = new(_ss.NewResultNodes[0]);

                if (!route.RemainingInstructions.Any())
                {
                    ResetRoute();
                    return false;
                }

                LogRouteResults(route);

                CurrentRoute = route;
                Reevaluated = true;
                return true;
            }

            ResetRoute();
            return false;
        }

        internal static void CheckRoute(ItemChanger.Transition lastTransition)
        {
            //RandoMapMod.Instance.LogDebug($"Last transition: {lastTransition}");

            if (CurrentRoute is null) return;

            Instruction instruction = CurrentRoute.RemainingInstructions.First();

            if (instruction.IsInProgress(lastTransition)) return;

            if (instruction.IsFinished(lastTransition))
            {
                CurrentRoute.RemainingInstructions.RemoveAt(0);
                if (!CurrentRoute.RemainingInstructions.Any())
                {
                    ResetRoute();
                    return;
                }
                UpdateRouteUI();
                return;
            }

            // The transition doesn't match the route
            switch (RandoMapMod.GS.WhenOffRoute)
            {
                case OffRouteBehaviour.Cancel:
                    ResetRoute();
                    break;
                case OffRouteBehaviour.Reevaluate:
                    TryReevaluateRoute(lastTransition);
                    UpdateRouteUI();
                    break;
                default:
                    break;
            }
        }

        internal static void ResetRoute()
        {
            CurrentRoute = null;
            StartScene = null;
            FinalScene = null;
            Reevaluated = false;
            _sp = null;
            _ss = null;
            _routes = null;

            RouteCompass.Update();
            UpdateRouteUI();

            RandoMapMod.Instance.LogFine("Reset route.");
        }

        private static void UpdateRouteUI()
        {
            RouteText.Instance?.Update();
            RouteSummaryText.Instance?.Update();
            RoomSelectionPanel.Instance?.Update();
        }

        internal static bool TryGetBenchwarpKey(out RmmBenchKey key)
        {
            if (CurrentRoute is not null && CurrentRoute.RemainingInstructions.First().IsOrIsSubclassInstanceOf<BenchwarpInstruction>())
            {
                key = ((BenchwarpInstruction)CurrentRoute.RemainingInstructions.First()).BenchKey;
                return true;
            }
            key = default;
            return false;
        }

        internal static bool CanCycleRoute(string scene)
        {
            return !Reevaluated
                && Utils.CurrentScene() == StartScene
                && scene == FinalScene
                && CurrentRoute is not null
                && CurrentRoute.RemainingInstructions.Count() == CurrentRoute.TotalInstructionCount;
        }

        /// <summary>
        /// May exclude a bench/start warp based on the transition and last respawn marker.
        /// </summary>
        private static List<StartPosition> GetBenchStartWarps(bool removeLastWarp = false)
        {
            RmmBenchKey key = new(Utils.CurrentScene(), PlayerData.instance.GetString("respawnMarkerName"));
            BenchwarpInterop.BenchNames.TryGetValue(key, out string lastWarp);

            List<StartPosition> benchStarts = BenchwarpInterop.GetVisitedBenchNames()
                    .Where(RmmPathfinder.SD.PositionLookup.ContainsKey)
                    .Select(b => new StartPosition("benchStart", RmmPathfinder.SD.PositionLookup[b], 1f)).ToList();

            if (removeLastWarp)
            {
                benchStarts.RemoveAll(b => b.Term.Name == lastWarp);
            }

            if (RmmPathfinder.SD.StartTerm is not null && (!removeLastWarp || lastWarp != BenchwarpInterop.BENCH_WARP_START))
            {
                benchStarts.Add(new("benchStart", RmmPathfinder.SD.StartTerm, 1f));
            }

            return benchStarts;
        }

        private static bool TryGetDreamGateStart(out StartPosition dreamGateStart, ItemChanger.Transition transition = default)
        {
            if (DreamgateTracker.DreamgateTiedTransition is null
                || (transition != default && transition.GateName is "dreamGate")
                || !RmmPathfinder.SD.PositionLookup.TryGetValue(DreamgateTracker.DreamgateTiedTransition, out Term term))
            {
                dreamGateStart = default;
                return false;
            }

            dreamGateStart = new("dreamGate", term, 1f);
            return true;
        }

        private static void LogRouteResults(Route route)
        {
            RandoMapMod.Instance.LogFine($"Found a route from {route.Node.StartPosition} to {route.Destination}:");
            RandoMapMod.Instance.LogFine(route.Node.PrintActionsShort());
            RandoMapMod.Instance.LogFine($"Node states count: {route.Node.CurrentStates.Count()}");
            RandoMapMod.Instance.LogFine($"Stateless search: {_sp?.Stateless}");
            RandoMapMod.Instance.LogFine($"Cumulative search time: {_ss?.SearchTime} ms");
        }
    }
}
