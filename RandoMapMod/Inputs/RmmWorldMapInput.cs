using InControl;
using MapChanger.Input;
using RandoMapMod.Modes;

namespace RandoMapMod.Input;

internal abstract class RmmWorldMapInput(string name, Func<PlayerAction> getPlayerAction, float holdMilliseconds = 0)
    : WorldMapInput(name, nameof(RandoMapMod), getPlayerAction, holdMilliseconds)
{
    public override bool UseCondition()
    {
        return RandoMapMod.Data is not null && RandoMapMod.Data.IsCorrectSaveType;
    }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && Conditions.RandoMapModEnabled();
    }
}
