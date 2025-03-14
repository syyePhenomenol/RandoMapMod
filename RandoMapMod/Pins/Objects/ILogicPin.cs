using RandomizerCore.Logic;

namespace RandoMapMod.Pins
{
    internal interface ILogicPin
    {
        LogicDef Logic { get; }
        HintDef HintDef { get; }
        void UpdateLogic();
    }
}