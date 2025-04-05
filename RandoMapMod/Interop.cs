namespace RandoMapMod;

internal static class Interop
{
    internal static bool HasBenchwarp { get; private set; } = false;

    internal static bool HasBenchRando { get; private set; } = false;

    // internal static bool HasDebugMod { get; private set; } = false;

    internal static void FindInteropMods()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            switch (assembly.GetName().Name)
            {
                case "Benchwarp":
                    HasBenchwarp = true;
                    continue;
                case "BenchRando":
                    HasBenchRando = true;
                    continue;
                // case "DebugMod":
                //     HasDebugMod = true;
                //     continue;
                default:
                    continue;
            }
        }
    }
}
