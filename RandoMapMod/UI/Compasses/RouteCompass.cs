using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class RouteCompass : HookModule
    {
        internal static RouteManager RM => RmmPathfinder.RM;
        private static GameObject goCompass;
        private static DirectionalCompass Compass => goCompass.GetComponent<DirectionalCompass>();

        public override void OnEnterGame()
        {
            Make();

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AfterSceneChange;
        }

        public override void OnQuitToMenu()
        {
            Destroy();

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AfterSceneChange;
        }

        internal static void Update()
        {
            // RandoMapMod.Instance.LogDebug("Update route compass");

            if (goCompass == null || Compass == null)
            {
                return;
            }
            
            if (GameManager.instance.IsNonGameplayScene())
            {
                goCompass.SetActive(false);
                return;
            }

            if (RM.CurrentRoute is Route route && route.CurrentInstruction.GetCompassLocation() is TransitionCompassLocation tcl)
            {
                Compass.Locations = new() { { "arrow", tcl } };
            }
            else
            {
                Compass.Locations = [];
            }

            goCompass.SetActive(Compass.Locations.Any());
        }

        private static void Make()
        {
            goCompass = DirectionalCompass.Create
            (
                "Route Compass", // name
                () => { return HeroController.instance?.gameObject; }, // get parent entity
                1.5f, // radius
                2.0f, // scale
                IsCompassEnabled, // bool condition
                true, // do sprite rotation
                false, // lerp
                0.5f // lerp duration
            );
        }

        private static void AfterSceneChange(Scene from, Scene to)
        {
            Update();
        }

        private static void Destroy()
        {
            UnityEngine.Object.Destroy(goCompass);
        }

        private static bool IsCompassEnabled()
        {
            return Conditions.TransitionRandoModeEnabled()
                && RandoMapMod.GS.ShowRouteCompass;
        }
    }
}
