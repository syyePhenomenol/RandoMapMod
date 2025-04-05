using RandoMapMod.Localization;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal class LogicInfo
{
    private readonly LogicDef _logic;
    private readonly ProgressionManager _pm;
    private readonly ProgressionManager _pmNoSequenceBreak;

    internal LogicInfo(LogicDef logic, ProgressionManager pm, ProgressionManager pmNoSequenceBreak)
    {
        _logic = logic;
        _pm = pm;
        _pmNoSequenceBreak = pmNoSequenceBreak;
    }

    internal LogicState State { get; private set; }

    internal void Update()
    {
        if (_logic.CanGet(_pm))
        {
            State = LogicState.Reachable;
        }
        else if (_logic.CanGet(_pmNoSequenceBreak))
        {
            State = LogicState.ReachableSequenceBreak;
        }
        else
        {
            State = LogicState.Unreachable;
        }
    }

    internal string GetLogicText()
    {
        return $"{"Logic".L()}: {_logic.InfixSource}";
    }
}

internal enum LogicState
{
    Reachable,
    ReachableSequenceBreak,
    Unreachable,
}
