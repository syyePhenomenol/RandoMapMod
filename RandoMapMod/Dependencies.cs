namespace RandoMapMod;

internal static class Dependencies
{
    private static readonly string[] _dependencies = ["ConnectionMetadataInjector", "MapChanger", "RCPathfinder"];

    internal static bool HasAll()
    {
        var assemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name);

        foreach (var dependency in _dependencies)
        {
            if (assemblyNames.Contains(dependency))
            {
                continue;
            }

            RandoMapMod.Instance.LogWarn($"Missing dependency: {dependency}");
            return false;
        }

        return true;
    }
}
