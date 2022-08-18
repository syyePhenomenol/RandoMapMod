using BenchRando.IC;
using Benchwarp;
using ItemChanger;
using System.Collections.Generic;
using System.Linq;
using static BenchRando.BRData;

namespace RandoMapMod
{
    internal class BenchRandoInterop
    {
        internal static Dictionary<RmmBenchKey, string> GetBenchTransitions()
        {
            return BenchLookup.ToDictionary(kvp => new RmmBenchKey(kvp.Value.SceneName, kvp.Value.GetRespawnMarkerName()), kvp => kvp.Key);
        }

        internal static bool IsBenchRandoEnabled()
        {
            BRLocalSettingsModule bsm = ItemChangerMod.Modules.Get<BRLocalSettingsModule>();
            return bsm != null && bsm.LS.Settings.IsEnabled();
        }
    }
}
