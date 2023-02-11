using BenchRando.IC;
using ItemChanger;
using static BenchRando.BRData;

namespace RandoMapMod
{
    internal class BenchRandoInterop
    {
        internal static Dictionary<RmmBenchKey, string> GetBenches()
        {
            BRLocalSettingsModule bsm = ItemChangerMod.Modules.Get<BRLocalSettingsModule>();
            return bsm.LS.Benches.ToDictionary(benchName => new RmmBenchKey(BenchLookup[benchName].SceneName, BenchLookup[benchName].GetRespawnMarkerName()), benchName => benchName);
        }

        internal static bool BenchRandoEnabled()
        {
            return ItemChangerMod.Modules.Get<BRLocalSettingsModule>() is BRLocalSettingsModule bsm
                && bsm.LS is not null && bsm.LS.Settings is not null
                && bsm.LS.Settings.IsEnabled();
        }

        internal static bool BenchesRandomized()
        {
            return ItemChangerMod.Modules.Get<BRLocalSettingsModule>() is BRLocalSettingsModule bsm
                && bsm.LS.Settings.RandomizedItems is not BenchRando.Rando.ItemRandoMode.None;
        }
    }
}
