using MapChanger.Defs;
using UnityEngine;

namespace RandoMapMod.UI
{
    public class TransitionCompassLocation : GameObjectCompassLocation
    {
        private readonly Sprite sprite;
        public override Sprite Sprite => sprite;

        private readonly Color color;
        public override Color Color => color;

        public TransitionCompassLocation(GameObject go) : base(go)
        {
            sprite = new EmbeddedSprite("GUI.Arrow").Value;
            color = RmmColors.GetColor(RmmColorSetting.UI_Compass);
        }
    }
}