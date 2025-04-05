using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using UnityEngine;

namespace RandoMapMod.UI;

public class RouteCompassInfo : CompassInfo
{
    public override string Name => "Route Compass";
    public override float Radius => 1.5f;
    public override float Scale => 2.0f;
    public override bool RotateSprite => true;
    public override bool Lerp => false;
    public override float LerpDuration => 0.5f;

    public override bool ActiveCondition()
    {
        return Conditions.TransitionRandoModeEnabled() && RandoMapMod.GS.ShowRouteCompass;
    }

    public override GameObject GetEntity()
    {
        return HeroController.instance?.gameObject;
    }

    internal void UpdateCompassTarget()
    {
        CompassTargets.Clear();

        if (
            RmmPathfinder.RM.CurrentRoute is Route route
            && route.CurrentInstruction.CompassObjectPaths is Dictionary<string, string> paths
            && paths.TryGetValue(Utils.CurrentScene(), out var goPath)
        )
        {
            CompassTargets.Add(new TransitionCompassTarget(goPath));
        }
    }
}
