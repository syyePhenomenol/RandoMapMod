using MapChanger.Defs;
using UnityEngine;

namespace RandoMapMod.UI
{
    public class TransitionCompassLocation(GameObject go) : GameObjectCompassLocation(go)
    {
        private readonly Sprite sprite = new EmbeddedSprite("GUI.Arrow").Value;
        public override Sprite Sprite => sprite;

        private readonly Color color = RmmColors.GetColor(RmmColorSetting.UI_Compass);
        public override Color Color => color;
    }
}