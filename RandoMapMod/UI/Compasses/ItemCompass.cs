using ConnectionMetadataInjector;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class ItemCompass : HookModule
    {
        private static GameObject goCompass;
        private static DirectionalCompass Compass => goCompass.GetComponent<DirectionalCompass>();

        private static Dictionary<string, List<PlacementCompassLocation>> compassLocations;

        public override void OnEnterGame()
        {
            Make();

            compassLocations = new();
            
            foreach (var kvp in ItemChanger.Internal.Ref.Settings.Placements)
            {
                if (SupplementalMetadata.Of(kvp.Value).Get(InteropProperties.CompassLocation) is not (string scene, float x, float y) location) continue;

                if (!compassLocations.TryGetValue(scene, out var list))
                {
                    list = new();
                    compassLocations.Add(scene, list);
                }

                list.Add(new((location.Item2, location.Item3), kvp.Value));
            }

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AfterSceneChange;
        }

        public override void OnQuitToMenu()
        {
            Destroy();

            compassLocations = null;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AfterSceneChange;
        }

        internal static void Update()
        {
            // RandoMapMod.Instance.LogDebug("Update item compass");

            if (goCompass == null || Compass == null)
            {
                return;
            }
            
            Compass.Locations.Clear();

            if (GameManager.instance.IsNonGameplayScene()
                || !compassLocations.TryGetValue(Utils.CurrentScene(), out var locations))
            {
                goCompass.SetActive(false);
                return;
            }

            foreach (var pcl in locations)
            {
                if (Compass.Locations.ContainsKey(pcl.Placement.Name)) continue;

                if (RandoMapMod.GS.ShowItemCompass is ItemCompassSetting.Reachable
                    && !RandomizerMod.RandomizerMod.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableLocations.Contains(pcl.Placement.Name)) continue;

                if (RandoMapMod.GS.ShowItemCompass is ItemCompassSetting.ReachableOutOfLogic
                    && !RandomizerMod.RandomizerMod.RS.TrackerData.uncheckedReachableLocations.Contains(pcl.Placement.Name)) continue;

                // RandoMapMod.Instance.LogDebug($"Add {pcl.Placement.Name}, {pcl.Position}");

                Compass.Locations.Add(pcl.Placement.Name, pcl);
            }

            goCompass.SetActive(Compass.Locations.Any());
        }

        internal static void UpdateCompassSprites()
        {
            if (Compass == null) return;

            Compass.UpdateSprite();
        }

        private static void Make()
        {
            goCompass = DirectionalCompass.Create
            (
                "Item Compass", // name
                () => { return HeroController.instance?.gameObject; }, // get parent entity
                1.5f, // radius
                1.5f, // scale
                IsCompassEnabled, // bool condition
                false, // do sprite rotation
                true, // lerp
                0.5f // lerp duration
            );
        }

        private static bool IsCompassEnabled()
        {
            return Conditions.RandoMapModEnabled() && RandoMapMod.GS.ShowItemCompass is not ItemCompassSetting.Off;
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
