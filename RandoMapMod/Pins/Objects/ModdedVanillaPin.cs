using ItemChanger;
using RandoMapMod.Settings;

namespace RandoMapMod.Pins
{
    // Essentially a generic ItemChanger placement pin
    internal sealed class ModdedVanillaPin : AbstractPlacementsPin
    {
        private protected override PoolsCollection PoolsCollection => PoolsCollection.Vanilla;

        private HintDef hintDef;
        internal override string HintText => hintDef.Text;

        internal override void Initialize(AbstractPlacement placement)
        {
            base.Initialize(placement);

            hintDef = new(InteropProperties.GetDefaultLocationHints(placement.Name));
        }
    }
}
