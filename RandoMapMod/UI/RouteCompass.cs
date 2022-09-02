using System.Collections.Generic;
using System.Linq;
using MapChanger;
using Modding.Utils;
using RandoMapMod.Modes;
using RandoMapMod.Transition;
using UnityEngine;
using SM = UnityEngine.SceneManagement.SceneManager;

namespace RandoMapMod.UI
{
    internal class RouteCompass : HookModule
    {
        private static GameObject goCompass;
        private static DirectionalCompass Compass => goCompass.GetComponent<DirectionalCompass>();
        private static GameObject Knight => HeroController.instance?.gameObject;
        internal static Dictionary<string, string> DoorObjectsByScene { get; private set; }
        internal static Dictionary<string, string> DoorObjectsByTransition { get; private set; }

        internal static void Load()
        {
            DoorObjectsByScene = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByScene.json");
            DoorObjectsByTransition = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByTransition.json");
        }

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
            Make();
            Update();
        }

        private static void Make()
        {
            if (goCompass is not null) Object.Destroy(goCompass);

            if (Knight is null || GameManager.instance.IsNonGameplayScene()) return;

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
            if (goCompass is null) return;

            if (Compass is not null && RouteTracker.SelectedRoute.Any())
            {
                string transition = RouteTracker.SelectedRoute.First();
                string scene = transition.GetScene();
                string gatePath = null;

                if (Utils.CurrentScene() == scene)
                {
                    if (DoorObjectsByTransition.ContainsKey(transition))
                    {
                        gatePath = DoorObjectsByTransition[transition];
                    }
                    else
                    {
                        gatePath = TransitionExtensions.GetDoor(transition);
                    }
                }
                else if ((transition.StartsWith("Stag-") || transition.StartsWith("Lower_Tram-") || transition.StartsWith("Upper_Tram-"))
                    && DoorObjectsByScene.ContainsKey(Utils.CurrentScene()))
                {
                    gatePath = DoorObjectsByScene[Utils.CurrentScene()];
                }

                if (gatePath is null)
                {
                    goCompass.SetActive(false);
                    return;
                }

                GameObject gateObject = UnityExtensions.FindGameObject(SM.GetActiveScene(), gatePath);

                if (gateObject is not null)
                {
                    Compass.TrackedObjects = new() { gateObject };
                    goCompass.SetActive(true);
                    return;
                }

                GameObject gateObject2 = UnityExtensions.FindGameObject(SM.GetActiveScene(), "_Transition Gates/" + gatePath);

                if (gateObject2 is not null)
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
