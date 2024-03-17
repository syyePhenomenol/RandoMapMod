using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class RouteCompass : HookModule
    {
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

            if (RouteManager.CurrentRoute is not null && RouteManager.CurrentRoute.RemainingInstructions.First().TryGetCompassGO(out GameObject go))
            {
                Compass.Locations = new() { { "arrow", new TransitionCompassLocation(go) } };
            }
            else
            {
                Compass.Locations = new();
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
