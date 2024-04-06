using System.Collections.ObjectModel;
using RandoMapMod.Transition;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC;
using RCPathfinder;
using RCPathfinder.Actions;
using JU = RandomizerCore.Json.JsonUtil;
using RM = RandomizerMod.RandomizerMod;
using SN = ItemChanger.SceneNames;

namespace RandoMapMod.Pathfinder
{
    internal class RmmSearchData : SearchData
    {
        internal StateUnion CurrentState { get; private set; }

        internal bool VanillaInfectedTransitions { get; }
        private static readonly string[] infectionTransitions =
        {
            "Crossroads_03[bot1]",
            "Crossroads_06[right1]",
            "Crossroads_10[left1]",
            "Crossroads_19[top1]"
        };

        private static Dictionary<string, string> conditionalTerms;

        internal ReadOnlyDictionary<string, ReadOnlyCollection<Term>> TransitionTermsByScene { get; private set; }

        internal AbstractAction DirthmouthStagTransition { get; private set; }
        internal Term StartTerm { get; private set; }
        internal ReadOnlyCollection<Term> BenchwarpTerms { get; private set; }

        internal static void LoadConditionalTerms()
        {
            conditionalTerms = JU.DeserializeFromEmbeddedResource<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.conditionalTerms.json");
        }

        public RmmSearchData(ProgressionManager reference) : base(reference)
        {
            if (Actions.Where(a => a.Name is "Room_Town_Stag_Station[left1]").FirstOrDefault() is AbstractAction action)
            {
                DirthmouthStagTransition = action;
            }

            if (LocalPM.ctx?.InitialProgression is ProgressionInitializer pi && pi.StartStateTerm is Term startTerm)
            {
                StartTerm = PositionLookup[startTerm.Name];
            }

            HashSet<Term> benchwarpTerms = new();

            foreach (KeyValuePair<string, RmmBenchKey> kvp in BenchwarpInterop.BenchKeys)
            {
                if (PositionLookup.TryGetValue(kvp.Key, out Term benchTerm))
                {
                    benchwarpTerms.Add(benchTerm);
                }
            }

            BenchwarpTerms = new(benchwarpTerms.ToArray());

            // To remove transitions that are blocked by infection from being included in the pathfinder
            VanillaInfectedTransitions = infectionTransitions.All(TransitionData.IsVanillaTransition);

            Dictionary<string, HashSet<Term>> transitionsByScene = new();

            foreach (Term position in Positions)
            {
                if (TransitionData.TryGetScene(position.Name, out string scene))
                {
                    if (transitionsByScene.TryGetValue(scene, out var transitions))
                    {
                        transitions.Add(position);
                        continue;
                    }

                    transitionsByScene[scene] = new() { position };
                }
            }

            transitionsByScene[SN.Room_Tram] = new() { PositionLookup["Lower_Tram"] };
            transitionsByScene[SN.Room_Tram_RG] = new() { PositionLookup["Upper_Tram"] };

            TransitionTermsByScene = new(transitionsByScene.ToDictionary(kvp => kvp.Key, kvp => new ReadOnlyCollection<Term>(kvp.Value.ToArray())));

            UpdateProgression();
        }

