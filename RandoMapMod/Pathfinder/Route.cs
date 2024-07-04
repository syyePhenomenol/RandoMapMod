using RandoMapMod.Localization;
using RandoMapMod.Pathfinder.Instructions;
using RandomizerCore.Logic;
using RCPathfinder;

namespace RandoMapMod.Pathfinder
{
    internal class Route
    {
        internal Node Node { get; }
        internal Term Start => Node.StartPosition.Term;
        internal Term Destination { get; }
        internal List<Instruction> RemainingInstructions { get; }
        internal Instruction FirstInstruction { get; }
        internal int TotalInstructionCount { get; }

        internal Route(Node node)
        {
            Node = node;

            Destination = node.Actions.LastOrDefault()?.Destination;

            RemainingInstructions = InstructionData.GetInstructions(node);

            FirstInstruction = RemainingInstructions.FirstOrDefault();

            TotalInstructionCount = RemainingInstructions.Count();
        }

        internal bool IsInProgress()
        {
            return TotalInstructionCount != RemainingInstructions.Count()
                && RemainingInstructions.Any();
        }

        internal string GetHint()
        {
            // Make hint based on waypoints that need to unlocked somewhere outside the room
            if (RemainingInstructions.Any(i => i.Text is "Town[door_sly]")
                && !PlayerData.instance.GetBool(nameof(PlayerData.slyRescued)))
            {
                return "You need to rescue Sly first.".L();
            }

            if (RemainingInstructions.Any(i => i.Text is "Town[door_bretta]")
                && !PlayerData.instance.GetBool(nameof(PlayerData.brettaRescued)))
            {
                return "You need to rescue Bretta first.".L();
            }

            if (RemainingInstructions.Any(i => i.Text is "Town[room_grimm]" or "Town[room_divine]")
                && !PlayerData.instance.GetBool(nameof(PlayerData.troupeInTown)))
            {
                return "You need to light the Nightmare Lantern first.".L();
            }

            if (RemainingInstructions.Any(i => i.Text is "RestingGrounds_05[right1]")
                && !PlayerData.instance.GetBool(nameof(PlayerData.gladeDoorOpened)))
            {
                return "You need to talk to Seer first.".L();
            }

            if (RemainingInstructions.Any(i => i.Text is "Waterways_07[right1]")
                && !PlayerData.instance.GetBool(nameof(PlayerData.waterwaysAcidDrained)))
            {
                return "You need to drain the Waterways acid first.".L();
            }

            return default;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Route route) return false;

            return RemainingInstructions.SequenceEqual(route.RemainingInstructions);
        }

        public override int GetHashCode()
        {
            int res = 0x2D2816FE;
            foreach (var i in RemainingInstructions)
            {
                res = res * 31 + (i == null ? 0 : i.GetHashCode());
            }
            return res;
        }
    }
}
