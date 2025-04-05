using ItemChanger;
using UnityEngine;

namespace RandoMapMod;

/// <summary>
/// Uses MapChanger's SpriteManager to get a Sprite.
/// </summary>
internal class EmbeddedSprite : ISprite
{
    public string Key { get; }
    public Sprite Value { get; }

    internal EmbeddedSprite(string key)
    {
        Key = key;
        Value = MapChanger.SpriteManager.Instance.GetSprite(key);
    }

    public ISprite Clone()
    {
        return (ISprite)MemberwiseClone();
    }
}
