using ItemChanger;
using MapChanger;
using UnityEngine;

namespace RandoMapMod.Pins;

/// <summary>
/// Has embedded scaling data.
/// </summary>
internal class ScaledPinSprite : ISprite
{
    private readonly ISprite _sprite;

    internal ScaledPinSprite(ISprite sprite, (int, int)? size)
    {
        if (sprite.Value is null)
        {
            _sprite = new EmbeddedSprite($"Pins.Unknown");
        }
        else
        {
            _sprite = sprite;
        }

        float scale;
        if (size is (int width, int height))
        {
            scale = SpriteManager.DEFAULT_PIN_SPRITE_SIZE / ((width + height) / 2f);
        }
        else
        {
            scale =
                SpriteManager.DEFAULT_PIN_SPRITE_SIZE
                / ((this._sprite.Value.rect.width + this._sprite.Value.rect.height) / 2f);
        }

        Scale = new Vector3(scale, scale, 1f);
    }

    internal ScaledPinSprite(ISprite sprite)
        : this(sprite, null) { }

    internal ScaledPinSprite(string key)
        : this(new EmbeddedSprite($"Pins.{key}"), null) { }

    public Sprite Value => _sprite.Value;

    internal Vector3 Scale { get; }

    public ISprite Clone()
    {
        return (ISprite)MemberwiseClone();
    }
}
