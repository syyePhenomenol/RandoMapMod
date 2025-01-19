using System.Reflection;

namespace RandoMapMod
{
    internal static class Interop
    {
        internal static bool HasBenchwarp { get; private set; } = false;

        internal static bool HasBenchRando { get; private set; } = false;

        internal static void FindInteropMods()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                switch (assembly.GetName().Name)
                {
                    case "Benchwarp":
                        HasBenchwarp = true;
                        continue;
                    case "BenchRando":
                        HasBenchRando = true;
                        continue;
                    default:
                        continue;
                }
            }
        }
    }
}
