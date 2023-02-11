namespace RandoMapMod.Pathfinder.Instructions
{
    internal class DreamgateInstruction : Instruction
    {
        internal DreamgateInstruction(string targetTransition) : base("Dreamgate", targetTransition) { }

        internal override bool IsFinished(ItemChanger.Transition lastTransition)
        {
            return DreamgateTracker.DreamgateTiedTransition == TargetTransition && lastTransition.GateName is "dreamGate";
        }
    }
}
