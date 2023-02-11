using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RandomizerMod.RandomizerData;

namespace RandoMapMod.Transition
{
    public record RmmTransitionDef
    {
        public string Name => $"{SceneName}[{DoorName}]";
        public string SceneName { get; init; }
        public string DoorName { get; init; }

        [JsonConstructor]
        public RmmTransitionDef(string sceneName, string doorName)
        {
            SceneName = sceneName;
            DoorName = doorName;
        }

        public RmmTransitionDef(TransitionDef td)
        {
            SceneName = td.SceneName;
            DoorName = td.DoorName;
        }

        public static bool TryMake(string str, out RmmTransitionDef td)
        {
            var match = Regex.Match(str, @"^(\w+)\[(\w+)\]$");

            if (match.Groups.Count == 3)
            {
                td = new(match.Groups[1].Value, match.Groups[2].Value);
                return true;
            }

            td = default;
            return false;
        }
    }
}
