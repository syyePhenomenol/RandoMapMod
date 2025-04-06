using RandoMapMod.Pathfinder;
using RandoMapMod.UI;

namespace RandoMapMod.Input;

internal class PathfinderBenchwarpInput : MapUIKeyInput
{
    internal PathfinderBenchwarpInput()
        : base("Toggle Pathfinder Benchwarp", UnityEngine.KeyCode.B)
    {
        Instance = this;
    }

    internal static PathfinderBenchwarpInput Instance { get; private set; }

    public override bool UseCondition()
    {
        return base.UseCondition() && Interop.HasBenchwarp;
    }

    public override void DoAction()
    {
        RandoMapMod.GS.ToggleAllowBenchWarpSearch();
        RmmPathfinder.RM.ResetRoute();
        base.DoAction();
    }
}
