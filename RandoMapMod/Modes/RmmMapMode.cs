using MapChanger;
using RandoMapMod.Transition;

namespace RandoMapMod.Modes;

internal class RmmMapMode : MapMode
{
    public override float Priority => 0f;
    public override bool ForceHasMap => true;
    public override bool ForceHasQuill => RandoMapMod.GS.AlwaysHaveQuill;
    public override bool? VanillaPins => false;
    public override bool? MapMarkers => RandoMapMod.GS.ShowMapMarkers ? null : false;
    public override bool ImmediateMapUpdate => true;
    public override bool FullMap => true;

    public override bool InitializeToThis()
    {
        if (TransitionData.IsTransitionRando)
        {
            return ModeName == RandoMapMod.GS.DefaultTransitionRandoMode.ToString().ToCleanName();
        }

        return ModeName == RandoMapMod.GS.DefaultItemRandoMode.ToString().ToCleanName();
    }
}
