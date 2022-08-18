using ItemChanger;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins
{
    /// <summary>
    /// Fetches only the "normal" sprite. Appropriate for if spoilers are on or items are being previewed.
    /// </summary>
    internal class PinItemSprite : ISprite
    {
        public string key;
        public PinItemSprite(string key)
        {
            this.key = key;
        }

        public Sprite Value => GetNormalSprite(key);

        internal static Sprite GetNormalSprite(string key)
        {
            string spriteName = key switch
            {
                "Dreamers" => "Dreamer",
                "Skills" => "Skill",
                "Charms" => "Charm",
                "Keys" => "Key",
                "Mask Shards" => "Mask",
                "Vessel Fragments" => "Vessel",
                "Charm Notches" => "Notch",
                "Pale Ore" => "Ore",
                "Geo Chests" => "Geo",
                "Rancid Eggs" => "Egg",
                "Relics" => "Relic",
                "Whispering Roots" => "Root",
                "Boss Essence" => "EssenceBoss",
                "Grubs" => "Grub",
                "Mimics" => "Grub",
                "Maps" => "Map",
                "Stags" => "Stag",
                "Lifeblood Cocoons" => "Cocoon",
                "Grimmkin Flames" => "Flame",
                "Journal Entries" => "Journal",
                "Geo Rocks" => "Rock",
                "Boss Geo" => "Geo",
                "Soul Totems" => "Totem",
                "Lore Tablets" => "Lore",
                "Shops" => "Shop",
                "Levers" => "Lever",
                "Mr Mushroom" => "Lore",
                "Benches" => "Bench",
                _ => "Unknown",
            };

            return MapChanger.SpriteManager.Instance.GetSprite($"Pins.{spriteName}");
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
