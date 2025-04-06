using UnityEngine;

namespace RandoMapMod.Input;

internal abstract class MapUIKeyInput : RmmGlobalHotkeyInput
{
    internal MapUIKeyInput(string name, KeyCode key)
        : base(name, "UI", key) { }
}
