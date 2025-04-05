using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Pathfinder;

internal static class StateBuilderExtensions
{
    internal static bool TrySetStateBool(this StateBuilder sb, StateManager sm, string name, bool value)
    {
        if (sm.GetBool(name) is StateBool stateBool)
        {
            sb.SetBool(stateBool, value);
            return true;
        }

        return false;
    }

    internal static bool TrySetStateInt(this StateBuilder sb, StateManager sm, string name, int value)
    {
        if (sm.GetInt(name) is StateInt stateBool)
        {
            sb.SetInt(stateBool, value);
            return true;
        }

        return false;
    }
}
