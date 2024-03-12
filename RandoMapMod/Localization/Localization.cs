using System.Text.RegularExpressions;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Localization
{
    public static class Localization
    {
        public static string ToLocalizeInstructionName(this string name)
        {
            var match = Regex.Match(name, @"^(\w+)\[(\w+)\]$");

            if (match.Groups.Count == 3)
            {
                return $"{L.Localize(match.Groups[1].Value)}[{L.Localize(match.Groups[2].Value)}]";
            }
            return L.Localize(name) ;
        }

    }
}