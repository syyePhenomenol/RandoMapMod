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
        public string key;
        public PinLocationSprite(string key)
        {
            this.key = key;
        }

        public Sprite Value
        {
            get
            {
                string spriteName = "Unknown";

                if (RandoMapMod.GS.PinStyle == PinStyle.Normal)
                {
                    return PinItemSprite.GetNormalSprite(key);
                }
                else if (RandoMapMod.GS.PinStyle == PinStyle.Q_Marks_1)
                {
                    spriteName = key switch
                    {
                        "Shops" => "Shop",
                        _ => "Unknown",
                    };
                }
                else if (RandoMapMod.GS.PinStyle == PinStyle.Q_Marks_2)
                {
                    spriteName = key switch
                    {
                        "Grubs" => "UnknownGrubInv",
                        "Mimics" => "UnknownGrubInv",
                        "Lifeblood Cocoons" => "UnknownLifebloodInv",
                        "Geo Rocks" => "UnknownGeoRockInv",
                        "Soul Totems" => "UnknownTotemInv",
                        "Shops" => "Shop",
                        _ => "Unknown",
                    };
                }
                else if (RandoMapMod.GS.PinStyle == PinStyle.Q_Marks_3)
                {
                    spriteName = key switch
                    {
                        "Grubs" => "UnknownGrub",
                        "Mimics" => "UnknownGrub",
                        "Lifeblood Cocoons" => "UnknownLifeblood",
                        "Geo Rocks" => "UnknownGeoRock",
                        "Soul Totems" => "UnknownTotem",
                        "Shops" => "Shop",
                        _ => "Unknown",
                    };
                }

                return MapChanger.SpriteManager.Instance.GetSprite($"Pins.{spriteName}");
            }
        }

        public ISprite Clone()
        {
            return (ISprite)MemberwiseClone();
        }
    }
}
