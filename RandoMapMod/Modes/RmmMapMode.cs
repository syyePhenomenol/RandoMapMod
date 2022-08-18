using MapChanger;

namespace RandoMapMod.Modes
{
    internal class RmmMapMode : MapMode
    {
        public override float Priority => 0f;
        public override bool ForceHasMap => true;
        public override bool ForceHasQuill => true;
        public override OverrideType VanillaPins => OverrideType.ForceOff;
        public override OverrideType MapMarkers => OverrideType.ForceOff;
        public override bool ImmediateMapUpdate => true;
        public override bool FullMap => true;
    }
}
