using ItemChanger;
using UnityEngine;

namespace RandoMapMod
{
    /// <summary>
    /// Uses MapChanger's SpriteManager to get a Sprite.
    /// </summary>
    internal class EmbeddedSprite : ISprite
    {
        public string Key { get; init; }
        
        private readonly Sprite value;
        public Sprite Value => value;

        internal EmbeddedSprite(string key)
        {
            Key = key;
            value = MapChanger.SpriteManager.Instance.GetSprite(key);
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
