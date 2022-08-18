using System.Linq;
using ConnectionMetadataInjector;
using ItemChanger;
using MapChanger.Defs;

namespace RandoMapMod.Pins
{
    internal static class InteropProperties
    {
        internal static readonly MetadataProperty<AbstractPlacement, string> ModSource = new("ModSource", (placement) => { return $"{char.MaxValue}"; });

        internal static readonly MetadataProperty<AbstractPlacement, bool> DoNotMakePin = new("DoNotMakePin", (placement) => { return false; });

        internal static readonly MetadataProperty<AbstractPlacement, ISprite> LocationPinSprite = new("PinSprite", GetDefaultLocationSprite);

        //TODO: move docstring into... documentation
        /// <summary>
        /// The effective length and height of the pin sprite, not including transparent pixels
        /// that may be in the texture.
        /// </summary>
        internal static readonly MetadataProperty<AbstractPlacement, (int, int)?> LocationPinSpriteSize = new("PinSpriteSize", (placement) => { return null; });

        private static ISprite GetDefaultLocationSprite(AbstractPlacement placement)
        {
            return new PinLocationSprite(SupplementalMetadata.OfPlacementAndLocations(placement).Get(InjectedProps.LocationPoolGroup));
        }

        internal static readonly MetadataProperty<AbstractItem, ISprite> ItemPinSprite = new("PinSprite", GetDefaultItemSprite);

        internal static readonly MetadataProperty<AbstractItem, (int, int)?> ItemPinSpriteSize = new("PinSpriteSize", (item) => { return null; });

        private static ISprite GetDefaultItemSprite(AbstractItem item)
        {
            return new PinItemSprite(SupplementalMetadata.Of(item).Get(InjectedProps.ItemPoolGroup));
        }

        internal static readonly MetadataProperty<AbstractPlacement, string[]> HighlightScenes = new("HighlightScenes", (placement) => { return null; });

        internal static readonly MetadataProperty<AbstractPlacement, (string, float, float)[]> MapLocations = new("MapLocations", (placement) => { return GetDefaultMapLocations(placement.Name); });

        internal static readonly MetadataProperty<AbstractPlacement, (string, float, float)[]> WorldMapLocations = new("WorldMapLocations", (placement) => { return null; });

        internal static readonly MetadataProperty<AbstractPlacement, (float, float)?> AbsMapLocation = new("AbsMapLocation", (placement) => { return null; });

        internal static readonly MetadataProperty<AbstractPlacement, int> PinGridIndex = new("PinGridIndex", (placement) => { return int.MaxValue; });

        internal static (string, float, float)[] GetDefaultMapLocations(string name)
        {
            if (MapChanger.Finder.TryGetLocation(name, out MapLocationDef mld))
            {
                return mld.MapLocations.Select(mapLocation => ((string, float, float))mapLocation).ToArray();
            }

            RandoMapMod.Instance.LogDebug($"No MapLocationDef found for placement {name}");

            if (ItemChanger.Finder.GetLocation(name) is AbstractLocation al && al.sceneName is not null)
            {
                return new (string, float, float)[] { new MapLocation() { MappedScene = MapChanger.Finder.GetMappedScene(al.sceneName) } };
            }

            RandoMapMod.Instance.LogDebug($"No MapLocation found for {name}.");

            return null;
        }
    }
}
