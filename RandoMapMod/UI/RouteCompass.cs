using System.Linq;
using MapChanger;
using Modding.Utils;
using RandoMapMod.Modes;
using RandoMapMod.Transition;
using UnityEngine;
using PD = RandoMapMod.Transition.PathfinderData;
using SM = UnityEngine.SceneManagement.SceneManager;

namespace RandoMapMod.UI
{
    internal class RouteCompass : HookModule
    {
        private static GameObject goCompass;
        private static DirectionalCompass Compass => goCompass.GetComponent<DirectionalCompass>();
        private static GameObject Knight => HeroController.instance?.gameObject;

        public override void OnEnterGame()
        {
            SM.activeSceneChanged += ActiveSceneChanged;
        }

        public override void OnQuitToMenu()
        {
            SM.activeSceneChanged -= ActiveSceneChanged;
        }

        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            CreateRouteCompass();
            Update();
        }

        private static void CreateRouteCompass()
        {
            if (goCompass != null && Compass != null) Compass.Destroy();

            if (Knight == null || GameManager.instance.IsNonGameplayScene()) return;

            Sprite arrow = new EmbeddedSprite("GUI.Arrow").Value;

            goCompass = DirectionalCompass.Create
            (
                "Route Compass", // name
                Knight, // parent entity
                arrow, // sprite
                RmmColors.GetColor(RmmColorSetting.UI_Compass), // color
                1.5f, // radius
                2.0f, // scale
                IsCompassEnabled, // bool condition
                false, // lerp
                0.5f // lerp duration
            );

            goCompass.SetActive(false);
        }

        public static void Update()
        {
            if (goCompass == null) return;

            if (Compass != null && RouteTracker.SelectedRoute.Any())
            {
                string transition = RouteTracker.SelectedRoute.First();
                string scene = transition.GetScene();
                string gate = "";

                if (Utils.CurrentScene() == scene)
                {
                    if (PD.DoorObjectsByTransition.ContainsKey(transition))
                    {
                        gate = PD.DoorObjectsByTransition[transition];
                    }
                    else if (TransitionData.IsInTransitionLookup(transition))
                    {
                        gate = TransitionData.GetTransitionDoor(transition);
                    }
                    else if (transition.Contains("[") && transition.Contains("]"))
                    {
                        gate = transition.Split(']')[0].Split('[')[1];
                    }
                }
                else if ((transition.IsStagTransition() || transition.IsTramTransition())
                    && PD.DoorObjectsByScene.ContainsKey(Utils.CurrentScene()))
                {
                    gate = PD.DoorObjectsByScene[Utils.CurrentScene()];
                }

                if (gate == "")
                {
                    goCompass.SetActive(false);
                    return;
                }

                GameObject gateObject = UnityExtensions.FindGameObject(SM.GetActiveScene(), gate);

                if (gateObject != null)
                {
                    Compass.TrackedObjects = new() { gateObject };
                    goCompass.SetActive(true);
                    return;
                }

                GameObject gateObject2 = UnityExtensions.FindGameObject(SM.GetActiveScene(), "_Transition Gates/" + gate);

                if (gateObject2 != null)
                {
                    Compass.TrackedObjects = new() { gateObject2 };
                    goCompass.SetActive(true);
                }
            }
            else
            {
                goCompass.SetActive(false);
            }
        }

        private static bool IsCompassEnabled()
        {
            return MapChanger.Settings.MapModEnabled()
                && MapChanger.Settings.CurrentMode().GetType().IsSubclassOf(typeof(TransitionRandoMode))
                && RandoMapMod.GS.ShowRouteCompass;
        }
    }
}
