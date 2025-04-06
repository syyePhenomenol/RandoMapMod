using UnityEngine;

namespace RandoMapMod.Input;

internal class DebugInput() : RmmGlobalHotkeyInput("Pin Debug Tool", "Misc", KeyCode.D)
{
    public override void DoAction()
    {
        Debugger.LogMapPosition();
    }
}
