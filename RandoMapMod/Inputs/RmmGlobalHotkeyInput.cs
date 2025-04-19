using MapChanger.Input;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using UnityEngine;

namespace RandoMapMod.Input;

internal abstract class RmmGlobalHotkeyInput(string name, string category, KeyCode key)
    : GlobalHotkeyInput(name, $"RMM {category}", key)
{
    public override bool UseCondition()
    {
        return RandoMapMod.Data is not null && RandoMapMod.Data.IsCorrectSaveType;
    }

    public override bool ActiveCondition()
    {
        return Conditions.RandoMapModEnabled();
    }

    public override void DoAction()
    {
        PauseMenu.Update();
        RmmPinManager.MainUpdate();
        PinSelector.Instance.MainUpdate();
        TransitionRoomSelector.Instance.MainUpdate();
        MapUILayerUpdater.Update();
    }
}
