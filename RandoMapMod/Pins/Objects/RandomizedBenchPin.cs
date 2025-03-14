using ItemChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;

namespace RandoMapMod.Pins
{
    // Overrides normal RandomizedPin behaviour if the bench can be warped to.
    internal sealed class RandomizedBenchPin : RandomizedPin, IBenchPin
    {
        internal override void Initialize(AbstractPlacement placement)
        {
            base.Initialize(placement);

            textBuilders.Add(this.GetBenchwarpText);
        }

        private protected override bool ActiveByCurrentMode()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench()) return true;

            return base.ActiveByCurrentMode();
        }

        private protected override bool ActiveBySettings()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench())
            {
                activePlacements.Add(placements.First());
                return true;
            }

            return base.ActiveBySettings();
        }

        private protected override bool ActiveByProgress()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench())
            {
                // Update active items normally and discard result
                base.ActiveByProgress();
                return true;
            }

            return base.ActiveByProgress();
        }

        private protected override void UpdatePinSprites()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench())
            {
                CycleSprites = [Psm.GetSprite("Benches")];
                return;
            }
            
            base.UpdatePinSprites();
        }

        private protected override PinShape GetMixedPinShape()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench())
            {
                return PinShape.Square;
            }

            return base.GetMixedPinShape();
        }

        private protected override string GetStatusText()
        {
            return $"{base.GetStatusText()}, {(this.IsVisitedBench() ? "can warp" : "cannot warp").L()}";
        }
    }
}
