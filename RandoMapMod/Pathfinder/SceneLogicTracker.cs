using System.Collections.ObjectModel;
using MapChanger;
using RandoMapMod.Modes;
using RandoMapMod.Transition;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RCPathfinder;
using RCPathfinder.Actions;
using UnityEngine;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pathfinder
{
    internal class SceneLogicTracker
    {
        private readonly RmmSearchData _sd;
        internal ReadOnlyCollection<string> InLogicScenes => new([.._inLogicScenes]);
        private HashSet<string> _inLogicScenes;
        private HashSet<string> _inLogicAdjacentScenes;
        private HashSet<string> _uncheckedReachableScenes;

        internal SceneLogicTracker(RmmSearchData sd)
        {
            _sd = sd;
            Update();
            Events.OnWorldMap += Events_OnWorldMap;
            Events.OnQuickMap += Events_OnQuickMap;
        }

        ~SceneLogicTracker()
        {
            Events.OnWorldMap -= Events_OnWorldMap;
            Events.OnQuickMap -= Events_OnQuickMap;
        }

        private void Events_OnQuickMap(GameMap arg1, GlobalEnums.MapZone arg2)
        {
            Update();
        }

        private void Events_OnWorldMap(GameMap obj)
        {
            Update();
        }

        private void Update()
        {
            _inLogicScenes = [];
            _inLogicAdjacentScenes = [];
            _uncheckedReachableScenes = [];

            _sd.UpdateProgression();

            // Get in-logic scenes from in-logic terms in SearchData
            foreach (var position in _sd.GetAllStateTerms().Where(p => _sd.LocalPM.Has(p)))
            {
                if (TransitionData.TryGetScene(position.Name, out string scene))
                {
                    _inLogicScenes.Add(scene);
                }
            }

            // Get in-logic adjacent scenes from transitions in SearchData connected by 1-cost actions
            // to current scene
            _inLogicAdjacentScenes = [..GetVisitedAdjacentScenes(Utils.CurrentScene())];

            // Get scenes where there are unchecked reachable transitions
            foreach (string transition in RM.RS.TrackerData.uncheckedReachableTransitions)
            {
                if (TransitionData.GetTransitionDef(transition) is TransitionDef td)
                {
                    _uncheckedReachableScenes.Add(td.SceneName);
                }
            }
        }

        internal bool GetRoomActive(string scene)
        {
            if (MapChanger.Settings.CurrentMode() is TransitionNormalMode)
            {
                return Tracker.HasVisitedScene(scene) || _inLogicScenes.Contains(scene);
            }

            if (MapChanger.Settings.CurrentMode() is TransitionVisitedOnlyMode)
            {
                return Tracker.HasVisitedScene(scene);
            }

            return true;
        }

        internal Vector4 GetRoomColor(string scene)
        {
            Vector4 color = RmmColors.GetColor(RmmColorSetting.Room_Out_of_logic);

            if (_inLogicScenes.Contains(scene))
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Normal);
            }

            if (_inLogicAdjacentScenes.Contains(scene))
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Adjacent);
            }

            if (scene == Utils.CurrentScene())
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Current);
            }

            if (_uncheckedReachableScenes.Contains(scene))
            {
                color.w = 1f;
            }

            return color;
        }

        internal bool IsInLogicScene(string scene)
        {
            return _inLogicScenes.Contains(scene);
        }

        private IEnumerable<string> GetVisitedAdjacentScenes(string scene)
        {
            if (!_sd.TryGetInLogicPositionsFromScene(scene, out IEnumerable<Term> inLogicPositions) || !inLogicPositions.Any())
            {
                return [];
            }

            SearchParams sp = new()
            {
                StartPositions = inLogicPositions.Select(_sd.GetNormalStartPosition),
                Destinations = [],
                MaxCost = 1f,
                MaxTime = 1000f,
                DisallowBacktracking = true
            };

            SearchState ss = new(sp);
            Algorithms.DijkstraSearch(RmmPathfinder.SD, sp, ss);

            HashSet<string> adjacentScenes = [];

            foreach (var node in ss.QueueNodes.Select(qn => qn.node).Concat(ss.TerminalNodes))
            {
                if (node.Actions.FirstOrDefault(a => a.Cost == 1f) is StandardAction action
                    && TransitionData.TryGetScene(action.Target.Name, out string adjacentScene))
                {
                    adjacentScenes.Add(adjacentScene);
                }
            }

            return adjacentScenes;
        }
    }
}
