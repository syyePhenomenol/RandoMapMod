using System.Diagnostics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;
using RandoMapMod.Pins;
using RandoMapMod.Transition;
using RandoMapMod.UI;

namespace RandoMapMod.Rooms;

internal class TransitionRoomSelector : RoomSelector
{
    private readonly Stopwatch _attackHoldTimer = new();

    internal static TransitionRoomSelector Instance { get; private set; }

    public override void Initialize(IEnumerable<ISelectable> rooms)
    {
        base.Initialize(rooms);

        Instance = this;
    }

    public override void OnMainUpdate(bool active)
    {
        base.OnMainUpdate(active);

        _attackHoldTimer.Reset();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0051:Remove unused private members",
        Justification = "Used by Unity"
    )]
    private void Update()
    {
        if (InputHandler.Instance.inputActions.menuSubmit.WasPressed && Hotkeys.NoCtrl() && SelectedObject is not null)
        {
            _attackHoldTimer.Reset();

            _ = RmmPathfinder.RM.TryGetNextRouteTo(SelectedObject.Key);

            RouteText.Instance.Update();
            RouteSummaryText.Instance.Update();
            RoomSelectionPanel.Instance.Update();
        }

        if (InputHandler.Instance.inputActions.attack.WasPressed && Hotkeys.NoCtrl())
        {
            _attackHoldTimer.Restart();
        }

        if (InputHandler.Instance.inputActions.attack.WasReleased)
        {
            _attackHoldTimer.Reset();
        }

        // Disable this benchwarp if the pin selector has already selected a visited bench
        if (_attackHoldTimer.ElapsedMilliseconds >= 500)
        {
            _attackHoldTimer.Reset();

            if (PinSelector.Instance.VisitedBenchNotSelected() && TryGetBenchwarpKey(out var benchKey))
            {
                _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(benchKey));
            }
        }
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

    private static string GetInstructionText()
    {
        var selectedScene = Instance.SelectedObject.Key;
        var text = "";

        text += $"{"Selected room".L()}: {selectedScene.LC()}.";

        List<InControl.BindingSource> bindings = new(InputHandler.Instance.inputActions.menuSubmit.Bindings);

        if (selectedScene == Utils.CurrentScene())
        {
            text += $" {"You are here".L()}.";
        }

        text += $"\n\n{"Press".L()} {Utils.GetBindingsText(bindings)}";

        if (RmmPathfinder.RM.CanCycleRoute(selectedScene))
        {
            text += $" {"to change starting / final transitions of current route".L()}.";
        }
        else
        {
            text += $" {"to find a new route".L()}.";
        }

        if (PinSelector.Instance.VisitedBenchNotSelected() && TryGetBenchwarpKey(out var _))
        {
            bindings = new(InputHandler.Instance.inputActions.attack.Bindings);

            text += $" {"Hold".L()} {Utils.GetBindingsText(bindings)} {"to benchwarp".L()}.";
        }

        return text;
    }

    private static bool TryGetBenchwarpKey(out RmmBenchKey key)
    {
        if (
            RmmPathfinder.RM.CurrentRoute is Route currentRoute
            && currentRoute.CurrentInstruction is BenchwarpAction ba
        )
        {
            key = ba.BenchKey;
            return true;
        }

        key = default;
        return false;
    }
}
