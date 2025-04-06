using InControl;
using MapChanger.Input;
using RandoMapMod.Modes;

namespace RandoMapMod.Input;

internal abstract class RmmWorldMapInput(string name, Func<PlayerAction> getPlayerAction, float holdMilliseconds = 0)
    : WorldMapInput(name, nameof(RandoMapMod), getPlayerAction, holdMilliseconds)
{
    public override bool UseCondition()
    {
        return RandomizerMod.RandomizerMod.IsRandoSave;
    }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && Conditions.RandoMapModEnabled();
    }
}
