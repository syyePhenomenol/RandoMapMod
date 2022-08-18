using ItemChanger;
using UnityEngine;

namespace RandoMapMod
{
    /// <summary>
    /// Uses MapChanger's SpriteManager to get a Sprite.
    /// </summary>
    public class EmbeddedSprite : ISprite
    {
        public string key;
        public Sprite Value => MapChanger.SpriteManager.Instance.GetSprite(key);

        public EmbeddedSprite(string key)
        {
            this.key = key;
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