        protected override LogicManagerBuilder CreateLocalLM(LogicManagerBuilder lmb)
        {
            lmb = base.CreateLocalLM(lmb);

            if (ReferencePM.ctx?.InitialProgression is not ProgressionInitializer pi || pi.StartStateTerm is not Term startTerm)
            {
                throw new NullReferenceException();
            }

            // Inject new terms
            ILogicFormat fmt = new JsonLogicFormat();
            lmb.DeserializeFile(LogicFileType.Transitions, fmt, RandoMapMod.Assembly.GetManifestResourceStream("RandoMapMod.Resources.Pathfinder.Logic.transitions.json"));
            lmb.DeserializeFile(LogicFileType.Waypoints, fmt, RandoMapMod.Assembly.GetManifestResourceStream("RandoMapMod.Resources.Pathfinder.Logic.waypoints.json"));

            // Do rest of edits
            foreach (RawLogicDef rld in JU.DeserializeFromEmbeddedResource<RawLogicDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Logic.edits.json"))
            {
                if (lmb.IsTerm(rld.name)) lmb.DoLogicEdit(rld);
            }
            foreach (RawSubstDef rsd in JU.DeserializeFromEmbeddedResource<RawSubstDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Logic.substitutions.json"))
            {
                if (lmb.IsTerm(rsd.name)) lmb.DoSubst(rsd);
            }

            // Remove Start_State from existing logic
            foreach (Term term in lmb.Terms)
            {
                if (lmb.LogicLookup.ContainsKey(term.Name))
                {
                    lmb.DoSubst(new(term.Name, startTerm.Name, "FALSE"));
                }
            }

            // Link Start_State with start terms
            foreach (Term term in pi.StartStateLinkedTerms)
            {
                if (lmb.IsTerm(term.Name)) lmb.DoLogicEdit(new(term.Name, $"ORIG | {startTerm.Name}"));
            }

            return lmb;
        }

        protected override List<AbstractAction> CreateActions()
        {
            List<AbstractAction> actions = base.CreateActions();

            foreach (AbstractAction action in actions)
            {
                if (action is StateLogicAction)
                {
                    action.Cost = 0f;
                }
            }

            // Add extra transitions
            foreach (var (location, item) in TransitionData.ExtraVanillaTransitions)
            {
                actions.Add(new PlacementAction(PositionLookup[location], PositionLookup[item]));
            }

            return actions;
        }

        private static readonly HashSet<string> extraRooms = new()
        {
            SN.Room_Final_Boss_Atrium,
            SN.GG_Atrium,
            SN.GG_Workshop
        };

        /// <summary>
        /// Remove miscellaneous logical connections for now.
        /// </summary>
        protected override Dictionary<Term, List<AbstractAction>> CreateActionLookup()
        {
            var actionLookup = base.CreateActionLookup();

            Dictionary<Term, List<AbstractAction>> actionLookupCopy = actionLookup.ToDictionary(kvp => kvp.Key, kvp => new List<AbstractAction>(kvp.Value));

            foreach (var kvp in actionLookup)
            {
                if (!TransitionData.TryGetScene(kvp.Key.Name, out string scene)) continue;

                foreach (var action in kvp.Value)
                {
                    if (action is not StateLogicAction) continue;

                    string newScene;
                    if (extraRooms.Contains(action.Destination.Name) || RandomizerMod.RandomizerData.Data.IsRoom(action.Destination.Name))
                    {
                        newScene = action.Destination.Name;
                    }
                    else if (!TransitionData.TryGetScene(action.Destination.Name, out newScene))
                    {
                        continue;
                    }

                    if (scene != newScene) actionLookupCopy[kvp.Key].Remove(action);
                }
            }

            return actionLookupCopy;
        }

        public override List<AbstractAction> GetActions(Node node)
        {
            // Prune out of logic stuff
            var actions = base.GetActions(node).Where(a => ReferencePM.lm.GetTerm(a.Destination.Name) is null
                || (a is PlacementAction && TransitionData.IsVanillaOrCheckedTransition(a.Name) && !IsBlockedByInfection(a.Name))
                || (a is not PlacementAction && ReferencePM.Has(a.Destination))).ToList();

            if (node.CurrentPosition.Name is "Can_Stag" && !actions.Contains(DirthmouthStagTransition))
            {
                actions.Add(DirthmouthStagTransition);
            }

            return actions;
        }

        private static readonly (string term, string pdBool)[] pdBoolTerms =
        {
            ("Left_Elevator_Activated", nameof(PlayerData.cityLift1)),
            ("Town_Lift_Activated", nameof(PlayerData.mineLiftOpened)),
            ("Opened_Mawlek_Wall", nameof(PlayerData.crossroadsMawlekWall)),
            ("Opened_Dung_Defender_Wall", nameof(PlayerData.dungDefenderWallBroken)),
            ("Opened_Resting_Grounds_Floor", nameof(PlayerData.openedRestingGrounds02)),
            ("Opened_Gardens_Stag_Exit", nameof(PlayerData.openedGardensStagStation)),
            ("Opened_Glade_Door", nameof(PlayerData.gladeDoorOpened))
        };

        public override void UpdateProgression()
        {
            PlayerData pd = PlayerData.instance;

            base.UpdateProgression();

            // Emulate a transition being possibly available via having the required term
            foreach (KeyValuePair<string, string> kvp in conditionalTerms
                .Where(kvp => RM.RS.Context.LM.GetTerm(kvp.Key) is not null
                    && RM.RS.Context.LM.GetTerm(kvp.Value) is Term valueTerm
                    && valueTerm.Type is not TermType.State))
            {
                if (RM.RS.TrackerData.pm.Get(kvp.Key) > 0)
                {
                    LocalPM.Set(kvp.Value, 1);
                }
            }

            foreach ((string term, string pdBool) in pdBoolTerms)
            {
                if (LocalPM.lm.GetTerm(term) is null) continue;
                LocalPM.Set(term, pd.GetBool(pdBool) ? 1 : 0);
            }

            if (LocalPM.lm.GetTerm("Opened_Shaman_Pillar") is Term shamanPillar)
            {
                LocalPM.Set(shamanPillar, pd.GetBool(nameof(PlayerData.shamanPillar)) || pd.GetBool(nameof(PlayerData.crossroadsInfected)) ? 1 : 0);
            }

            foreach (PersistentBoolData pbd in SceneData.instance.persistentBoolItems)
            {
                switch (pbd.sceneName)
                {
                    case SN.Waterways_02:
                        if (pbd.id is "Quake Floor (1)")
                        {
                            LocalPM.Set("Broke_Waterways_Bench_Ceiling", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.Ruins1_31:
                        if (pbd.id is "Breakable Wall Ruin Lift")
                        {
                            LocalPM.Set("City_Toll_Wall_Broken", pbd.activated ? 1 : 0);
                        }
                        if (pbd.id is "Ruins Lever")
                        {
                            LocalPM.Set("Lever-Shade_Soul", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.RestingGrounds_10:
                        if (pbd.id is "Collapser Small (5)")
                        {
                            LocalPM.Set("Broke_Catacombs_Ceiling", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.Crossroads_10:
                        if (pbd.id is "Battle Scene")
                        {
                            LocalPM.Set("Defeated_False_Knight", pbd.activated ? 1 : 0);
                        }
                        break;
                }
            }

            foreach (PersistentIntData pid in SceneData.instance.persistentIntItems)
            {
                if (pid.sceneName is SN.Ruins1_31 && pid.id is "Ruins Lift")
                {
                    LocalPM.Set("City_Toll_Elevator_Up", pid.value % 2 is 1 ? 1 : 0);
                    LocalPM.Set("City_Toll_Elevator_Down", pid.value % 2 is 0 ? 1 : 0);
                }
            }

            UpdateCurrentState();
        }

        internal void UpdateCurrentState()
        {
            StateManager sm = LocalPM.lm.StateManager;
            StateBuilder sb = new(sm);
            PlayerData pd = PlayerData.instance;

            // USEDSHADE
            sb.SetBool(sm.GetBoolStrict("OVERCHARMED"), pd.GetBool(nameof(PlayerData.overcharmed)));
            sb.SetBool(sm.GetBoolStrict("SPENTALLSOUL"), pd.GetInt(nameof(PlayerData.MPCharge)) is 0 && pd.GetInt(nameof(PlayerData.MPReserve)) is 0);
            // CANNOTREGAINSOUL
            // CANNOTSHADESKIP
            // HASTAKENDAMAGE
            // HASTAKENDOUBLEDAMAGE
            // HASALMOSTDIED
            sb.SetBool(sm.GetBoolStrict("BROKEHEART"), pd.GetBool(nameof(PlayerData.brokenCharm_23)));
            sb.SetBool(sm.GetBoolStrict("BROKEGREED"), pd.GetBool(nameof(PlayerData.brokenCharm_24)));
            sb.SetBool(sm.GetBoolStrict("BROKESTRENGTH"), pd.GetBool(nameof(PlayerData.brokenCharm_25)));
            sb.SetBool(sm.GetBoolStrict("NOFLOWER"), !pd.GetBool(nameof(PlayerData.hasXunFlower)));
            // NOPASSEDCHARMEQUIP
            for (int i = 1; i <= 40; i++)
            {
                sb.SetBool(sm.GetBoolStrict($"CHARM{i}"), pd.GetBool($"equippedCharm_{i}"));
                sb.SetBool(sm.GetBoolStrict($"noCHARM{i}"), !pd.GetBool($"gotCharm_{i}"));
            }

            // SPENTSOUL
            // SPENTRESERVESOUL
            // SOULLIMITER
            // REQUIREDMAXSOUL
            // SPENTHP
            // SPENTBLUEHP
            sb.SetInt(sm.GetIntStrict("USEDNOTCHES"), pd.GetInt(nameof(PlayerData.charmSlotsFilled)));
            sb.SetInt(sm.GetIntStrict("MAXNOTCHCOST"), pd.GetInt(nameof(PlayerData.charmSlots)));

            CurrentState = new((State)new(sb));
        }

        private bool IsBlockedByInfection(string transition)
        {
            return infectionTransitions.Contains(transition)
                && VanillaInfectedTransitions
                && PlayerData.instance.GetBool(nameof(PlayerData.crossroadsInfected));
        }

        /// <summary>
        /// Groups transitions that are accesible from another transition in the same scene (both ways).
        /// </summary>
        internal StartPosition[] GetPrunedStartTerms(string scene)
        {
            if (scene is SN.Room_Tram) return new StartPosition[] { new("Lower_Tram", RmmPathfinder.SD.PositionLookup["Lower_Tram"], 0f) };
            if (scene is SN.Room_Tram_RG) return new StartPosition[] { new("Upper_Tram", RmmPathfinder.SD.PositionLookup["Upper_Tram"], 0f) };

            if (!TransitionTermsByScene.TryGetValue(scene, out var transitions)) return new StartPosition[] { };

            List<Term> inLogicTransitions = new(transitions.Where(t => (RM.RS.TrackerData.pm.lm.GetTerm(t.Name) is not null && RM.RS.TrackerData.pm.Get(t.Name) > 0)
                || TransitionTracker.InLogicExtraTransitions.Contains(t.Name)));

            SearchParams sp = new()
            {
                StartPositions = transitions.Select(t => new StartPosition(t.Name, t, 0f)).ToArray(),
                StartState = RmmPathfinder.SD.CurrentState,
                Destinations = transitions.ToArray(),
                MaxCost = 1f,
                MaxTime = 1000f,
                DisallowBacktracking = false
            };

            SearchState ss = new(sp);
            
            // RandoMapMod.Instance.LogDebug("Pruned start terms search");
            Algorithms.DijkstraSearch(RmmPathfinder.SD, sp, ss);

            List<Node> nodes = new(ss.ResultNodes.Where(n => n.Depth > 0 && n.StartPosition.Term != n.Actions.Last().Destination));

            List<StartPosition> prunedTransitions = new();

            foreach (Term transition in inLogicTransitions)
            {
                // RandoMapMod.Instance.LogDebug(transition.Name);

                if (prunedTransitions.Where(t => nodes.Any(n => n.StartPosition.Term == transition && n.Actions.Last().Destination == t.Term)
                    && nodes.Any(n => n.StartPosition.Term == t.Term && n.Actions.Last().Destination == transition))
                    .FirstOrDefault() is StartPosition accessibleTransition)
                {
                    // RandoMapMod.Instance.LogDebug($"Accessible from {accessibleTransition.Term}");

                    prunedTransitions.Add(new(accessibleTransition.Key, transition, 0f));
                    continue;
                }

                // RandoMapMod.Instance.LogDebug($"New transition {transition}");

                prunedTransitions.Add(new(transition.Name, transition, 0f));
            }

            if (scene is SN.Room_Town_Stag_Station
                && !inLogicTransitions.Any(t => t.Name is "Room_Town_Stag_Station[left1]")
                && ReferencePM.Get("Can_Stag") > 0)
            {
                prunedTransitions.Add(new("Can_Stag", PositionLookup["Can_Stag"], 0f));
            }

            return prunedTransitions.ToArray();
        }

        internal Term[] GetTransitionTerms(string scene)
        {
            if (!TransitionTermsByScene.TryGetValue(scene, out var transitions)) return new Term[] { };

            return transitions.ToArray();
        }
    }
}
