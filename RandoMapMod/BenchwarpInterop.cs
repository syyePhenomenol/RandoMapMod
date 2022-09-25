using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchRando;
using Benchwarp;
using InControl;
using MapChanger;
using Modding;
using UnityEngine;

namespace RandoMapMod
{
    internal record struct RmmBenchKey(string SceneName, string RespawnMarkerName);

    internal class BenchwarpInterop
    {
        internal const string BENCH_EXTRA_SUFFIX = "_Extra";
        internal const string BENCH_WARP_START = "Warp-Start";

        internal static Dictionary<RmmBenchKey, string> BenchNames { get; private set; } = new();

        internal static Dictionary<string, RmmBenchKey> BenchKeys { get; private set; } = new();

        internal static void Load()
        {
            BenchNames = new();
            BenchKeys = new();

            if (Interop.HasBenchRando() && BenchRandoInterop.BenchRandoEnabled())
            {
                BenchNames = BenchRandoInterop.GetBenches();
            }
            else
            {
                Dictionary<string, string> benchwarp = MapChanger.JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.benchwarp.json");

                foreach (KeyValuePair<string, string> kvp in benchwarp)
                {
                    Bench bench = Bench.Benches.FirstOrDefault(b => b.sceneName == kvp.Key);

                    if (bench is null) continue;

                    BenchNames.Add(new(bench.sceneName, bench.respawnMarker), kvp.Value);
                }
            }

            RmmBenchKey startKey = new(ItemChanger.Internal.Ref.Settings.Start.SceneName, "ITEMCHANGER_RESPAWN_MARKER");

            BenchNames.Add(startKey, BENCH_WARP_START);

            BenchKeys = BenchNames.ToDictionary(t => t.Value, t => t.Key);
        }

        internal static bool IsVisitedBench(string benchName)
        {
            return benchName is BENCH_WARP_START or $"{BENCH_WARP_START}{BENCH_EXTRA_SUFFIX}"
                || ((BenchKeys.TryGetValue(benchName, out RmmBenchKey key)
                    || (benchName.Length > BENCH_EXTRA_SUFFIX.Length && BenchKeys.TryGetValue(benchName.Substring(0, benchName.Length - BENCH_EXTRA_SUFFIX.Length), out key)))
                && GetVisitedBenchKeys().Contains(key));
        }

        internal static string GetHotkey(string benchName, string sceneName)
        {
            if (benchName is BenchwarpInterop.BENCH_WARP_START) return "SB";
            Bench benchObject;
            try
            {
                benchObject = Bench.Benches.Where(b => b.name == benchName.Replace("Bench-", "").ToCleanName()).Single();
            }
            catch (Exception)
            {
                benchObject = Bench.Benches.FirstOrDefault(b => b.sceneName == sceneName);
            }
            if (benchObject == null) return "";
            int benchIndex = Bench.Benches.IndexOf(benchObject);
            return (benchIndex / 10).ToString() + (benchIndex % 10).ToString();
        }

        internal static IEnumerable<string> GetVisitedBenchNames()
        {
            return GetVisitedBenchKeys()
                .Where(b => BenchNames.ContainsKey(b))
                .Select(b => BenchNames[b])
                .Concat(new List<string>() { BENCH_WARP_START });
        }

        internal static IEnumerator DoBenchwarp(string benchName)
        {
            if (BenchKeys.TryGetValue(benchName, out RmmBenchKey benchKey))
            {
                yield return DoBenchwarpInternal(benchKey);
            }
            else if (benchName.Length > BENCH_EXTRA_SUFFIX.Length && BenchKeys.TryGetValue(benchName.Substring(0, benchName.Length - BENCH_EXTRA_SUFFIX.Length), out benchKey))
            {
                yield return DoBenchwarpInternal(benchKey);
            }
        }

        private static IEnumerator DoBenchwarpInternal(RmmBenchKey benchKey)
        {
            InputHandler.Instance.inputActions.openInventory.CommitWithState(true, ReflectionHelper.GetField<OneAxisInputControl, ulong>(InputHandler.Instance.inputActions.openInventory, "pendingTick") + 1, 0);
            yield return new WaitWhile(() => GameManager.instance.inventoryFSM.ActiveStateName != "Closed");
            yield return new WaitForSeconds(0.15f);
            UIManager.instance.TogglePauseGame();
            yield return new WaitWhile(() => !GameManager.instance.IsGamePaused());
            yield return new WaitForSecondsRealtime(0.1f);

            if (GameManager.instance.IsGamePaused())
            {
                Bench bench = Bench.Benches.FirstOrDefault(b => b.sceneName == benchKey.SceneName && b.respawnMarker == benchKey.RespawnMarkerName);

                if (bench is not null)
                {
                    bench.SetBench();
                }
                else
                {
                    Benchwarp.Events.SetToStart();
                }

                ChangeScene.WarpToRespawn();
            }
        }

        /// <summary>
        /// Gets the BenchKeys from Benchwarp's visited benches and converts them to RmmBenchKeys. 
        /// </summary>
        private static HashSet<RmmBenchKey> GetVisitedBenchKeys()
        {
            return new(Benchwarp.Benchwarp.LS.visitedBenchScenes.Select(bwKey => new RmmBenchKey(bwKey.SceneName, bwKey.RespawnMarkerName)));
        }
    }
}
