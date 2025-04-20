namespace RandoMapMod;

internal static class Interop
{
    internal static bool HasBenchRando { get; private set; } = false;

    internal static void FindInteropMods()
    {
        HasBenchRando = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name is "BenchRando");
    }
}
