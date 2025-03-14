using ConnectionMetadataInjector.Util;
using RandoMapMod.Pins;

namespace RandoMapMod.Settings
{
    public class LocalSettings
    {
        public bool InitializedPreviously = false;

        public bool SpoilerOn = false;
        public bool RandomizedOn = true;
        public bool VanillaOn = false;

        public List<string> AllPoolGroups;
        public HashSet<string> RandoLocationPoolGroups;
        public HashSet<string> RandoItemPoolGroups;
        public HashSet<string> VanillaLocationPoolGroups;
        public HashSet<string> VanillaItemPoolGroups;

        public Dictionary<string, PoolState> PoolSettings;

        public GroupBySetting GroupBy = GroupBySetting.Location;

        internal void Initialize()
        {
            if (InitializedPreviously) return;

            AllPoolGroups = [];
            RandoLocationPoolGroups = [];
            RandoItemPoolGroups = [];
            VanillaLocationPoolGroups = [];
            VanillaItemPoolGroups = [];

            foreach (RmmPin pin in RmmPinManager.Pins.Values)
            {
                if (pin is RandomizedPin)
                {
                    RandoLocationPoolGroups.UnionWith(pin.LocationPoolGroups);
                    RandoItemPoolGroups.UnionWith(pin.ItemPoolGroups);
                }
                if (pin is VanillaPin or NonRandoPin)
                {
                    VanillaLocationPoolGroups.UnionWith(pin.LocationPoolGroups);
                    VanillaItemPoolGroups.UnionWith(pin.ItemPoolGroups);
                }
            }

            foreach (string poolGroup in Enum.GetValues(typeof(PoolGroup))
                .Cast<PoolGroup>()
                .Select(poolGroup => poolGroup.FriendlyName())
                .Where(poolGroup => RandoLocationPoolGroups.Contains(poolGroup)
                    || RandoItemPoolGroups.Contains(poolGroup)
                    || VanillaLocationPoolGroups.Contains(poolGroup)
                    || VanillaItemPoolGroups.Contains(poolGroup)))
            {
                AllPoolGroups.Add(poolGroup);
            }
            foreach (string poolGroup in RandoLocationPoolGroups
                .Union(RandoItemPoolGroups)
                .Union(VanillaLocationPoolGroups)
                .Union(VanillaItemPoolGroups)
                .Where(poolGroup => !AllPoolGroups.Contains(poolGroup)))
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

        internal bool IsActivePoolGroup(string poolGroup, PoolsCollection poolsCollection)
        {
            return GetPoolGroupSetting(poolGroup) switch
            {
                PoolState.On => true,
                PoolState.Off => false,
                PoolState.Mixed => poolsCollection switch
                {
                    PoolsCollection.Randomized => RandomizedOn,
                    PoolsCollection.Vanilla => VanillaOn,
                    _ => true
                },
                _ => true,
            };
        }

        internal bool IsRandomizedCustom()
        {
            if (GroupBy == GroupBySetting.Item)
            {
                if (!RandoItemPoolGroups.Any()) return false;

                return (!RandomizedOn && RandoItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (RandomizedOn && RandoItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
            }
            else
            {
                if (!RandoLocationPoolGroups.Any()) return false;

                return (!RandomizedOn && RandoLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (RandomizedOn && RandoLocationPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
            }
        }

        internal bool IsVanillaCustom()
        {
            if (GroupBy == GroupBySetting.Item)
            {
                if (!VanillaItemPoolGroups.Any()) return false;

                return (!VanillaOn && VanillaItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.On))
                || (VanillaOn && VanillaItemPoolGroups.Any(group => GetPoolGroupSetting(group) == PoolState.Off));
            }
            else
            {
                if (!RandoLocationPoolGroups.Any()) return false;

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
            if (!PoolSettings.ContainsKey(poolGroup)) return;

            PoolSettings[poolGroup] = PoolSettings[poolGroup] switch
            {
                PoolState.Off => PoolState.On,
                PoolState.On => PoolState.Off,
                PoolState.Mixed => PoolState.On,
                _ => PoolState.On
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
            foreach (string poolGroup in AllPoolGroups)
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
}