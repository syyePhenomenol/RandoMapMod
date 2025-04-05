using ItemChanger;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal class RandomizedPinDef(
    AbstractPlacement placement,
    ProgressionManager pm,
    ProgressionManager pmNoSequenceBreak
) : LogicICPinDef(placement, "Randomized", pm, pmNoSequenceBreak)
{
    internal override float GetZPriority()
    {
        return base.GetZPriority() + (10f * (int)Logic.State);
    }

    private protected override string GetPreviewText()
    {
        return base.GetPreviewText()
            ?.Replace("Pay ", "")
            ?.Replace("Once you own ", "")
            ?.Replace(", I'll gladly sell it to you.", "")
            ?.Replace("Requires ", "");
    }
}
