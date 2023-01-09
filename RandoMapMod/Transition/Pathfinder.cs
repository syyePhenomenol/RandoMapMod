using System.Collections.Generic;
using System.Linq;
using MapChanger;
using RandoMapMod.Modes;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class Pathfinder : HookModule
    {
        internal static StateUnion AnyState => RmmPM.lm.StateManager.DefaultStateSingleton;
        internal static ProgressionManager RmmPM { get; private set; }

        private static (string, string)[] conditionalTerms;

        public override void OnEnterGame()
        {
            // Helps with preventing throws to load them in like this
            conditionalTerms = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.conditionalTerms.json")
                .Where(kvp => RM.RS.Context.LM.Terms.TermLookup.ContainsKey(kvp.Key) && RM.RS.Context.LM.Terms.TermLookup.ContainsKey(kvp.Value))
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToArray();

            RandoMapMod.Instance.LogFine("Loaded conditional terms:");
            foreach ((string, string) pair in conditionalTerms)
            {
                RandoMapMod.Instance.LogFine(pair);
            }

            RmmPM = new(TransitionData.LM, RM.RS.Context);

            // Remove start terms
            foreach (string term in TransitionData.GetStartTerms())
            {
                term.RemoveFrom(RmmPM);
            }

            UpdateProgression();
            GetFullNetwork();

            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate += UpdateProgression;
        }

        public override void OnQuitToMenu()
        {
            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate -= UpdateProgression;
        }

        internal static void UpdateProgression()
        {
            foreach (Term term in RM.RS.TrackerData.lm.Terms)
            {
                if (!term.Name.IsTransition() && !term.Name.IsScene())
                {
                    if (Term.GetTermType(term) is TermType.State)
                    {
                        term.Name.SetToState(RmmPM, RM.RS.TrackerData.pm.GetState(term));
                    }
                    else
                    {
                        RmmPM.Set(term, RM.RS.TrackerData.pm.Get(term));
                    }
                }
            }

            // Emulate a transition being possibly available via having the required term
            foreach ((string ifTerm, string thenTerm) in conditionalTerms)
            {
                if (ifTerm.IsIn(RM.RS.TrackerData.pm))
                {
                    thenTerm.AddTo(RmmPM);
                }
            }

            RmmPM.Set("Town_Lift_Activated", PlayerData.instance.GetBool("mineLiftOpened") ? 1 : 0);
            RmmPM.Set("Opened_Shaman_Pillar", PlayerData.instance.GetBool("shamanPillar") ? 1 : 0);
            RmmPM.Set("Opened_Mawlek_Wall", PlayerData.instance.GetBool("crossroadsMawlekWall") ? 1 : 0);
            RmmPM.Set("Opened_Dung_Defender_Wall", PlayerData.instance.GetBool("dungDefenderWallBroken") ? 1 : 0);

            foreach (PersistentBoolData pbd in SceneData.instance.persistentBoolItems)
            {
                if (pbd.sceneName is "Waterways_02" && pbd.id is "Quake Floor (1)")
                {
                    RmmPM.Set("Broke_Waterways_Bench_Ceiling", pbd.activated ? 1 : 0);
                }
                else if (pbd.sceneName is "Ruins1_31" && pbd.id is "Breakable Wall Ruin Lift")
                {
                    RmmPM.Set("City_Toll_Wall_Broken", pbd.activated ? 1 : 0);
                }
                else if (pbd.sceneName is "Ruins1_31" && pbd.id is "Ruins Lever")
                {
                    RmmPM.Set("Lever-Shade_Soul", pbd.activated ? 1 : 0);
                }
            }

            foreach (PersistentIntData pid in SceneData.instance.persistentIntItems)
            {
                if (pid.sceneName is "Ruins1_31" && pid.id is "Ruins Lift")
                {
                    RmmPM.Set("City_Toll_Elevator_Up", pid.value % 2 is 1 ? 1 : 0);
                    RmmPM.Set("City_Toll_Elevator_Down", pid.value % 2 is 0 ? 1 : 0);
                }
            }
        }

        internal static List<string> GetRoute(string startScene, string finalScene, List<List<string>> rejectedRoutes)
        {
            if (startScene is null || finalScene is null) return new();

            HashSet<string> visitedTransitions = new();
            LinkedList<SearchNode> queue = new();

            // Initialize queue. Prioritise benches over other transitions
            if (RandoMapMod.GS.PathfinderBenchwarp && Interop.HasBenchwarp())
            {
                foreach (string benchName in BenchwarpInterop.GetVisitedBenchNames())
                {
                    TryAddNode(queue, visitedTransitions, new SearchNode(null, benchName), rejectedRoutes);
                }
            }

            TryAddDreamGate(queue, visitedTransitions, rejectedRoutes);

            foreach (SearchNode node in SearchNode.GetNodesFromScene(startScene))
            {
                if (IsVanillaOrCheckedTransition(node.InTransition))
                {
                    foreach (SearchNode child in node.GetChildren(RmmPM))
                    {
                        TryAddNode(queue, visitedTransitions, child, rejectedRoutes);
                    }
                }
            }

            // Traverse search space
            while (queue.Any())
            {
                SearchNode parent = queue.First();
                queue.RemoveFirst();

                // Avoid terminating on duplicate/redudant new paths
                if (parent.Scene == finalScene && !rejectedRoutes.Any(r => r.First() == parent.Route.First() && r.Last() == parent.Route.Last()))
                {
                    // For conciseness, no other paths to same final transition with a different starting benchwarp
                    if (parent.Route.First().IsBenchwarpTransition()
                        && rejectedRoutes.Any(r => r.Last() == parent.Route.Last() && r.First().IsBenchwarpTransition())) continue;

                    parent.PrintRoute();
                    return parent.Route;
                }

                foreach (SearchNode child in parent.GetChildren(RmmPM))
                {
                    TryAddNode(queue, visitedTransitions, child, rejectedRoutes);
                }
            }

            return new();
        }

        // The start transition and final transition are both in-transitions.
        internal static List<string> GetRoute(string startTransition, string finalTransition)
        {
            if (startTransition is null || finalTransition is null || startTransition == finalTransition) return new();

            HashSet<string> visitedTransitions = new();
            LinkedList<SearchNode> queue = new();

            // Initialize queue. Prioritise doubling back, followed by other transitions, followed by benches
            SearchNode start = new(startTransition);
            if (start.InTransition is not null && start.Scene is not null)
            {
                foreach (SearchNode child in start.GetChildren(RmmPM, true))
                {
                    TryAddNode(queue, visitedTransitions, child);
                }
            }

            TryAddDreamGate(queue, visitedTransitions);

            if (RandoMapMod.GS.PathfinderBenchwarp && Interop.HasBenchwarp())
            {
                foreach (string outTransition in BenchwarpInterop.GetVisitedBenchNames())
                {
                    TryAddNode(queue, visitedTransitions, new SearchNode(null, outTransition));
                }
            }

            // Traverse search space
            while (queue.Any())
            {
                SearchNode parent = queue.First();
                queue.RemoveFirst();

                if (parent.InTransition == finalTransition)
                {
                    parent.PrintRoute();
                    return parent.Route;
                }

                foreach (SearchNode child in parent.GetChildren(RmmPM))
                {
                    TryAddNode(queue, visitedTransitions, child);
                }
            }

            return new();
        }

        internal static HashSet<string> GetAdjacentReachableScenes(string scene)
        {
            HashSet<string> scenes = new();

            foreach (SearchNode node in SearchNode.GetNodesFromScene(scene))
            {
                if (IsVanillaOrCheckedTransition(node.InTransition))
                {
                    foreach (SearchNode child in node.GetChildren(RmmPM))
                    {
                        string outTransition = child.Route.First();

                        if (outTransition.IsInfectedTransition()) continue;

                        if (IsVanillaOrCheckedTransition(outTransition) || outTransition.IsSpecialTransition())
                        {
                            scenes.Add(outTransition.GetAdjacentScene());
                        }
                    }
                }
            }

            return scenes;
        }
        
        /// <summary>
        /// Logs which transitions are logically reachable from each transition within a room.
        /// </summary>
        internal static void GetFullNetwork()
        {
            RandoMapMod.Instance.LogFine("Full pathfinder network:");

            foreach (string inTransition in TransitionData.AdjacencyLookup.Values.OrderBy(t => t))
            {
                RandoMapMod.Instance.LogFine(inTransition);

                if (inTransition.GetScene() is string scene && TransitionData.Scenes.TryGetValue(scene, out PathfinderScene ps))
                {
                    string[] allTransitions = ps.GetAllTransitions();
                    string[] reachableTransitions = ps.GetReachableTransitions(RmmPM, inTransition);

                    foreach (string outTransition in allTransitions)
                    {
                        if (reachableTransitions.Contains(outTransition))
                        {
                            RandoMapMod.Instance.LogFine($"---> {outTransition}");
                        }
                        else
                        {
                            RandoMapMod.Instance.LogFine($"-x-> {outTransition}");
                        }
                    }
                }
            }
        }

        private static void TryAddDreamGate(LinkedList<SearchNode> queue, HashSet<string> traversedTransitions, List<List<string>> rejectedRoutes = null)
        {
            if (DreamgateTracker.DreamgateTiedTransition is not null)
            {
                SearchNode node = new(null, DreamgateTracker.DREAMGATE);

                queue.AddLast(node);

                // Allow other routes to go through a rejected route
                if (node.IsFollowingRejectedRoute(rejectedRoutes)) return;

                traversedTransitions.Add(DreamgateTracker.DREAMGATE);
            }
        }

        private static void TryAddNode(LinkedList<SearchNode> queue, HashSet<string> traversedTransitions, SearchNode node, List<List<string>> rejectedRoutes = null)
        {
            if (!node.Route.Any())
            {
                RandoMapMod.Instance.LogWarn($"There needs to be at least one transition in the route to add it to the queue. In-transition: {node.InTransition}");
            }

            string outTransition = node.Route.Last();

            // Don't add transitions already visited (unless bypassed by repeating route)
            if (traversedTransitions.Contains(outTransition)) return;

            // Don't add transitions already visited in current route
            if (node.Route.Where(t => t == outTransition).Count() > 1) return;

            // Don't add out-transitions connecting to in-transitions already visited in current route
            // Exclude benchwarps since their adjacencies are themselves
            if (!node.InTransition.IsBenchwarpTransition() && node.Route.Any(t => t == node.InTransition)) return;

            // Don't add transitions into rooms not displayed in this mode
            if (MapChanger.Settings.CurrentMode() is TransitionVisitedOnlyMode
                && !PlayerData.instance.scenesVisited.Contains(node.Scene)) return;

            // Don't add transitions blocked by infection
            if (outTransition.IsInfectedTransition()) return;

            if (IsVanillaOrCheckedTransition(outTransition) || outTransition.IsSpecialTransition())
            {
                queue.AddLast(node);

                // Allow other routes to go through a rejected route
                if (node.IsFollowingRejectedRoute(rejectedRoutes)) return;

                traversedTransitions.Add(outTransition);
            }
        }

        private static bool IsVanillaOrCheckedTransition(string transition)
        {
            return RM.RS.TrackerData.HasVisited(transition)
                || (transition.IsVanillaTransition() && transition.IsIn(RM.RS.TrackerData.pm));
        }
    }
}
