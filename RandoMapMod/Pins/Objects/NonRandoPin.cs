using RandoMapMod.Settings;

namespace RandoMapMod.Pins
{
    // Essentially a generic ItemChanger placement pin
    internal sealed class NonRandoPin : AbstractPlacementsPin
    {
        private protected override PoolsCollection PoolsCollection => PoolsCollection.Vanilla;
    }
}
