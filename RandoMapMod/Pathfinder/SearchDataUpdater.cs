using RandoMapMod.Transition;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using SN = ItemChanger.SceneNames;

namespace RandoMapMod.Pathfinder
{
    internal class SearchDataUpdater
    {
        private readonly RmmSearchData _sd;
        private readonly MainUpdater _mu;

        internal StateUnion CurrentState { get; private set; }

        internal ProgressionManager LocalPM => _sd.LocalPM;
        internal LogicManager LocalLM => _sd.LocalPM.lm;

        internal SearchDataUpdater(RmmSearchData sd)
        {
            _sd = sd;

            // Do automatic updating on new state-valued waypoints and transitions
            _mu = new(LocalLM, LocalPM);
            _mu.AddWaypoints(LocalPM.lm.Waypoints.Where(w => !_sd.ReferencePM.lm.Terms.Contains(w.term) && w.term.Type is TermType.State));

            foreach (LogicTransition lt in LocalPM.lm.TransitionLookup.Values.Where(t => !_sd.ReferencePM.lm.Terms.Contains(t.term)))
            {
                _mu.AddEntry(new StateUpdateEntry(lt.term, lt.logic));

                // Add update entry for the placement
                if (TransitionData.PlacementLookup.TryGetValue(lt.Name, out string target)
                    && LocalPM.lm.GetTerm(target) is Term targetTerm)
                {
                    _mu.LinkState(lt.term, targetTerm);
                }
            }
        }
        
        private static readonly (string term, string pdBool)[] pdBoolTerms =
        [
            ("RMM_Dung_Defender_Wall", nameof(PlayerData.dungDefenderWallBroken)),
            ("RMM_Mawlek_Wall", nameof(PlayerData.crossroadsMawlekWall)),
            ("RMM_Shaman_Pillar", nameof(PlayerData.shamanPillar)),
            ("RMM_Lower_Kingdom's_Edge_Wall", nameof(PlayerData.outskirtsWall)),
            ("RMM_Mantis_Big_Door", nameof(PlayerData.defeatedMantisLords)),
            ("RMM_Archives_Exit_Wall", nameof(PlayerData.oneWayArchive)),
            ("RMM_Gardens_Stag_Exit", nameof(PlayerData.openedGardensStagStation)),
            ("RMM_Left_Elevator", nameof(PlayerData.cityLift1)),
            ("RMM_Resting_Grounds_Floor", nameof(PlayerData.openedRestingGrounds02)),
            ("RMM_Glade_Door", nameof(PlayerData.gladeDoorOpened)),
            ("RMM_Sanctum_Glass_Floor", nameof(PlayerData.brokenMageWindow)),
            ("RMM_Bathhouse_Door", nameof(PlayerData.bathHouseOpened)),
            ("RMM_Elegant_Door", nameof(PlayerData.openedMageDoor_v2)),
            ("RMM_Emilitia_Door", nameof(PlayerData.city2_sewerDoor)),
            ("RMM_Catacombs_Wall", nameof(PlayerData.restingGroundsCryptWall)),
            ("RMM_Bathhouse_Wall", nameof(PlayerData.bathHouseWall)),
            ("RMM_Love_Door", nameof(PlayerData.openedLoveDoor)),
            ("RMM_Bretta_Door", nameof(PlayerData.brettaRescued)),
            ("RMM_Jiji_Door", nameof(PlayerData.jijiDoorUnlocked)),
            ("RMM_Sly_Door", nameof(PlayerData.slyRescued)),
            ("RMM_Dirtmouth_Station_Door", nameof(PlayerData.openedTownBuilding)),
            ("RMM_Dirtmouth_Lift", nameof(PlayerData.mineLiftOpened)),
            ("RMM_Divine_Door", nameof(PlayerData.divineInTown)),
            ("RMM_Grimm_Door", nameof(PlayerData.troupeInTown)),
            ("RMM_Waterways_Manhole", nameof(PlayerData.openedWaterwaysManhole)),
            ("RMM_Waterways_Acid", nameof(PlayerData.waterwaysAcidDrained)),
            ("RMM_Infected", nameof(PlayerData.crossroadsInfected))
        ];

