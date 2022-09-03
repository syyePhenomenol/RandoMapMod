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
    internal static class RouteCompass
    {
        private static GameObject goCompass;
        private static DirectionalCompass Compass => goCompass?.GetComponent<DirectionalCompass>();
        private static GameObject Knight => HeroController.instance?.gameObject;
        internal static Dictionary<string, string> DoorObjectsByScene { get; private set; }
        internal static Dictionary<string, string> DoorObjectsByTransition { get; private set; }

        internal static void Load()
        {
            DoorObjectsByScene = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByScene.json");
            DoorObjectsByTransition = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByTransition.json");
        }

        internal static void Update()
        {
            RandoMapMod.Instance.LogDebug("Update compass");

            Destroy();

            if (Knight == null || GameManager.instance.IsNonGameplayScene()) return;

            Make();

            if (RouteTracker.SelectedRoute.Any())
            {
                string transition = RouteTracker.SelectedRoute.First();
                string gatePath = null;

                // The compass might update in a scene not well defined such as Cinematic_Stag_Travel. Everything does need to be nested like this
                if (transition.StartsWith("Stag-") || transition.StartsWith("Lower_Tram-") || transition.StartsWith("Upper_Tram-"))
                {
                    if (DoorObjectsByScene.ContainsKey(Utils.CurrentScene()))
                    {
                        gatePath = DoorObjectsByScene[Utils.CurrentScene()];
                    }
                }
                else
                {
                    if (Utils.CurrentScene() == transition.GetScene())
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
                }

                if (gatePath is null)
                {
                    goCompass.SetActive(false);
                    return;
                }

                GameObject gateObject = UnityExtensions.FindGameObject(SM.GetActiveScene(), gatePath);

                if (gateObject != null)
                {
                    Compass.TrackedObjects = new() { gateObject };
                    goCompass.SetActive(true);
                    return;
                }

                GameObject gateObject2 = UnityExtensions.FindGameObject(SM.GetActiveScene(), "_Transition Gates/" + gatePath);

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

        private static void Make()
        {
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
        }

        private static void Destroy()
        {
            Object.Destroy(goCompass);
        }

        private static bool IsCompassEnabled()
        {
            return MapChanger.Settings.MapModEnabled()
                && MapChanger.Settings.CurrentMode().GetType().IsSubclassOf(typeof(TransitionRandoMode))
                && RandoMapMod.GS.ShowRouteCompass;
        }
    }
}
