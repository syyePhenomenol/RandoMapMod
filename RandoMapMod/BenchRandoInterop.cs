using BenchRando.IC;
using ItemChanger;
using System.Collections.Generic;
using System.Linq;
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
            BRLocalSettingsModule bsm = ItemChangerMod.Modules.Get<BRLocalSettingsModule>();
            return bsm != null && bsm.LS.Settings.IsEnabled();
        }

        internal static bool BenchesRandomized()
        {
            BRLocalSettingsModule bsm = ItemChangerMod.Modules.Get<BRLocalSettingsModule>();
            return bsm != null && bsm.LS.Settings.RandomizedItems is not BenchRando.Rando.ItemRandoMode.None;
        }
    }
}
