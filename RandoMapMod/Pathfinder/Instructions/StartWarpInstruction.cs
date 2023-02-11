using RandomizerCore.Logic;

namespace RandoMapMod.Pathfinder.Instructions
{
    internal class StartWarpInstruction : BenchwarpInstruction
    {
        internal StartWarpInstruction(Term startTerm) : base(BenchwarpInterop.BENCH_WARP_START, BenchwarpInterop.StartKey, startTerm.Name) { }
    }
}
