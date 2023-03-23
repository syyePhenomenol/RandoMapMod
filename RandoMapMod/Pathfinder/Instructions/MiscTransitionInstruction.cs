namespace RandoMapMod.Pathfinder.Instructions
{
    /// <summary>
    /// A best guess for miscellaneous cases where the final destination is a normal transition.
    /// </summary>
    internal class MiscTransitionInstruction : Instruction
    {
        internal MiscTransitionInstruction(string scene, string destination) : base($"{scene}>?>{destination}", destination)
        {

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

        /// <summary>
        /// More than one scene transition may be needed.
        /// </summary>
        internal override bool IsInProgress(ItemChanger.Transition lastTransition)
        {
            return !IsFinished(lastTransition);
        }
    }
}
