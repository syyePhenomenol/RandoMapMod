using RandoMapMod.Localization;

namespace RandoMapMod.Pathfinder.Actions
{
    internal static class InstructionExtensions
    {
        internal static string ToArrowedText(this IInstruction i)
        {
            return $" -> {i.SourceText.LT()}";
        }
    }
}