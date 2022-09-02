using ItemChanger;
using UnityEngine;

namespace RandoMapMod
{
    /// <summary>
    /// Uses MapChanger's SpriteManager to get a Sprite.
    /// </summary>
    internal class EmbeddedSprite : ISprite
    {
        internal string Key { get; init; }
        public Sprite Value => MapChanger.SpriteManager.Instance.GetSprite(Key);

        internal EmbeddedSprite(string key)
        {
            Key = key;
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
