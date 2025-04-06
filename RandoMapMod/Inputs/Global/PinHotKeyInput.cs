using UnityEngine;

namespace RandoMapMod.Input;

internal abstract class PinHotkeyInput : RmmGlobalHotkeyInput
{
    internal PinHotkeyInput(string name, KeyCode key)
        : base(name, "Pins", key) { }
}
