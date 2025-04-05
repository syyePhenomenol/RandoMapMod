using MapChanger;
using MapChanger.MonoBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI;

internal class RouteCompass : HookModule
{
    private static GameObject _goCompass;

    internal static RouteCompassInfo Info { get; private set; }

    public override void OnEnterGame()
    {
        Info = new();
        Make();
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AfterSceneChange;
    }

    public override void OnQuitToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AfterSceneChange;
        Destroy();
        Info = null;
    }

    internal static void Update()
    {
        Info.UpdateCompassTarget();
    }

    private static void Make()
    {
        _goCompass = DirectionalCompass.Make(Info);
    }

    private static void Destroy()
    {
        UnityEngine.Object.Destroy(_goCompass);
    }

    private static void AfterSceneChange(Scene from, Scene to)
    {
        Update();
    }
}
