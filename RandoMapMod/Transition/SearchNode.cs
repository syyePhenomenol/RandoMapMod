using System.Collections.Generic;
using System.Linq;
using RandomizerCore.Logic;

namespace RandoMapMod.Transition
{
    internal class SearchNode
    {
        /// <summary>
        /// Starts with an empty route.
        /// </summary>
        internal SearchNode(string inTransition)
        {
            InTransition = inTransition;
            Scene = InTransition?.GetScene();
            Route = new();
        }

        /// <summary>
        /// Starts with the transition in the route.
        /// </summary>
        internal SearchNode(SearchNode parent, string outTransition)
        {
            InTransition = outTransition?.GetAdjacency();
            Scene = InTransition?.GetScene();

            if (parent is null)
            {
                Route = new() { outTransition };
            }
            else
            {
                Route = new(parent.Route);
                Route.Add(outTransition);
            }
        }

        /// <summary>
        /// Returns empty nodes (no route) with all non-special transitions as seeds.
        /// </summary>
        internal static SearchNode[] GetNodesFromScene(string scene)
        {
            if (!TransitionData.Scenes.TryGetValue(scene, out PathfinderScene ps))
            {
                RandoMapMod.Instance.LogWarn($"No PathfinderScene for {scene}");

                return new SearchNode[] { };
            }

            return ps.GetAllTransitions()
                .Where(transition => !transition.IsSpecialTransition())
                .Select(inTransition => new SearchNode(inTransition))
                .Where(node => node.Scene is not null)
                .ToArray();
        }

        /// <summary>
        /// Returns the next adjacent nodes with the new transition added to the route.
        /// </summary>
        internal SearchNode[] GetChildren(ProgressionManager pm, bool reorder = false)
        {
            if (!TransitionData.Scenes.TryGetValue(Scene, out PathfinderScene ps))
            {
                RandoMapMod.Instance.LogWarn($"No PathfinderScene for {Scene}");

                return new SearchNode[] { };
            }

            LinkedList<SearchNode> children = new(ps.GetReachableTransitions(pm, InTransition).Select(outTransition => new SearchNode(this, outTransition))
                .Where(node => node.InTransition is not null && node.Scene is not null));

            if (!reorder) return children.ToArray();

            // Reorder children so that doubling back is highest priority, then other special transitions
            List<SearchNode> priorityNodes = new();

            priorityNodes.AddRange(children.Where(n => n.Route.First().IsSpecialTransition()));
            priorityNodes.AddRange(children.Where(n => n.Route.First() == InTransition));

            foreach (SearchNode node in priorityNodes)
            {
                children.Remove(node);
                children.AddFirst(node);
            }

            return children.ToArray();
        }

        internal bool IsFollowingRejectedRoute(List<List<string>> rejectedRoutes)
        {
            if (rejectedRoutes is null || !rejectedRoutes.Any()) return false;

            return rejectedRoutes.Any(route => route.Count >= Route.Count && Route.Select((t, i) => new { t, i }).All(x => route[x.i] == x.t));
        }

        internal void PrintRoute()
        {
            string text = "Current route:";

            foreach (string outTransition in Route)
            {
                text += " -> " + outTransition;
            }

            RandoMapMod.Instance.LogDebug(text);
        }

        internal string InTransition { get; init; }
        internal string Scene { get; init; }
        internal List<string> Route { get; init; }
    }
}
