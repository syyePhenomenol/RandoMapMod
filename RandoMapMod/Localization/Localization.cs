using System.Text.RegularExpressions;
using MapChanger;

namespace RandoMapMod.Localization;

public static class Localization
{
    /// <summary>
    /// Localize text
    /// </summary>
    /// <param name="text"></param>
    public static string L(this string text)
    {
        return RandomizerMod.Localization.Localize(text);
    }

    /// <summary>
    /// Localize and clean text
    /// </summary>
    /// <param name="text"></param>
    public static string LC(this string text)
    {
        return text.L().ToCleanName();
    }

    /// <summary>
    /// Localize and clean text in transition format A[B]
    /// </summary>
    /// <param name="name"></param>
    public static string LT(this string name)
    {
        var match = Regex.Match(name, @"^(\w+)\[(\w+)\]$");

        if (match.Groups.Count == 3)
        {
            return $"{match.Groups[1].Value.LC()}[{match.Groups[2].Value.LC()}]";
        }

        return name.LC();
    }
}
