using MapChanger;
using MapChanger.MonoBehaviours;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class ItemCompass : HookModule
    {
        internal static ItemCompassInfo Info { get; private set; }
        private static GameObject goCompass;

        public override void OnEnterGame()
        {
            Info = new();
            Make();
            On.PlayMakerFSM.Start += LateUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EarlyUpdate;
        }

        public override void OnQuitToMenu()
        {
            On.PlayMakerFSM.Start -= LateUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= EarlyUpdate;
            Destroy();
            Info = null;
        }

        private static void EarlyUpdate(Scene from, Scene to)
        {
            Info.UpdateCompassTargets();

            finishedLateUpdate = false;
        }

        private static bool finishedLateUpdate;

        private static void LateUpdate(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);

            if (finishedLateUpdate)
            {
                return;
            }

            RandoMapMod.Instance.LogFine("Update item compass");
            Info.UpdateCurrentCompassTargets();
            finishedLateUpdate = true;
        }

        private static void Make()
        {
            goCompass = DirectionalCompass.Make(Info);
        }

        private static void Destroy()
        {
            UnityEngine.Object.Destroy(goCompass);
        }
    }
}
