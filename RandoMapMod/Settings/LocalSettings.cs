using ConnectionMetadataInjector.Util;
using Newtonsoft.Json;
using RandoMapMod.Pins;

namespace RandoMapMod.Settings;

public class LocalSettings
{
    [JsonProperty]
    public bool InitializedPreviously { get; private set; } = false;

    [JsonProperty]
    public bool SpoilerOn { get; private set; } = false;

    [JsonProperty]
    public bool RandomizedOn { get; private set; } = true;

    [JsonProperty]
    public bool VanillaOn { get; private set; } = false;

    [JsonProperty]
    public List<string> AllPoolGroups { get; private set; }

    [JsonProperty]
    public HashSet<string> RandoLocationPoolGroups { get; private set; }

    [JsonProperty]
    public HashSet<string> RandoItemPoolGroups { get; private set; }

    [JsonProperty]
    public HashSet<string> VanillaLocationPoolGroups { get; private set; }

    [JsonProperty]
    public HashSet<string> VanillaItemPoolGroups { get; private set; }

    [JsonProperty]
    public Dictionary<string, PoolState> PoolSettings { get; private set; }

    [JsonProperty]
    public GroupBySetting GroupBy { get; private set; } = GroupBySetting.Location;

    internal void Initialize()
    {
        if (InitializedPreviously)
        {
            return;
        }

        AllPoolGroups = [];
        RandoLocationPoolGroups = [];
        RandoItemPoolGroups = [];
        VanillaLocationPoolGroups = [];
        VanillaItemPoolGroups = [];

        foreach (var pin in RmmPinManager.Pins.Values)
        {
            switch (pin.Def)
            {
                case RandomizedPinDef:
                    RandoLocationPoolGroups.UnionWith(pin.Def.LocationPoolGroups);
                    RandoItemPoolGroups.UnionWith(pin.Def.ItemPoolGroups);
                    break;
                case VanillaPinDef:
                case VanillaICPinDef:
                    VanillaLocationPoolGroups.UnionWith(pin.Def.LocationPoolGroups);
                    VanillaItemPoolGroups.UnionWith(pin.Def.ItemPoolGroups);
                    break;
                default:
                    break;
            }
        }

        // The following is done to ensure the correct ordering of pools.
        foreach (
            var poolGroup in Enum.GetValues(typeof(PoolGroup))
                .Cast<PoolGroup>()
                .Select(poolGroup => poolGroup.FriendlyName())
                .Where(poolGroup =>
                    RandoLocationPoolGroups.Contains(poolGroup)
                    || RandoItemPoolGroups.Contains(poolGroup)
                    || VanillaLocationPoolGroups.Contains(poolGroup)
                    || VanillaItemPoolGroups.Contains(poolGroup)
                )
        )
        {
            AllPoolGroups.Add(poolGroup);
        }

        foreach (
            var poolGroup in RandoLocationPoolGroups
                .Union(RandoItemPoolGroups)
                .Union(VanillaLocationPoolGroups)
                .Union(VanillaItemPoolGroups)
                .Where(poolGroup => !AllPoolGroups.Contains(poolGroup))
        )
        {
            AllPoolGroups.Add(poolGroup);
        }

        PoolSettings = AllPoolGroups.ToDictionary(poolGroup => poolGroup, poolGroup => PoolState.On);

        ResetPoolSettings();

        InitializedPreviously = true;
    }

    internal void ToggleGroupBy()
    {
        GroupBy = (GroupBySetting)(((int)GroupBy + 1) % Enum.GetNames(typeof(GroupBySetting)).Length);
        ResetPoolSettings();
    }

    internal void ToggleSpoilers()
    {
        SpoilerOn = !SpoilerOn;
    }

    internal void ToggleRandomized()
    {
        RandomizedOn = !RandomizedOn;
        ResetPoolSettings();
    }

    internal void ToggleVanilla()
    {
        VanillaOn = !VanillaOn;
        ResetPoolSettings();
    }

    internal bool IsActivePoolGroup(string poolGroup, string poolsCollection)
    {
        return GetPoolGroupSetting(poolGroup) switch
        {
            PoolState.On => true,
            PoolState.Off => false,
            PoolState.Mixed => poolsCollection switch
            {
                "Randomized" => RandomizedOn,
                "Vanilla" => VanillaOn,
                _ => true,
            },
            _ => true,
        };
    }

    internal bool IsRandomizedCustom()
    {
        if (GroupBy == GroupBySetting.Item)
        {
            if (!RandoItemPoolGroups.Any())
            {
                return false;
            }

            return (!RandomizedOn && RandoItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (RandomizedOn && RandoItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
        }
        else
        {
            if (!RandoLocationPoolGroups.Any())
            {
                return false;
            }

            return (!RandomizedOn && RandoLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (RandomizedOn && RandoLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
        }
    }

    internal bool IsVanillaCustom()
    {
        if (GroupBy == GroupBySetting.Item)
        {
            if (!VanillaItemPoolGroups.Any())
            {
                return false;
            }

            return (!VanillaOn && VanillaItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (VanillaOn && VanillaItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
        }
        else
        {
            if (!RandoLocationPoolGroups.Any())
            {
                return false;
            }

            return (!VanillaOn && VanillaLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (VanillaOn && VanillaLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
        }
    }

    internal PoolState GetPoolGroupSetting(string poolGroup)
    {
        if (PoolSettings.ContainsKey(poolGroup))
        {
            return PoolSettings[poolGroup];
        }

        return PoolState.Off;
    }

    internal void SetPoolGroupSetting(string poolGroup, PoolState state)
    {
        if (PoolSettings.ContainsKey(poolGroup))
        {
            PoolSettings[poolGroup] = state;
        }
    }

    internal void TogglePoolGroupSetting(string poolGroup)
    {
        if (!PoolSettings.ContainsKey(poolGroup))
        {
            return;
        }

        PoolSettings[poolGroup] = PoolSettings[poolGroup] switch
        {
            PoolState.Off => PoolState.On,
            PoolState.On => PoolState.Off,
            PoolState.Mixed => PoolState.On,
            _ => PoolState.On,
        };
    }

    /// <summary>
    /// Reset the PoolGroups that are active based on the RandomizedOn, VanillaOn and Group By settings.
    /// When an individual pool that by default contains a mixed of randomized/vanilla placements gets toggled,
    /// It will either be forced to "On" or "Off" and the corresponding affected RandommizedOn/VanillaOn setting
    /// appears as "Custom" in the UI.
    /// </summary>
    private void ResetPoolSettings()
    {
        foreach (var poolGroup in AllPoolGroups)
        {
            SetPoolGroupSetting(poolGroup, GetResetPoolState(poolGroup));
        }

        PoolState GetResetPoolState(string poolGroup)
        {
            bool isRando;
            bool isVanilla;

            if (GroupBy == GroupBySetting.Item)
            {
                isRando = RandoItemPoolGroups.Contains(poolGroup);
                isVanilla = VanillaItemPoolGroups.Contains(poolGroup);
            }
            else
            {
                isRando = RandoLocationPoolGroups.Contains(poolGroup);
                isVanilla = VanillaLocationPoolGroups.Contains(poolGroup);
            }

            if (isRando && isVanilla && RandomizedOn != VanillaOn)
            {
                return PoolState.Mixed;
            }

            if ((isRando && RandomizedOn) || (isVanilla && VanillaOn))
            {
                return PoolState.On;
            }

            return PoolState.Off;
        }
    }
}
