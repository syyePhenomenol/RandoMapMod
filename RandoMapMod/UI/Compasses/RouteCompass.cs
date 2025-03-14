using MapChanger;
using MapChanger.MonoBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class RouteCompass : HookModule
    {
        internal static RouteCompassInfo Info { get; private set; }
        private static GameObject goCompass;

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
            // RandoMapMod.Instance.LogDebug("Update route compass");
            Info.UpdateCompassTarget();
        }

        private static void Make()
        {
            goCompass = DirectionalCompass.Make(Info);
        }

        private static void Destroy()
        {
            UnityEngine.Object.Destroy(goCompass);
        }

        private static void AfterSceneChange(Scene from, Scene to)
        {
            Update();
        }
    }
}
