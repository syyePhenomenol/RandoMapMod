using System.Diagnostics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pins;
using RandoMapMod.Transition;
using RandoMapMod.UI;

namespace RandoMapMod.Rooms;

internal class TransitionRoomSelector : RoomSelector
{
    private readonly Stopwatch _attackHoldTimer = new();

    internal static TransitionRoomSelector Instance { get; private set; }

    internal SelectRoomRouteInput SelectRoomRouteInput { get; } = new();
    internal RouteBenchwarpInput RouteBenchwarpInput { get; } = new();

    internal void Initialize(IEnumerable<ISelectable> rooms)
    {
        base.Initialize([SelectRoomRouteInput, RouteBenchwarpInput], rooms);

        Instance = this;
    }

    public override void OnMainUpdate(bool active)
    {
        base.OnMainUpdate(active);

        _attackHoldTimer.Reset();
    }

    private protected override bool ActiveByCurrentMode()
    {
        return Conditions.TransitionRandoModeEnabled();
    }

    private protected override bool ActiveByToggle()
    {
        return RandoMapMod.GS.RoomSelectionOn;
    }

    protected override void OnSelectionChanged()
    {
        RoomSelectionPanel.Instance.Update();
    }

    internal string GetText()
    {
        var instructions = GetInstructionText();
        var transitions = TransitionStringBuilder.GetUncheckedVisited(SelectedObject.Key);

        if (transitions is "")
        {
            return instructions;
        }

        return $"{instructions}\n\n{transitions}";
    }

    private string GetInstructionText()
    {
        var selectedScene = SelectedObject.Key;
        var text = "";

        text += $"{"Selected room".L()}: {selectedScene.LC()}.";

        if (selectedScene == Utils.CurrentScene())
        {
            text += $" {"You are here".L()}.";
        }

        var selectBindingText = SelectRoomRouteInput.GetBindingsText();
        text += $"\n\n{"Press".L()} {selectBindingText}";

        if (RmmPathfinder.RM.CanCycleRoute(selectedScene))
        {
            text += $" {"to change starting / final transitions of current route".L()}.";
        }
        else
        {
            text += $" {"to find a new route".L()}.";
        }

        var benchBindingText = RouteBenchwarpInput.GetBindingsText();
        if (PinSelector.Instance.VisitedBenchNotSelected() && RouteBenchwarpInput.TryGetBenchwarpKey(out var _))
        {
            text += $" {"Hold".L()} {benchBindingText} {"to benchwarp".L()}.";
        }

        return text;
    }
}
