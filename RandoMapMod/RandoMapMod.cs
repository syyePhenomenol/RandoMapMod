using System.Reflection;
using MapChanger;
using MapChanger.Defs;
using Modding;
using RandoMapCore;

namespace RandoMapMod;

public class RandoMapMod : Mod
{
    public RandoMapMod()
    {
        Instance = this;
    }

    internal static Assembly Assembly => Assembly.GetExecutingAssembly();
    internal static RandoMapMod Instance { get; private set; }

    public override string GetVersion()
    {
        return "3.6.0";
    }

    public override int LoadPriority()
    {
        return 10;
    }

    public override void Initialize()
    {
        LogDebug($"Initializing");

        Interop.FindInteropMods();

        Finder.InjectLocations(
            JsonUtil.DeserializeFromAssembly<Dictionary<string, MapLocationDef>>(
                Assembly,
                "RandoMapMod.Resources.locations.json"
            )
        );

        RandoMapCoreMod.AddDataModule(new RmmDataModule());

        LogDebug($"Initialization complete.");
    }
}
