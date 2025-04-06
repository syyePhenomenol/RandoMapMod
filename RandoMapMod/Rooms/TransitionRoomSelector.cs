using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Input;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pins;
using RandoMapMod.Transition;
using RandoMapMod.UI;

namespace RandoMapMod.Rooms;

internal class TransitionRoomSelector : RoomSelector
{
    internal static TransitionRoomSelector Instance { get; private set; }

    public override void Initialize(IEnumerable<ISelectable> rooms)
    {
        base.Initialize(rooms);

        Instance = this;
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

        var selectBindingText = SelectRoomRouteInput.Instance.GetBindingsText();
        text += $"\n\n{"Press".L()} {selectBindingText}";

        if (RmmPathfinder.RM.CanCycleRoute(selectedScene))
        {
            text += $" {"to change starting / final transitions of current route".L()}.";
        }
        else
        {
            text += $" {"to find a new route".L()}.";
        }

        var benchBindingText = BenchwarpInput.Instance.GetBindingsText();
        if (PinSelector.Instance.VisitedBenchNotSelected() && BenchwarpInput.TryGetBenchwarpFromRoute(out var _))
        {
            text += $" {"Hold".L()} {benchBindingText} {"to benchwarp".L()}.";
        }

        return text;
    }
}
