using ConnectionMetadataInjector;
using ItemChanger;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal static class InteropProperties
{
    // RandoMapMod currently has built-in metadata for the following connection mods:
    // - RandomizableLevers
    // - RandoPlus
    // - BenchRando

    // Other connection mods can inject their own pin position/behaviour by adding properties to an IInteropTag,
    // and adding the tag to either the placement/location or item.

    // ConnectionMetadataInjector defines the "PoolGroup" string property for placements/locations and items.

    // ** IMPORTANT PART, PLEASE READ CAREFULLY **
    // RandoMapMod will try to position the pin with the following priority:
    //      1) WorldMapLocation (or the first tuple of the deprecated WorldMapLocations), only if "scene" has a room
    //          sprite on the map.
    //      2) The first tuple of MapLocations where "scene" has a room sprite on the map.
    //      3) AbsMapLocation
    //      4) A default MapLocation defined by internal defs.
    //      5) A default WorldMapLocation if the placement has a CoordinateLocation.
    //      6) The miscellaneous pin grid, sorted by:
    //          - ModSource
    //          - LocationPoolGroup
    //          - PinGridIndex
    //          - AbstractPlacement name

    // For pins using "MapLocations" in particular, use the following style guidelines:
    // - Try to place pins at least 0.3 apart.
    // - Try to place pins in a logical manner with the rest of the existing pins.
    // - Try to place pins so the majority of it is over the corresponding room sprite.

    // If you do not provide a valid MapLocation-type property, and the placement doesn't have a valid scene name,
    // it will appear in the miscellaneous grid.
    // Sorting of these grid pins is done by the following values in decreasing order of priority:

    // HighlightScenes and PinGridIndex are only used if the pin is placed in the grid.

    /// <summary>
    /// The name of the connection mod that this placement/location belongs to. Used for sorting pins in a grid
    /// if applicable.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, string> ModSource =
        new("ModSource", (placement) => $"{char.MaxValue}");

    /// <summary>
    /// Permanently prevents the pin from being shown under any settings.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, bool> DoNotMakePin =
        new("DoNotMakePin", (placement) => false);

    /// <summary>
    /// If you want to make a pin for a non-randomizer placement. Has limited functionality.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, bool> MakeVanillaPin =
        new("MakeVanillaPin", (placement) => false);

    /// <summary>
    /// A key to access a built in sprite, for a placement's location.
    /// The key can be a pool group, an existing sprite key, or a path to the desired png with
    /// MapChanger.Resources.Sprites.Pins as the root directory.
    /// Takes precedence over LocationPinSprite.
    /// The built in sprites update their value properly depending on the PinStyle setting.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, string> LocationPinSpriteKey =
        new("PinSpriteKey", (placement) => null);

    /// <summary>
    /// The sprite shown when spoilers are off and the pin is not previewed.
    /// If not provided, defaults to one based on the PoolGroup if possible. Otherwise, a question mark.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, ISprite> LocationPinSprite =
        new("PinSprite", (placement) => null);

    /// <summary>
    /// The pixel dimensions (x, y) of the location pin sprite, not including transparent pixels that may
    /// be in the texture.
    /// If not provided, defaults to the size of the LocationPinSprite's texture.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, (int x, int y)?> LocationPinSpriteSize =
        new("PinSpriteSize", (placement) => null);

    /// <summary>
    /// A key to access a built in sprite, for a placement's specific item.
    /// The key can be a pool group, an existing sprite key, or a path to the desired png with
    /// MapChanger.Resources.Sprites.Pins as the root directory.
    /// Takes precedence over ItemPinSprite.
    /// </summary>
    internal static readonly MetadataProperty<AbstractItem, string> ItemPinSpriteKey =
        new("PinSpriteKey", (item) => null);

    /// <summary>
    /// The sprite shown when spoilers are on or the pin is previewed, and this item is currently displayed in the
    /// animated cycle.
    /// If not provided, defaults to one based on the PoolGroup if possible. Otherwise, a question mark.
    /// </summary>
    internal static readonly MetadataProperty<AbstractItem, ISprite> ItemPinSprite = new("PinSprite", (item) => null);

    /// <summary>
    /// The pixel dimensions (x, y) of the item pin sprite, not including transparent pixels that may be in the texture.
    /// If not provided, defaults to the size of the ItemPinSprite's texture.
    /// </summary>
    internal static readonly MetadataProperty<AbstractItem, (int x, int y)?> ItemPinSpriteSize =
        new("PinSpriteSize", (item) => null);

    /// <summary>
    /// Array of (scene, x-offset, y-offset) tuples to position the pin based on offset from the center of a room sprite.
    /// The offset is interpreted as the world coordinate in the actual room.
    /// The mod will attempt to place the pin at the first scene in the array that has a corresponding room sprite.
    /// </summary>
    [Obsolete("Please use WorldMapLocation instead.")]
    internal static readonly MetadataProperty<AbstractPlacement, (string scene, float x, float y)[]> WorldMapLocations =
        new("WorldMapLocations", (placement) => null);

    /// <summary>
    /// Array of (scene, x-offset, y-offset) tuples to position the pin based on offset from the center of a room sprite.
    /// The offset is interpreted as the world coordinate in the actual room.
    /// This property is only valid if the provided scene is mapped.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, (string scene, float x, float y)?> WorldMapLocation =
        new("WorldMapLocation", (placement) => null);

    /// <summary>
    /// Array of (scene, x-offset, y-offset) tuples to position the pin based on offset from the center of a
    /// room sprite.
    /// The offset is interpreted as unscaled (raw LocalPosition).
    /// You can use Ctrl-D with Debug level logging to get the offset values from the knight's world position.
    /// The mod will attempt to place the pin at the first scene in the array that has a corresponding room sprite.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, (string scene, float x, float y)[]> MapLocations =
        new("MapLocations", (placement) => null);

    /// <summary>
    /// (x-offset, y-offset) to position the pin based on absolute offset from the center of the map.
    /// The offset is interpreted as unscaled (raw LocalPosition).
    /// You can use Ctrl-D with Debug level logging to get the offset values from the knight's world position.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, (float x, float y)?> AbsMapLocation =
        new("AbsMapLocation", (placement) => null);

    /// <summary>
    /// The scenes for a multi-scene placement. The corresponding rooms are highlighted when the pin is selected.
    /// The pin will also appear in a grid on the quick map, if one of the HighlightScenes is part of the
    /// current map zone (and the placement is not cleared).
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, string[]> HighlightScenes =
        new("HighlightScenes", (placement) => null);

    /// <summary>
    /// Value to sort pins in the miscellaneous grid on the map.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, int> PinGridIndex =
        new("PinGridIndex", (placement) => int.MaxValue);

    /// <summary>
    /// Override logic infix fetched from the LogicManager.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, string> LogicInfix =
        new("LogicInfix", (placement) => null);

    /// <summary>
    /// Optional text that can be revealed to help players find the placement's location. name = hint,
    /// logic = requirements for the hint to be relevant.
    /// If you always want the hint to show, set logic to TRUE.
    /// </summary>
    internal static readonly MetadataProperty<AbstractPlacement, RawLogicDef[]> LocationHints =
        new("LocationHints", (placement) => RmmPinManager.Dpm.GetDefaultLocationHints(placement.Name));

    /// <summary>
    /// GameObjects (of given object path) by scene (dictionary key) to point the item compass to.
    /// Tracking can be done either for a static or moving GameObject.
    /// For static objects, setting updateEveryFrame to false is recommended to save computation.
    /// </summary>
    internal static readonly MetadataProperty<
        AbstractPlacement,
        Dictionary<string, (string objectName, bool updateEveryFrame)[]>
    > GameObjectCompassLocations = new("GameObjectCompassLocations", (placement) => null);

    /// <summary>
    /// Fixed coordinates by scene (dictionary key) to point the item compass to.
    /// </summary>
    internal static readonly MetadataProperty<
        AbstractPlacement,
        Dictionary<string, (float x, float y)[]>
    > FixedCompassLocations = new("FixedCompassLocations", (placement) => null);

    // If neither GameObjectCompassLocations or FixedCompassLocations are provided, the mod will try to
    // fetch in the following order:
    // - A static compass location based on internal coordinates lookup (for base/Lever/Bench rando only)
    // - A static compass location based on the AbstractLocation
    // - A static compass location based on WorldMapLocation
}
