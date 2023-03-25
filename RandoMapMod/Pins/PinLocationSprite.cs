using ConnectionMetadataInjector;
using ItemChanger;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins
{
    /// <summary>
    /// Fetches the correct pin sprite based on the PinStyle setting.
    /// </summary>
    internal class PinLocationSprite : ISprite
    {
        public string Key { get; }

        public Sprite Value => values.TryGetValue(RandoMapMod.GS.PinStyle, out var value) ? value : values[PinStyle.Normal];
        private readonly Dictionary<PinStyle, Sprite> values = new();

        /// <summary>
        /// Sets the sprite based on a placement's PoolGroup.
        /// </summary>
        public PinLocationSprite(AbstractPlacement placement)
        {
            Key = SupplementalMetadata.Of(placement).Get(InjectedProps.LocationPoolGroup);

            foreach (PinStyle style in Enum.GetValues(typeof(PinStyle)))
            {
                values[style] = PinSpriteManager.GetStyleDependentSprite(Key, style, true);
            }
        }

        /// <summary>
        /// Sets the sprite based on a connection-provided key.
        /// </summary>
        public PinLocationSprite(string key)
        {
            Key = key;

            foreach (PinStyle style in Enum.GetValues(typeof(PinStyle)))
            {
                values[style] = PinSpriteManager.GetStyleDependentSprite(key, style, false);
            }
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
