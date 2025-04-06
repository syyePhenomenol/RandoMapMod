using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;
using RandoMapMod.Pins;

namespace RandoMapMod.Input;

internal class BenchwarpInput : RmmWorldMapInput
{
    internal BenchwarpInput()
        : base("World Map Benchwarp", () => InputHandler.Instance.inputActions.attack, 500f)
    {
        Instance = this;
    }

    internal static BenchwarpInput Instance { get; private set; }

    public override bool UseCondition()
    {
        return base.UseCondition() && Interop.HasBenchwarp;
    }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && (RandoMapMod.GS.PinSelectionOn || RandoMapMod.GS.RoomSelectionOn);
    }

    public override void DoAction()
    {
        if (PinSelector.Instance.SelectedObject?.Key is string key && BenchwarpInterop.IsVisitedBench(key))
        {
            _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(key));
        }
        else if (TryGetBenchwarpFromRoute(out var benchKey))
        {
            _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(benchKey));
        }
    }

    internal static bool TryGetBenchwarpFromRoute(out RmmBenchKey key)
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
