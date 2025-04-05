using MapChanger;
using RandoMapMod.Localization;

namespace RandoMapMod.Pins;

internal class BenchInfo(string name)
{
    internal bool IsActiveBench { get; private set; }
    internal bool IsVisitedBench { get; private set; }

    internal void Update()
    {
        IsVisitedBench = BenchwarpInterop.IsVisitedBench(name);
        IsActiveBench = RandoMapMod.GS.ShowBenchwarpPins && IsVisitedBench;
    }

    internal string GetBenchwarpText()
    {
        if (!IsActiveBench)
        {
            return null;
        }

        return $"{"Hold".L()} {Utils.GetBindingsText(new(InputHandler.Instance.inputActions.attack.Bindings))} {"to benchwarp".L()}.";
    }
}
