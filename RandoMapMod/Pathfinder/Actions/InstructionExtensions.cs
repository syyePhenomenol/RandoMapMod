using MapChanger;
using Modding.Utils;
using RandoMapMod.Localization;
using RandoMapMod.UI;
using UnityEngine;

namespace RandoMapMod.Pathfinder.Actions
{
    internal static class InstructionExtensions
    {
        private const string TRANSITION_GATE_PREFIX = "_Transition Gates/";

        internal static string ToArrowedText(this IInstruction i)
        {
            return $" -> {i.SourceText.LT()}";
        }

        internal static TransitionCompassLocation GetCompassLocation(this IInstruction i)
        {
            if (i.CompassObjectPaths is null || !i.CompassObjectPaths.TryGetValue(Utils.CurrentScene(), out string objPath))
            {
                return null;
            }

            try
            {
                if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), objPath) is GameObject compassGO)
                {
                    return new(compassGO);
                }

                if (UnityExtensions.FindGameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), $"{TRANSITION_GATE_PREFIX}{objPath}") is GameObject compassGO2)
                {
                    return new(compassGO2);
                }
            }
            catch (Exception)
            {
                RandoMapMod.Instance.LogWarn($"Couldn't find object called {objPath} in {Utils.CurrentScene()}");
            }

            return null;
        }
    }
}