using ItemChanger;
using MapChanger.Defs;
using RandoMapMod.Pins;
using UnityEngine;

namespace RandoMapMod.UI
{
    internal class PlacementCompassLocation : CompassLocation
    {
        private bool isActive;
        public override bool IsActive => isActive;

        public AbstractPlacement Placement { get; init; }
        private readonly Vector2 position;
        public override Vector2 Position => position;

        public override Sprite Sprite => PinSpriteManager.GetLocationSprite(Placement).Value;

        public override Color Color => RmmColors.GetColor(RmmColorSetting.UI_Compass);

        internal PlacementCompassLocation(WorldCoordinates location, AbstractPlacement placement)
        {
            Placement = placement;
            position = new Vector2(location.X, location.Y);

            OnGive(null);

            foreach (var item in placement.Items)
            {
                item.OnGive += OnGive;
            }
        }

        private void OnGive(ReadOnlyGiveEventArgs args)
        {
            isActive = !Placement.AllEverObtained();
        }
    }
}