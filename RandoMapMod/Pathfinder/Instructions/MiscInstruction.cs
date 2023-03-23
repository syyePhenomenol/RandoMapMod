namespace RandoMapMod.Pathfinder.Instructions
{
    /// <summary>
    /// A best guess for miscellaneous cases where the final destination is not a normal transition.
    /// </summary>
    internal class MiscInstruction : Instruction
    {
        internal MiscInstruction(string scene, string newScene) : base($"{scene}>?>{newScene}", $"{newScene}[]")
        {

        }

        internal override bool IsFinished(ItemChanger.Transition lastTransition)
        {
            return TargetScene == lastTransition.SceneName;
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
