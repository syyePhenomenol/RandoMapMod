using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore.Logic;
using PD = RandoMapMod.Transition.PathfinderData;

namespace RandoMapMod.Transition
{
    internal static class PathfinderExtensions
    {
        internal static HashSet<string> GetTransitionsInScene(this string scene)
        {
            HashSet<string> transitions = TransitionData.GetTransitionsByScene(scene);

            if (PD.TransitionsByScene.TryGetValue(scene, out HashSet<string> specialTransitions))
            {
                transitions.UnionWith(specialTransitions);
            }

            return transitions;
        }

        internal static string GetScene(this string transition)
        {
            if (TransitionData.IsInTransitionLookup(transition))
            {
                return TransitionData.GetScene(transition);
            }
            else if (PD.ScenesByTransition.ContainsKey(transition))
            {
                return PD.ScenesByTransition[transition];
            }
            else if (transition.Contains("["))
            {
                return transition.Split('[')[0];
            }

            return null;
        }

        // Returns the correct adjacent scene for special transitions
        internal static string GetAdjacentScene(this string transition)
        {
            if (PD.AdjacentTerms.ContainsKey(transition))
            {
                return PD.AdjacentScenes[transition];
            }

            return transition.GetAdjacentTerm().GetScene();
        }

        internal static string GetAdjacentTerm(this string transition)
        {
            if (transition.IsSpecialTransition())
            {
                return PD.AdjacentTerms[transition];
            }

            // Some top transitions don't have an adjacent transition
            if (TransitionData.IsInTransitionLookup(transition))
            {
                return TransitionData.GetAdjacentTransition(transition);
            }

            RandoMapMod.Instance.LogWarn($"No adjacent term for {transition}");

            return null;
        }

        internal static bool IsSpecialTransition(this string transition)
        {
            return PD.AdjacentTerms.ContainsKey(transition) || transition.IsBenchwarpTransition();
        }

        internal static bool IsBenchwarpTransition(this string transition)
        {
            return BenchwarpInterop.BenchKeys.ContainsKey(transition);
        }

        internal static bool IsStagTransition(this string transition)
        {
            return transition.IsSpecialTransition() && transition.StartsWith("Stag");
        }

        internal static bool IsElevatorTransition(this string transition)
        {
            return transition.IsSpecialTransition() && (transition.StartsWith("Left_Elevator") || transition.StartsWith("Right_Elevator"));
        }

        internal static bool IsTramTransition(this string transition)
        {
            return transition.IsSpecialTransition() && (transition.StartsWith("Lower_Tram") || transition.StartsWith("Upper_Tram"));
        }

        internal static bool IsWarpTransition(this string transition)
        {
            return transition.IsSpecialTransition() && transition.Contains("[warp]");
        }

        internal static bool TryGetSceneWaypoint(this string scene, out LogicWaypoint waypoint)
        {
            if (PD.WaypointScenes.ContainsKey(scene))
            {
                waypoint = PD.WaypointScenes[scene];
                return true;
            }

            waypoint = null;
            return false;
        }
    }
}
