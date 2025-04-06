using RandoMapMod.Input;
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

        var bindingsText = BenchwarpInput.Instance.GetBindingsText();
        return $"{"Hold".L()} {bindingsText} {"to benchwarp".L()}.";
    }
}
