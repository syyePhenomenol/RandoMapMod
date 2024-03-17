using ItemChanger.Extensions;
using MapChanger;
using RandoMapMod.Localization;
using UnityEngine;

namespace RandoMapMod.Pathfinder.Instructions
{
    internal class Instruction
    {
        private static Dictionary<string, Dictionary<string, string>> routeCompassOverrides;

        internal static void LoadRouteCompassOverrides()
        {
            routeCompassOverrides = JsonUtil.DeserializeFromAssembly<Dictionary<string, Dictionary<string, string>>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Compass.routeCompassOverrides.json");
        }

        private static readonly string transitionGatePrefix = "_Transition Gates/";

        internal string TargetScene => TargetTransition.Split('[')[0];

        /// <summary>
        /// The transition that indicates the instruction has been performed.
        /// </summary>
        internal string TargetTransition { get; }

        internal string ArrowedText => $" -> {Text.LT()}";

        internal string Text { get; }

        /// <summary>
        /// The path to objects per scene that the compass should point to.
        /// </summary>
        internal Dictionary<string, string> CompassObjects { get; protected private init; }

        internal Instruction(string text, string targetTransition)
        {
            Text = text;

            TargetTransition = targetTransition;

            if (routeCompassOverrides.TryGetValue(text, out var objs))
            {
                CompassObjects = objs;
            }
        }

        internal virtual bool IsInProgress(ItemChanger.Transition lastTransition)
        {
            return false;
        }

        internal virtual bool IsFinished(ItemChanger.Transition lastTransition)
        {
            return TargetTransition == lastTransition.ToString();
        }

        internal bool TryGetCompassGO(out GameObject go)
        {
            if (CompassObjects is not null && CompassObjects.TryGetValue(Utils.CurrentScene(), out string objPath))
            {
                if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), objPath) is GameObject compassGO)
                {
                    go = compassGO;
                    return true;
                }

                if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), $"{transitionGatePrefix}{objPath}") is GameObject compassGO2)
                {
                    go = compassGO2;
                    return true;
                }

                RandoMapMod.Instance.LogWarn($"Couldn't find object called {objPath} in {Utils.CurrentScene()}");
            }

            go = null;
            return false;
        }
    }
}
