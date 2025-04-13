using ItemChanger.Extensions;
using MapChanger.Defs;
using UnityEngine;

namespace RandoMapMod.UI;

public class TransitionCompassTarget : CompassTarget
{
    private readonly Sprite _sprite = new EmbeddedSprite("GUI.Arrow").Value;
    private readonly Color _color;

    internal TransitionCompassTarget(string goPath, Color color)
    {
        _color = color;
        Position = new TransitionCompassPosition(goPath);
    }

    public override bool IsActive()
    {
        return true;
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public override Vector3 GetScale()
    {
        return Vector3.one;
    }

    public override Color GetColor()
    {
        return _color;
    }
}

public class TransitionCompassPosition(string goPath) : FixedCompassPosition(GetPosition(goPath))
{
    private const string TRANSITION_GATE_PREFIX = "_Transition Gates/";

    private static Vector2? GetPosition(string goPath)
    {
        if (
            UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), goPath)
            is GameObject compassGO
        )
        {
            return compassGO.transform.position;
        }

        if (
            UnityExtensions.FindGameObject(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                $"{TRANSITION_GATE_PREFIX}{goPath}"
            )
            is GameObject compassGO2
        )
        {
            return compassGO2.transform.position;
        }

        return null;
    }
}
