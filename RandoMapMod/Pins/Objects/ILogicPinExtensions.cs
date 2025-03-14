using ItemChanger;
using MapChanger;
using RandoMapMod.Localization;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins
{
    internal static class ILogicPinExtensions
    {
        internal static string GetLogicText(this ILogicPin pin)
        {
            return pin.Logic is not null ? $"\n\n{"Logic".L()}: {pin.Logic.InfixSource}" : "";
        }

        internal static string GetHintText(this ILogicPin pin)
        {
            if (pin.HintDef.Text is null) return "";

            if (RmmPinSelector.ShowHint) return pin.HintDef.Text;

            return $"\n\n{"Press".L()} {Utils.GetBindingsText(new(InputHandler.Instance.inputActions.quickCast.Bindings))} {"to reveal location hint".L()}.";
        }
    }
}