        internal void Update()
        {
            // Update stateless waypoint terms
            PlayerData pd = PlayerData.instance;

            foreach ((string term, string pdBool) in pdBoolTerms)
            {
                LocalPM.Set(term, pd.GetBool(pdBool) ? 1 : 0);
            }

            if (LocalPM.lm.GetTerm("RMM_Not_Infected") is Term notInfected)
            {
                LocalPM.Set(notInfected, !pd.GetBool(nameof(PlayerData.crossroadsInfected)) ? 1 : 0);
            }

            if (LocalPM.lm.GetTerm("RMM_Blue_Room_Door") is Term blueRoomDoor)
            {
                LocalPM.Set(blueRoomDoor, pd.GetBool(nameof(PlayerData.blueRoomDoorUnlocked))
                    || (BossSequenceBindingsDisplay.CountCompletedBindings() >= 8) ? 1 : 0);
            }

            foreach (PersistentBoolData pbd in SceneData.instance.persistentBoolItems)
            {
                switch (pbd.sceneName)
                {
                    case SN.Crossroads_10:
                        if (pbd.id is "Battle Scene")
                        {
                            LocalPM.Set("RMM_False_Knight_Gate", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.Fungus2_14:
                        if (pbd.id is "Mantis Lever (1)")
                        {
                            LocalPM.Set("RMM_Mantis_Big_Floor", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.RestingGrounds_10:
                        if (pbd.id is "Collapser Small (5)")
                        {
                            LocalPM.Set("RMM_Catacombs_Ceiling", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.Ruins1_31:
                        if (pbd.id is "Breakable Wall Ruin Lift")
                        {
                            LocalPM.Set("RMM_City_Toll_Wall", pbd.activated ? 1 : 0);
                        }
                        if (pbd.id is "Ruins Lever")
                        {
                            LocalPM.Set("RMM_Shade_Soul_Exit", pbd.activated ? 1 : 0);
                        }
                        break;
                    case SN.Waterways_02:
                        if (pbd.id is "Quake Floor")
                        {
                            LocalPM.Set("RMM_Flukemarm_Floor", pbd.activated ? 1 : 0);
                        }
                        if (pbd.id is "Quake Floor (1)")
                        {
                            LocalPM.Set("RMM_Waterways_Bench_Floor", pbd.activated ? 1 : 0);
                        }
                        break;
                    // case SN.Waterways_04:
                    //     if (pbd.id is "Quake Floor")
                    //     {
                    //         LocalPM.Set("RMM_Waterways_Bench_Floor_1", pbd.activated ? 1 : 0);
                    //     }
                    //     if (pbd.id is "Quake Floor (1)")
                    //     {
                    //         LocalPM.Set("RMM_Waterways_Bench_Floor_2", pbd.activated ? 1 : 0);
                    //     }
                    //     break;
                    case SN.Waterways_05:
                        if (pbd.id is "Quake Floor")
                        {
                            LocalPM.Set("RMM_Dung_Defender_Floor", pbd.activated ? 1 : 0);
                        }
                        break;
                }
            }

            foreach (PersistentIntData pid in SceneData.instance.persistentIntItems)
            {
                if (pid.sceneName is SN.Ruins1_31 && pid.id is "Ruins Lift")
                {
                    LocalPM.Set("RMM_City_Toll_Lift_Up", pid.value % 2 is 1 ? 1 : 0);
                    LocalPM.Set("RMM_City_Toll_Lift_Down", pid.value % 2 is 0 ? 1 : 0);
                }
            }

            // Do updates on new state-valued terms
            _mu.StartUpdating();
            _mu.StopUpdating();

            // Update current state
            StateManager sm = LocalLM.StateManager;
            StateBuilder sb = new(sm);

            // USEDSHADE
            TrySetStateBool("OVERCHARMED", pd.GetBool(nameof(PlayerData.overcharmed)));
            // CANNOTOVERCHARM
            // CANNOTREGAINSOUL
            // TrySetStateBool("SPENTALLSOUL", pd.GetInt(nameof(PlayerData.MPCharge)) is 0 && pd.GetInt(nameof(PlayerData.MPReserve)) is 0);
            // CANNOTSHADESKIP
            TrySetStateBool("BROKEHEART", pd.GetBool(nameof(PlayerData.brokenCharm_23)));
            TrySetStateBool("BROKEGREED", pd.GetBool(nameof(PlayerData.brokenCharm_24)));
            TrySetStateBool("BROKESTRENGTH", pd.GetBool(nameof(PlayerData.brokenCharm_25)));
            TrySetStateBool("NOFLOWER", !pd.GetBool(nameof(PlayerData.hasXunFlower)));
            // NOPASSEDCHARMEQUIP
            for (int i = 1; i <= 40; i++)
            {
                TrySetStateBool($"CHARM{i}", pd.GetBool($"equippedCharm_{i}"));
                TrySetStateBool($"noCHARM{i}", !pd.GetBool($"gotCharm_{i}"));
            }

            // SPENTSOUL
            // SPENTRESERVESOUL
            // SOULLIMITER
            // REQUIREDMAXSOUL
            // SPENTHP
            // SPENTBLUEHP
            // LAZYSPENTHP
            TrySetStateInt("USEDNOTCHES", pd.GetInt(nameof(PlayerData.charmSlotsFilled)));
            TrySetStateInt("MAXNOTCHCOST", pd.GetInt(nameof(PlayerData.charmSlots)));

            CurrentState = new((State)new(sb));

            void TrySetStateBool(string name, bool value)
            {
                sb.TrySetStateBool(sm, name, value);
            }

            void TrySetStateInt(string name, int value)
            {
                sb.TrySetStateInt(sm, name, value);
            }
        }
    }
}