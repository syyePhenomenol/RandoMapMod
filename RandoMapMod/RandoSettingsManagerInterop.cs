using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoMapMod;

internal static class RandoSettingsManagerInterop
{
    public static void Hook()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new RmmSettingsProxy());
    }
}

internal class RmmSettingsProxy : RandoSettingsProxy<RmmSettings, string>
{
    public override string ModKey => nameof(RandoMapMod);

    public override VersioningPolicy<string> VersioningPolicy { get; } =
        new EqualityVersioningPolicy<string>(RandoMapMod.Instance.GetVersion());

    public override void ReceiveSettings(RmmSettings settings)
    {
        settings ??= new();
        RmmMenuPage.Instance.Mef.SetMenuValues(settings);
    }

    public override bool TryProvideSettings(out RmmSettings settings)
    {
        settings = RandoMapMod.GS;
        return settings.Any;
    }
}
