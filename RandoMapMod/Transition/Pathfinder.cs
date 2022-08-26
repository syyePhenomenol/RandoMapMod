using RandoMapMod.Modes;
using RandomizerCore.Logic;
using System.Collections.Generic;
using System.Linq;
using PD = RandoMapMod.Transition.PathfinderData;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    public static class Pathfinder
    {
        private static RandomizerMod.Settings.TrackerData Td => RM.RS.TrackerData;

        internal static ProgressionManager localPm;

        internal static void Initialize()
        {
            localPm = new(PD.Lm, RM.RS.Context);

            // Remove start terms
            foreach (string term in PD.GetStartTerms())
            {
                localPm.Set(term, 0);
            }

            UpdateProgression();
        }

        public static void UpdateProgression()
        {
            foreach (Term term in Td.pm.lm.Terms)
            {
                if (!RandomizerMod.RandomizerData.Data.IsTransition(term.Name)
                    && !RandomizerMod.RandomizerData.Data.IsRoom(term.Name))
                {
                    localPm.Set(term.Id, Td.pm.Get(term.Id));
                }
            }

            // Emulate a transition being possibly available via having the required term
            foreach (KeyValuePair<string, string> pair in PD.ConditionalTerms)
            {
                if (Td.pm.Get(pair.Key) > 0)
                {
                    localPm.Set(pair.Value, 1);
                }
            }

            localPm.Set("Town_Lift_Activated", PlayerData.instance.GetBool("mineLiftOpened") ? 1 : 0);
            localPm.Set("Opened_Shaman_Pillar", PlayerData.instance.GetBool("shamanPillar") ? 1 : 0);
            localPm.Set("Opened_Mawlek_Wall", PlayerData.instance.GetBool("crossroadsMawlekWall") ? 1 : 0);
            localPm.Set("Opened_Dung_Defender_Wall", PlayerData.instance.GetBool("dungDefenderWallBroken") ? 1 : 0);
            localPm.Set("ELEGANT", PlayerData.instance.GetBool("hasWhiteKey") ? 1 : 0);

            foreach (PersistentBoolData pbd in SceneData.instance.persistentBoolItems)
            {
                if (pbd.sceneName == "Waterways_02" && pbd.id == "Quake Floor (1)")
                {
                    localPm.Set("Broke_Waterways_Bench_Ceiling", pbd.activated ? 1 : 0);
                }
                else if (pbd.sceneName == "Ruins1_31" && pbd.id == "Breakable Wall Ruin Lift")
                {
                    localPm.Set("City_Toll_Wall_Broken", pbd.activated ? 1 : 0);
                }
                else if (pbd.sceneName == "Ruins1_31" && pbd.id == "Ruins Lever")
                {
                    localPm.Set("Lever-Shade_Soul", pbd.activated ? 1 : 0);
                }
            }

            foreach (PersistentIntData pid in SceneData.instance.persistentIntItems)
            {
                if (pid.sceneName == "Ruins1_31" && pid.id == "Ruins Lift")
                {
                    localPm.Set("City_Toll_Elevator_Up", pid.value % 2 == 1 ? 1 : 0);
                    localPm.Set("City_Toll_Elevator_Down", pid.value % 2 == 0 ? 1 : 0);
                }
            }
        }

        // Calculates the shortest route (by number of transitions) from start to final.
        // The search space will be purely limited to rooms that have been visited + unreached reachable locations
        // A ProgressionManager is used to track logic while traversing through the search space
        // If reevaluating, start and final are transitions instead of scenes
        public static List<string> ShortestRoute(string start, string final, List<List<string>> rejectedRoutes, bool reevaluate)
        {
            RandoMapMod.Instance.LogDebug($"Start: {start}, Final: {final}, Reevaluate: {reevaluate}");

            if (start == null || final == null) return new();

            if (reevaluate && start == final) return new();

            HashSet<string> candidateReachableTransitions = new();
            HashSet<string> normalTransitionSpace = GetNormalTransitionSpace();

            // Algorithm (BFS)
            HashSet<string> visitedTransitions = new();
            LinkedList<SearchNode> queue = new();
            string searchScene;

            InitializeNodeQueue();

            while (queue.Any())
            {
                SearchNode node = queue.First();
                queue.RemoveFirst();

                if (reevaluate)
                {
                    // If reevaluating, we just check if the final transition is correct
                    if (final != "" && node.lastAdjacentTerm == final)
                    {
                        node.PrintRoute();
                        return node.route;
                    }
                }
                else
                {
                    // Avoid terminating on duplicate/redudant new paths
                    if (node.scene == final && !rejectedRoutes.Any(r => r.First() == node.route.First() && r.Last().GetAdjacentTerm() == node.lastAdjacentTerm))
                    {
                        // No other paths to same final transition with a different starting benchwarp
                        if (node.route.First().IsBenchwarpTransition()
                            && rejectedRoutes.Any(r => r.Last().GetAdjacentTerm() == node.lastAdjacentTerm && r.First().IsBenchwarpTransition())) continue;

                        node.PrintRoute();
                        return node.route;
                    }
                }

                searchScene = node.scene;

                localPm.StartTemp();

                localPm.Set(node.lastAdjacentTerm, 1);

                // It is important we use all the reachable transitions in the room for correct logic, even if they are unchecked
                candidateReachableTransitions = new(searchScene.GetTransitionsInScene());

                while (UpdateReachableTransitions(searchScene, candidateReachableTransitions)) { }

                foreach (string transition in candidateReachableTransitions
                    .Where(t => !visitedTransitions.Contains(t))
                    .Where(t => localPm.Get(t) > 0))
                {
                    TryAddNode(node, transition);
                }

                localPm.RemoveTempItems();
                candidateReachableTransitions.Clear();
            }

            // No route found, or the parameters are invalid
            return new();

            void InitializeNodeQueue()
            {
                // Add initial bench warp transitions if setting is enabled
                if (RandoMapMod.GS.PathfinderBenchwarp && Interop.HasBenchwarp())
                {
                    foreach (string transition in BenchwarpInterop.GetVisitedBenchNames())
                    {
                        TryAddNode(null, transition);
                    }
                }

                localPm.StartTemp();

                // If reevaluating, start is a term instead of a scene
                if (reevaluate)
                {
                    if (localPm.lm.TermLookup.ContainsKey(start))
                    {
                        localPm.Set(start, 1);
                    }

                    searchScene = start.GetScene() ?? start.GetAdjacentScene();
                    candidateReachableTransitions = searchScene.GetTransitionsInScene();

                    while (UpdateReachableTransitions(searchScene, candidateReachableTransitions)) { }

                    // Remove certain top transitions that can't be returned to 
                    if (localPm.lm.TransitionLookup.TryGetValue(start, out LogicTransition transition) && !transition.CanGet(localPm))
                    {
                        localPm.Set(start, 0);
                    }
                }
                else
                {
                    IEnumerable<string> seedTransitions = TransitionData.GetTransitionsByScene(start)
                        .Where(t => normalTransitionSpace.Contains(t) || localPm.lm.TransitionLookup[t].CanGet(localPm));

                    foreach (string transition in seedTransitions)
                    {
                        localPm.Set(transition, 1);
                    }
                    searchScene = start;
                    candidateReachableTransitions = start.GetTransitionsInScene();

                    while (UpdateReachableTransitions(searchScene, candidateReachableTransitions)) { }
                }

                foreach (string transition in candidateReachableTransitions.Where(t => localPm.lm.TransitionLookup[t].CanGet(localPm)))
                {
                    TryAddNode(null, transition);
                }

                localPm.RemoveTempItems();

                if (reevaluate)
                {
                    // Prefer doubling back if possible
                    List<SearchNode> startNodes; 

                    if (start == "Room_Tram_RG[door1]")
                    {
                        startNodes = queue.Where(n => n.route.First().StartsWith("Upper_Tram")).ToList();
                    }
                    else if (start == "Room_Tram[door1]")
                    {
                        startNodes = queue.Where(n => n.route.First().StartsWith("Lower_Tram")).ToList();
                    }
                    else
                    {
                        startNodes = queue.Where(n => n.route.First() == start).ToList();
                    }

                    foreach(SearchNode startNode in startNodes)
                    {
                        queue.Remove(startNodes.First());
                        queue.AddFirst(startNodes.First());
                    }
                }
            }

            void TryAddNode(SearchNode node, string transition)
            {
                // Check if a normal transition has actually been visited so far (as opposed to unchecked)
                if (transition.IsSpecialTransition() || normalTransitionSpace.Contains(transition))
                {
                    SearchNode newNode;

                    string adjacent = transition.GetAdjacentTerm();

                    if (adjacent == null || !localPm.lm.TermLookup.ContainsKey(adjacent)) return;

                    if (node != null)
                    {
                        // No repeated transitions
                        if (node.route.Any(t => t == transition) || node.route.Any(t => t == adjacent)) return;

                        newNode = new(transition.GetAdjacentScene(), node.route, adjacent);
                        newNode.route.Add(transition);

                        // Keep index of rejectedRoutes with same current transition
                        newNode.repeatedRoutes = node.repeatedRoutes.Where(i => rejectedRoutes[i].Count >= newNode.route.Count && rejectedRoutes[i][newNode.route.Count - 1] == transition);
                    }
                    else
                    {
                        newNode = new(transition.GetAdjacentScene(), new() { transition }, adjacent);

                        // Get index of rejectedRoutes with same starting transition
                        newNode.repeatedRoutes = rejectedRoutes.Select((r, i) => new { r, i }).Where(x => x.r.First() == transition).Select(x => x.i);
                    }

                    //newNode.PrintRoute();

                    queue.AddLast(newNode);

                    // If the route matches any rejected route so far, allow for other routes to visit the same transition
                    if (!newNode.repeatedRoutes.Any())
                    {
                        visitedTransitions.Add(transition);
                    }
                }
            }
        }

        class SearchNode
        {
            public SearchNode(string scene, List<string> route, string lat)
            {
                this.scene = scene;
                this.route = new(route);
                lastAdjacentTerm = lat;
            }

            public void PrintRoute()
            {
                string text = "Current route:";

                foreach (string transition in route)
                {
                    text += " -> " + transition;
                }

                RandoMapMod.Instance.LogDebug(text);
            }

            public string scene;
            public List<string> route = new();
            public string lastAdjacentTerm;
            // The indexes of the routes in rejectedRoutes this node is repeating
            public IEnumerable<int> repeatedRoutes;
        }

        // Use the pathfinder to figure out all reachable scenes including through special transitions (but not benchwarps)
        public static HashSet<string> GetAdjacentReachableScenes(string scene)
        {
            HashSet<string> adjacentReachableScenes = new();

            HashSet<string> normalTransitionSpace = GetNormalTransitionSpace();

            IEnumerable<string> seedTransitions = TransitionData.GetTransitionsByScene(scene)
                        .Where(t => normalTransitionSpace.Contains(t) || localPm.lm.TransitionLookup[t].CanGet(localPm));

            HashSet<string> candidateReachableTransitions = scene.GetTransitionsInScene();

            localPm.StartTemp();

            foreach (string transition in seedTransitions)
            {
                localPm.Set(transition, 1);
            }

            while (UpdateReachableTransitions(scene, candidateReachableTransitions)) { }

            foreach (string transition in candidateReachableTransitions.Where(t => localPm.Get(t) > 0))
            {
                adjacentReachableScenes.Add(transition.GetAdjacentScene());
            }

            localPm.RemoveTempItems();

            return adjacentReachableScenes;
        }

        private static readonly HashSet<string> infectionBlockedTransitions = new()
        {
            "Crossroads_03[bot1]",
            "Crossroads_06[right1]",
            "Crossroads_10[left1",
            "Crossroads_19[top1]"
        };

        public static HashSet<string> GetNormalTransitionSpace()
        {
            HashSet<string> transitions = new();

            // Add normal transitions
            foreach (string transition in Td.lm.TransitionLookup.Keys)
            {
                if (Td.uncheckedReachableTransitions.Contains(transition)
                    || transition.GetAdjacentTerm() == null) continue;

                string scene = transition.GetScene();

                if (MapChanger.Settings.CurrentMode() is TransitionVisitedOnlyMode
                    && !PlayerData.instance.scenesVisited.Contains(scene)) continue;

                if (Td.pm.Get(transition) > 0
                    // Prevents adding certain randomized transitions that haven't been visited yet in uncoupled rando
                    && !(TransitionData.IsRandomizedTransition(transition)
                        && !Td.visitedTransitions.ContainsKey(transition)))
                {
                    transitions.Add(transition);
                }
            }

            if (PlayerData.instance.GetBool("crossroadsInfected") && infectionBlockedTransitions.All(t => !TransitionData.IsRandomizedTransition(t)))
            {
                transitions.RemoveWhere(t => infectionBlockedTransitions.Contains(t));
            }

            return transitions;
        }

        // Add other in-logic transitions in the current room
        public static bool UpdateReachableTransitions(string searchScene, HashSet<string> candidateReachableTransitions)
        {
            bool continueUpdating = false;

            foreach (string transition in candidateReachableTransitions)
            {
                if (localPm.lm.TransitionLookup[transition].CanGet(localPm)
                    && localPm.Get(transition) < 1)
                {
                    localPm.Set(transition, 1);
                    continueUpdating = true;
                }
            }

            if (searchScene.TryGetSceneWaypoint(out LogicWaypoint waypoint)
                && !localPm.Has(waypoint.term) && waypoint.CanGet(localPm))
            {
                localPm.Add(waypoint);
                continueUpdating = true;
            }

            return continueUpdating;
        }

        /// <summary>
        /// Logs which transitions are logically reachable from each starting transition (within the room).
        /// </summary>
        public static void GetFullNetwork()
        {
            RandoMapMod.Instance.LogDebug("Full pathfinder network:");

            foreach (Term term in localPm.lm.Terms)
            {
                RandoMapMod.Instance.LogDebug($"{term.Name}");
                string scene = term.Name.GetScene();

                if (scene is null) continue;

                localPm.StartTemp();

                localPm.Set(term, 1);

                HashSet<string> candidateReachableTransitions = new(scene.GetTransitionsInScene());

                while (UpdateReachableTransitions(scene, candidateReachableTransitions)) { }

                foreach (string transition in candidateReachableTransitions)
                {
                    if (localPm.Get(transition) > 0)
                    {
                        RandoMapMod.Instance.LogDebug($"---> {transition}");
                    }
                    else
                    {
                        RandoMapMod.Instance.LogDebug($"-x-> {transition}");
                    }
                }

                localPm.RemoveTempItems();
            }
        }
    }
}
