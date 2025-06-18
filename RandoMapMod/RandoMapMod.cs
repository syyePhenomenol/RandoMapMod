using System.Reflection;
using Modding;
using RandoMapCore;
using RandomizerMod.Logging;
using RandomizerMod.RC;

namespace RandoMapMod;

public class RandoMapMod : Mod, IGlobalSettings<RmmSettings>, ILocalSettings<RmmSettings>
{
    public RandoMapMod()
    {
        Instance = this;
    }

    internal static Assembly Assembly => Assembly.GetExecutingAssembly();
    internal static RandoMapMod Instance { get; private set; }

    internal static RmmSettings GS { get; private set; } = new();
    internal static RmmSettings LS { get; private set; } = new();

    public void OnLoadGlobal(RmmSettings s)
    {
        GS = s;
    }

    public RmmSettings OnSaveGlobal()
    {
        return GS;
    }

    public void OnLoadLocal(RmmSettings s)
    {
        LS = s;
    }

    public RmmSettings OnSaveLocal()
    {
        return LS;
    }

    public override string GetVersion()
    {
        return "3.6.6";
    }

    public override int LoadPriority()
    {
        return 10;
    }

    public override void Initialize()
    {
        LogDebug($"Initializing");

        if (ModHooks.GetMod("Randomizer 4") is null || ModHooks.GetMod("RandoMapCoreMod") is null)
        {
            return;
        }

        Interop.FindInteropMods();

        RmmMenuPage.Hook();
        RandoController.OnExportCompleted += InitializeLocalSettings;
        RandoController.OnCalculateHash += AdjustHash;
        SettingsLog.AfterLogSettings += LogRmmSettings;

        if (ModHooks.GetMod("RandoSettingsManager") is not null)
        {
            RandoSettingsManagerInterop.Hook();
        }

        RandoMapCoreMod.AddDataModule(new RmmDataModule());
        MapChanger.Localization.AddLocalizer(Localize);

        LogDebug($"Initialization complete.");
    }

    private static void InitializeLocalSettings(RandoController rc)
    {
        LS = GS.Clone();
    }

    private static int AdjustHash(RandoController rc, int orig)
    {
        return GS.GetSettingsHash();
    }

    private static void LogRmmSettings(LogArguments args, TextWriter tw)
    {
        tw.WriteLine("Logging RandoMapMod settings:");
        using Newtonsoft.Json.JsonTextWriter jtw = new(tw) { CloseOutput = false };
        RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, GS);
        tw.WriteLine();
    }

    private static string Localize(string text)
    {
        var localization = RandomizerMod.Localization.Localize(text);

        if (localization == text)
        {
            return null;
        }

        return localization;
    }
}
