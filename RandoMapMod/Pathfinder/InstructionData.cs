using System.Collections.ObjectModel;
using MapChanger;
using RandoMapMod.Pathfinder.Instructions;
using RandoMapMod.Transition;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder
{
    internal class InstructionData
    {
        internal static InstructionData Instance;
        internal static ReadOnlyDictionary<(string waypoint, string targetScene), WaypointInstruction> WaypointInstructions { get; private set; }

        internal ReadOnlyCollection<Instruction> Instructions { get; }
        internal ReadOnlyDictionary<string, TransitionInstruction> TransitionInstructions { get; }
        internal StartWarpInstruction StartWarpInstruction { get; }
        internal ReadOnlyDictionary<string, BenchwarpInstruction> BenchwarpInstructions { get; }
        internal DreamgateInstruction DreamgateInstruction { get; private set; } = null;

        //private readonly EmptyInstruction empty = new();

        //private readonly Dictionary<(string, string), MiscTransitionInstruction> miscTransitionInstructions = new();
        //private readonly Dictionary<(string, string), MiscInstruction> miscInstructions = new();

        internal static void LoadWaypointInstructions()
        {
            WaypointInstruction[] waypointInstructions = JsonUtil.DeserializeFromAssembly<WaypointInstruction[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.waypointInstructions.json");

            Dictionary<(string, string), WaypointInstruction> wiLookup = new();

            foreach (WaypointInstruction wi in waypointInstructions)
            {
                wiLookup[(wi.Waypoint, wi.TargetScene)] = wi;
            }

            WaypointInstructions = new(wiLookup);
        }

        internal InstructionData(RmmSearchData sd)
        {
            Instance = this;

            Dictionary<string, TransitionInstruction> transitionInstructions = new();

            foreach (AbstractAction action in sd.Actions)
            {
                if (action.IsOrIsSubclassInstanceOf<PlacementAction>())
                {
                    transitionInstructions[action.Name] = new(action.Name, action.Destination.Name);
                }
            }

            Instructions = new(transitionInstructions.Values.ToArray());
            TransitionInstructions = new(transitionInstructions);

            if (Interop.HasBenchwarp())
            {
                StartWarpInstruction = new(sd.StartTerm);
                BenchwarpInstructions = new(BenchwarpInterop.BenchKeys.ToDictionary(b => b.Key, b => new BenchwarpInstruction(b.Key, b.Value, b.Key)));
            }
        }

        internal static List<Instruction> GetInstructions(Node node)
        {
            List<Instruction> instructions = new();

            string position = node.StartPosition.Name;
            TransitionData.TryGetScene(position, out string scene);

            if (Interop.HasBenchwarp())
            {
                if (position == Instance.StartWarpInstruction.Waypoint)
                {
                    instructions.Add(Instance.StartWarpInstruction);
                    scene = Instance.StartWarpInstruction.TargetScene;
                }

                if (Instance.BenchwarpInstructions.TryGetValue(position, out var benchWarp))
                {
                    instructions.Add(benchWarp);
                    scene = benchWarp.TargetScene;
                }
            }

            if (node.Key is "dreamGate")
            {
                instructions.Add(Instance.DreamgateInstruction);
            }

            foreach (AbstractAction action in node.Actions)
            {
                if (TryGetInstruction(position, scene, action, out Instruction instruction))
                {
                    instructions.Add(instruction);
                    scene = instruction.TargetScene;

                    //if (instruction == Instance.empty) return new();
                }

                position = action.Destination.Name;
            }

            return instructions;
        }

        private static readonly HashSet<string> extraRooms = new()
        {
            "Room_Final_Boss_Atrium",
            "GG_Atrium",
            "GG_Workshop"
        };

        private static bool TryGetInstruction(string position, string scene, AbstractAction action, out Instruction instruction)
        {
            if (Instance is null) throw new NullReferenceException(nameof(Instance));

            if (action.IsOrIsSubclassInstanceOf<PlacementAction>())
            {
                instruction = Instance.TransitionInstructions[action.Name];
                return true;
            }

            // We need to get the waypoint instruction that matches the current scene, current position and target scene.
            // The scene gets propagated from the previous term if current position doesn't have a scene.
            // This ensures that the scene/newScene comparison is correct.
            if (action.IsOrIsSubclassInstanceOf<StateLogicAction>())
            {
                string newScene;
                if (extraRooms.Contains(action.Destination.Name) || RandomizerMod.RandomizerData.Data.IsRoom(action.Destination.Name))
                {
                    newScene = action.Destination.Name;
                }
                else if (!TransitionData.TryGetScene(action.Destination.Name, out newScene))
                {
                    instruction = default;
                    return false;
                }

                if (scene != newScene)
                {
                    if (WaypointInstructions.TryGetValue((position, newScene), out WaypointInstruction wi))
                    {
                        instruction = wi;
                        return true;
                    }
                    //// Fallback handling for when the position is a transition and its target is in the newScene - assume going through the transition
                    //else if (TransitionData.Placements.TryGetValue(action.Name, out string target)
                    //    && TransitionData.GetTransitionDef(target) is RmmTransitionDef targetTd
                    //    && targetTd.SceneName == newScene)
                    //{
                    //    RandoMapMod.Instance?.LogDebug($"New best guess TransitionInstruction: {action.Name}-?>{target}-?>{action.Destination.Name}");
                    //    instruction = Instance.TransitionInstructions[action.Name];
                    //}
                    //// The destination is a transition
                    //else if (TransitionData.GetTransitionDef(action.Destination.Name) is not null)
                    //{
                    //    RandoMapMod.Instance?.LogDebug($"New MiscTranstisionInstruction: {scene}-?>{action.Destination.Name}");
                    //    instruction = GetOrAddMiscTransitionInstruction(scene, action.Destination.Name);
                    //}
                    //else
                    //{
                    //    RandoMapMod.Instance?.LogDebug($"New MiscInstruction: {scene}-?>{newScene}");
                    //    instruction = GetOrAddMiscInstruction(scene, newScene);
                    //}
                    //else
                    //{
                    //    //RandoMapMod.Instance?.LogDebug($"Empty instruction for action {action.Name}, {position}, {scene}, {newScene}, {action.Destination.Name}");
                    //    instruction = Instance.empty;
                    //}

                    //return true;
                }

                //RandoMapMod.Instance?.LogDebug($"No instruction for action {action.Name}, {position}, {scene}, {newScene}, {action.Destination.Name}");
            }

            instruction = default;
            return false;
        }

        internal static void UpdateDreamgateInstruction(string dreamgateTiedTransition)
        {
            Instance.DreamgateInstruction = new(dreamgateTiedTransition);
        }

        //private static MiscTransitionInstruction GetOrAddMiscTransitionInstruction(string scene, string destination)
        //{
        //    if (Instance.miscTransitionInstructions.TryGetValue((scene, destination), out var instruction))
        //    {
        //        return instruction;
        //    }

        //    return Instance.miscTransitionInstructions[(scene, destination)] = new MiscTransitionInstruction(scene, destination);
        //}

        //private static MiscInstruction GetOrAddMiscInstruction(string scene, string newScene)
        //{
        //    if (Instance.miscInstructions.TryGetValue((scene, newScene), out var instruction))
        //    {
        //        return instruction;
        //    }

        //    return Instance.miscInstructions[(scene, newScene)] = new MiscInstruction(scene, newScene);
        //}
    }
}