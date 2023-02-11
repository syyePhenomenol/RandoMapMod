using RandoMapMod.Transition;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Instructions
{
    internal class TransitionInstruction : Instruction
    {
        internal TransitionInstruction(PlacementAction action) : base(action.Name, action.Destination.Name)
        {
            if (CompassObjects is null && TransitionData.GetTransitionDef(action.Name) is RmmTransitionDef td)
            {
                CompassObjects = new() { { td.SceneName, td.DoorName } };
            }
        }

        internal override bool IsFinished(ItemChanger.Transition lastTransition)
        {
            // Fix for big mantis village transition
            string lastTransitionFixed = lastTransition.ToString() switch
            {
                "Fungus2_15[top2]" => "Fungus2_15[top3]",
                "Fungus2_14[bot1]" => "Fungus2_14[bot3]",
                _ => lastTransition.ToString()
            };

            return TargetTransition == lastTransitionFixed;
        }
    }
}
