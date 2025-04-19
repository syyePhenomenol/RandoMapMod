using System.Collections.ObjectModel;
using MapChanger;
using RandoMapMod.Data;
using RandoMapMod.Modes;
using RandoMapMod.Transition;
using RCPathfinder;
using RCPathfinder.Actions;
using UnityEngine;

namespace RandoMapMod.Pathfinder;

internal class SceneLogicTracker
{
    private readonly RmmSearchData _sd;

    private HashSet<string> _inLogicScenes;
    private HashSet<string> _inLogicAdjacentScenes;
    private HashSet<string> _uncheckedReachableScenes;

    internal SceneLogicTracker(RmmSearchData sd)
    {
        _sd = sd;
        Update();
    }

    internal ReadOnlyCollection<string> InLogicScenes => new([.. _inLogicScenes]);

    internal void Events_OnQuickMap(GameMap _0, GlobalEnums.MapZone _1)
    {
        Update();
    }

    internal void Events_OnWorldMap(GameMap _)
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
            if (TransitionData.TryGetScene(position.Name, out var scene))
            {
                _ = _inLogicScenes.Add(scene);
            }
        }

        // Get in-logic adjacent scenes from transitions in SearchData connected by 1-cost actions
        // to current scene
        _inLogicAdjacentScenes = [.. GetVisitedAdjacentScenes(Utils.CurrentScene())];

        // Get scenes where there are unchecked reachable transitions
        foreach (var transition in RandoMapMod.Data.UncheckedReachableTransitions)
        {
            if (TransitionData.GetTransitionDef(transition) is RmcTransitionDef td)
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
        var color = RmmColors.GetColor(RmmColorSetting.Room_Out_of_logic);

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
        if (!_sd.TryGetInLogicPositionsFromScene(scene, out var inLogicPositions) || !inLogicPositions.Any())
        {
            return [];
        }

        SearchParams sp =
            new()
            {
                StartPositions = inLogicPositions.Select(_sd.GetNormalStartPosition),
                Destinations = [],
                MaxCost = 1f,
                MaxTime = 1000f,
                DisallowBacktracking = true,
            };

        SearchState ss = new(sp);
        _ = Algorithms.DijkstraSearch(_sd, sp, ss);

        HashSet<string> adjacentScenes = [];

        foreach (var node in ss.QueueNodes.Select(qn => qn.node).Concat(ss.TerminalNodes))
        {
            if (
                node.Actions.FirstOrDefault(a => a.Cost == 1f) is StandardAction action
                && TransitionData.TryGetScene(action.Target.Name, out var adjacentScene)
            )
            {
                _ = adjacentScenes.Add(adjacentScene);
            }
        }

        return adjacentScenes;
    }
}
