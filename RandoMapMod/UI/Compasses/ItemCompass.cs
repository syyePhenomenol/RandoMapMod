using MapChanger;
using MapChanger.MonoBehaviours;
using RandomizerMod.IC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI;

internal class ItemCompass : HookModule
{
    private static GameObject _goCompass;

    private bool _finishedLateUpdate;

    internal static ItemCompassInfo Info { get; private set; }

    public override void OnEnterGame()
    {
        Info = new();
        Make();

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EarlyUpdate;
        On.PlayMakerFSM.Start += LateUpdate;
        TrackerUpdate.OnFinishedUpdate += Info.UpdateCurrentCompassTargets;
    }

    public override void OnQuitToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= EarlyUpdate;
        On.PlayMakerFSM.Start -= LateUpdate;
        TrackerUpdate.OnFinishedUpdate -= Info.UpdateCurrentCompassTargets;

        Destroy();
        Info = null;
    }

    private void EarlyUpdate(Scene from, Scene to)
    {
        Info.UpdateCompassTargets();

        _finishedLateUpdate = false;
    }

    private void LateUpdate(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
    {
        orig(self);

        if (_finishedLateUpdate)
        {
            return;
        }

        RandoMapMod.Instance.LogFine("Update item compass");
        Info.UpdateCurrentCompassTargets();
        _finishedLateUpdate = true;
    }

    private void Make()
    {
        _goCompass = DirectionalCompass.Make(Info);
    }

    private void Destroy()
    {
        UnityEngine.Object.Destroy(_goCompass);
    }
}
