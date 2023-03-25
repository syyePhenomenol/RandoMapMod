using ConnectionMetadataInjector;
using ItemChanger;
using UnityEngine;

namespace RandoMapMod.Pins
{
    /// <summary>
    /// Fetches only the "normal" sprite. Appropriate for if spoilers are on or items are being previewed.
    /// </summary>
    internal class PinItemSprite : ISprite
    {
        public string Key { get; }

        public Sprite Value => value;
        private readonly Sprite value;

        /// <summary>
        /// Sets the sprite based on an item's PoolGroup.
        /// </summary>
        public PinItemSprite(AbstractItem item)
        {
            Key = SupplementalMetadata.Of(item).Get(InjectedProps.ItemPoolGroup);

            value = PinSpriteManager.GetSprite(Key, true);
        }

        /// <summary>
        /// Sets the sprite based on a connection-provided key.
        /// </summary>
        public PinItemSprite(string key)
        {
            Key = key;

            value = PinSpriteManager.GetSprite(Key, false);
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
