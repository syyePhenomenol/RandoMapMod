using MapChanger;
using RandoMapMod.Localization;

namespace RandoMapMod.Pins
{
    internal static class BenchExtensions
    {
        internal static bool IsVisitedBench(this IBenchPin benchPin)
        {
            return BenchwarpInterop.IsVisitedBench(benchPin.name);
        }
        
        internal static string GetBenchwarpText(this IBenchPin benchPin)
        {
            if (!benchPin.IsVisitedBench()) return "";

            List<InControl.BindingSource> attackBindings = new(InputHandler.Instance.inputActions.attack.Bindings);
            return $"\n\n{"Hold".L()} {Utils.GetBindingsText(attackBindings)} {"to benchwarp".L()}.";
        }
    }
}