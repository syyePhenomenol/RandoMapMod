using MapChanger;
using RandoMapMod.Settings;
using RandoMapMod.UI;
using RandomizerCore.Logic;
using RCPathfinder;
using RCPathfinder.Actions;
using JU = RandomizerCore.Json.JsonUtil;

namespace RandoMapMod.Pathfinder
{
    internal class RouteManager
    {
        private readonly RmmSearchData _sd;
        private readonly Dictionary<(string, string), RouteHint> _routeHints;
        internal Route CurrentRoute { get; private set; }
        private SearchParams _sp;
        private SearchState _ss;
        private HashSet<Route> _routes;
        private string _startScene;
        private string _finalScene;
        private bool _reevaluated;

        internal RouteManager(RmmSearchData sd)
        {
            _sd = sd;
            _routeHints = JU.DeserializeFromEmbeddedResource<RouteHint[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.routeHints.json")
                .ToDictionary(rh => (rh.Start, rh.Destination), rh => rh);

            ItemChanger.Events.OnBeginSceneTransition += CheckRoute;
            MapChanger.Settings.OnSettingChanged += ResetRoute;
        }

        ~RouteManager()
        {
            ItemChanger.Events.OnBeginSceneTransition -= CheckRoute;
            MapChanger.Settings.OnSettingChanged -= ResetRoute;
        }

        internal bool CanCycleRoute(string scene)
        {
            return !_reevaluated
                && Utils.CurrentScene() == _startScene
                && scene == _finalScene
                && CurrentRoute is not null
                && CurrentRoute.NotStarted;
        }

        internal bool TryGetNextRouteTo(string scene)
        {
            _sd.UpdateProgression();

            // Reset
            if (!CanCycleRoute(scene))
            {
                _startScene = Utils.CurrentScene();
                _finalScene = scene;

                _sp = new()
                {
                    // Give start jump actions higher priority
                    StartPositions = _sd.GetPrunedPositionsFromScene(_startScene).Select(_sd.GetNormalStartPosition)
                        .Concat([new ArbitraryPosition(_sd.Updater.CurrentState, -0.5f)]),
                    Destinations = _sd.GetPrunedPositionsFromScene(_finalScene),
                    MaxCost = 100f,
                    MaxTime = 1000f,
                    TerminationCondition = TerminationConditionType.Any,
                    Stateless = _ss is not null && _ss.HasTimedOut,
                    DisallowBacktracking = false
                };

                if (!_sp.StartPositions.Any() || !_sp.Destinations.Any())
                {
                    ResetRoute();
                    return false;
                }

                _ss = new(_sp);
                _routes = [];
            }

            _reevaluated = false;

            while (Algorithms.DijkstraSearch(_sd, _sp, _ss))
            {
                Route route = GetRoute(_ss.NewResultNodes.First());

                if (route.FinishedOrEmpty
                    // || route.FirstInstruction.StartText == route.LastInstruction.DestinationText
                    || _routes.Any(r => r.FirstInstruction == route.FirstInstruction && r.LastInstruction == route.LastInstruction))
                {
                    RandoMapMod.Instance.LogFine($"Redundant route: {route.Node.DebugString}");
                    continue;
                }
                
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

        private void CheckRoute(ItemChanger.Transition lastTransition)
        {
            RandoMapMod.Instance.LogFine($"Last transition: {lastTransition}");

            if (!_sd.LocalPM.lm.Terms.IsTerm(lastTransition.ToString()))
            {
                RandoMapMod.Instance.LogFine("Transition not in PM");
            }

            if (CurrentRoute is null) return;

            if (CurrentRoute.CheckCurrentInstruction(lastTransition))
            {
                if (CurrentRoute.FinishedOrEmpty) ResetRoute();
                UpdateRouteUI();
                return;
            }

            // The transition doesn't match the route's instruction
            switch (RandoMapMod.GS.WhenOffRoute)
            {
                case OffRouteBehaviour.Cancel:
                    ResetRoute();
                    break;
                case OffRouteBehaviour.Reevaluate:
                    ReevaluateRoute(lastTransition);
                    UpdateRouteUI();
                    break;
                default:
                    break;
            }
        }

        private void ReevaluateRoute(ItemChanger.Transition transition)
        {
            _sd.UpdateProgression();

            _startScene = transition.SceneName;
            Term destination = CurrentRoute.Node.Current.Term;

            IEnumerable<Position> startPositions;
            if (TryGetStartPosition(transition, out Position start))
            {
                startPositions = [start];
            }
            else
            {
                startPositions = _sd.GetPrunedPositionsFromScene(Utils.CurrentScene()).Select(_sd.GetNormalStartPosition);
            }

            _sp = new()
            {
                // Give start jump actions lower priority
                StartPositions = startPositions.Concat([new ArbitraryPosition(_sd.Updater.CurrentState, 0.5f)]),
                Destinations = [destination],
                MaxCost = 100f,
                MaxTime = 1000f,
                TerminationCondition = TerminationConditionType.Any,
                DisallowBacktracking = false
            };

            if (!_sp.StartPositions.Any() || !_sp.Destinations.Any())
            {
                ResetRoute();
                return;
            }

            _ss = new(_sp);
            
            if (Algorithms.DijkstraSearch(_sd, _sp, _ss))
            {
                // RandoMapMod.Instance.LogFine($"{_ss.NewResultNodes.First().DebugString}");

                Route route = GetRoute(_ss.NewResultNodes.First());

                if (route.FinishedOrEmpty)
                {
                    ResetRoute();
                    return;
                }

                LogRouteResults(route);

                CurrentRoute = route;
                _reevaluated = true;
                return;
            }

            ResetRoute();
        }

        internal void ResetRoute()
        {
            CurrentRoute = null;
            _sp = null;
            _ss = null;
            _routes = null;
            _startScene = null;
            _finalScene = null;
            _reevaluated = false;

            RouteCompass.Update();
            UpdateRouteUI();

            RandoMapMod.Instance.LogFine("Reset route.");
        }

        private void UpdateRouteUI()
        {
            RouteText.Instance?.Update();
            RouteSummaryText.Instance?.Update();
            RoomSelectionPanel.Instance?.Update();
        }

        private bool TryGetStartPosition(ItemChanger.Transition transition, out Position start)
        {
            if (_sd.StateTermLookup.TryGetValue(transition.ToString(), out Term position)
                // Try get last benchwarp
                || (Interop.HasBenchwarp && transition.GateName is ""
                    && BenchwarpInterop.TryGetLastWarp(out string benchName, out RmmBenchKey _)
                    && _sd.StateTermLookup.TryGetValue(benchName, out position)))
            {
                if (_sd.LocalPM.Has(position))
                {
                    start = _sd.GetNormalStartPosition(position);
                    return true;
                }

                RandoMapMod.Instance.LogFine($"Exited out of transition {transition} that is not in logic");
                start = null;
                return false;
            }

            RandoMapMod.Instance.LogFine($"Exited out of unrecognized transition {transition}");
            start = null;
            return false;
        }

        private Route GetRoute(Node node)
        {
            List<RouteHint> routeHints = [];
            foreach (StandardAction a in node.Actions.Where(a => a is StandardAction).Cast<StandardAction>())
            {
                if (_routeHints.TryGetValue((a.Source.Name, a.Target.Name), out RouteHint rh))
                {
                    routeHints.Add(rh);
                }
            }

            return new Route(node, routeHints.Distinct());
        }

        private void LogRouteResults(Route route)
        {
            RandoMapMod.Instance.LogFine($"Found a route from {route.Node.Start.Term} to {route.Node.Current.Term}:");
            RandoMapMod.Instance.LogFine(route.Node.DebugString);
            RandoMapMod.Instance.LogFine($"Node states count: {route.Node.Current.States.Count()}");
            RandoMapMod.Instance.LogFine($"Stateless search: {_sp?.Stateless}");
            RandoMapMod.Instance.LogFine($"Cumulative search time: {_ss?.SearchTime} ms");
        }
    }
}
