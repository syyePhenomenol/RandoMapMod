using ItemChanger;
using MapChanger;
using UnityEngine;

namespace RandoMapMod.Pins
{
    /// <summary>
    /// Has embedded scaling data.
    /// </summary>
    internal class ScaledPinSprite : ISprite
    {
        private readonly ISprite sprite;
        public Sprite Value => sprite.Value;

        internal Vector3 Scale { get; init; }

        internal ScaledPinSprite(ISprite sprite, (int, int)? size)
        {
            if (sprite.Value is null)
            {
                this.sprite = new EmbeddedSprite($"Pins.Unknown");
            }
            else
            {
                this.sprite = sprite;
            }

            float scale;
            if (size is (int width, int height))
            {
                scale = SpriteManager.DEFAULT_PIN_SPRITE_SIZE / ((width + height) / 2f);
            }
            else
            {
                scale = SpriteManager.DEFAULT_PIN_SPRITE_SIZE / ((this.sprite.Value.rect.width + this.sprite.Value.rect.height) / 2f);
            }
            
            Scale = new Vector3(scale, scale, 1f);
        }

        internal ScaledPinSprite(ISprite sprite) : this(sprite, null) { }

        internal ScaledPinSprite(string key) : this(new EmbeddedSprite($"Pins.{key}"), null) { }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
