using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using MapChanger.Defs;
using UnityEngine;

namespace RandoMapMod.UI
{
    public class TransitionCompassTarget : CompassTarget
    {
        internal TransitionCompassTarget(string goPath)
        {
            Position = new TransitionCompassPosition(goPath);
        }

        public override bool IsActive() => true;

        private readonly Sprite sprite = new EmbeddedSprite("GUI.Arrow").Value;
        public override Sprite GetSprite() => sprite;

        private readonly Color color = RmmColors.GetColor(RmmColorSetting.UI_Compass);
        public override Color GetColor() => color;
    }

    public class TransitionCompassPosition(string goPath) : FixedCompassPosition(GetPosition(goPath))
    {
        private const string TRANSITION_GATE_PREFIX = "_Transition Gates/";

        private static Vector2? GetPosition(string goPath)
        {
            if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), goPath) is GameObject compassGO)
            {
                return compassGO.transform.position;
            }
            
            if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), $"{TRANSITION_GATE_PREFIX}{goPath}") is GameObject compassGO2)
            {
                return compassGO2.transform.position;
            }

            return null;
        }
    }
}