using ItemChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal sealed class RandomizedBenchPinDef : RandomizedPinDef
{
    public RandomizedBenchPinDef(
        AbstractPlacement placement,
        ProgressionManager pm,
        ProgressionManager pmNoSequenceBreak
    )
        : base(placement, pm, pmNoSequenceBreak)
    {
        Bench = new(placement.Name);

        TextBuilders.Add(Bench.GetBenchwarpText);
    }

    internal BenchInfo Bench { get; }

    internal override void Update()
    {
        base.Update();
        Bench.Update();
    }

    internal override bool ActiveByCurrentMode()
    {
        if (Bench.IsActiveBench)
        {
            return true;
        }

        return base.ActiveByCurrentMode();
    }

    internal override bool ActiveBySettings()
    {
        if (Bench.IsActiveBench)
        {
            return true;
        }

        return base.ActiveBySettings();
    }

    internal override bool ActiveByProgress()
    {
        if (Bench.IsActiveBench)
        {
            return true;
        }

        return base.ActiveByProgress();
    }

    internal override IEnumerable<ScaledPinSprite> GetPinSprites()
    {
        if (Bench.IsActiveBench)
        {
            return [RmmPinManager.Psm.GetSprite("Benches")];
        }

        return base.GetPinSprites();
    }

    internal override PinShape GetMixedPinShape()
    {
        if (Bench.IsActiveBench)
        {
            return PinShape.Square;
        }

        return base.GetMixedPinShape();
    }

    internal override float GetZPriority()
    {
        if (Bench.IsActiveBench)
        {
            return -1f;
        }

        return base.GetZPriority();
    }

    private protected override string GetStatusText()
    {
        return $"{base.GetStatusText()}, {(Bench.IsVisitedBench ? "can warp" : "cannot warp").L()}";
    }
}
