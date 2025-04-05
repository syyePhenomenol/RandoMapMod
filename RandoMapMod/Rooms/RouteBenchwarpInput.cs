using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;

namespace RandoMapMod.Pins;

internal class RouteBenchwarpInput()
    : RmmMapInput("World Map Benchwarp", InputHandler.Instance.inputActions.attack, 500f)
{
    public override void DoAction()
    {
        if (PinSelector.Instance.VisitedBenchNotSelected() && TryGetBenchwarpKey(out var benchKey))
        {
            _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(benchKey));
        }
    }

    internal static bool TryGetBenchwarpKey(out RmmBenchKey key)
    {
        if (
            RmmPathfinder.RM.CurrentRoute is Route currentRoute
            && currentRoute.CurrentInstruction is BenchwarpAction ba
        )
        {
            key = ba.BenchKey;
            return true;
        }

        key = default;
        return false;
    }
}
