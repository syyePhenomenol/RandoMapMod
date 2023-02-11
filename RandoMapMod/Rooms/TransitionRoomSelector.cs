using System.Diagnostics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Instructions;
using RandoMapMod.Transition;
using RandoMapMod.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Rooms
{
    internal class TransitionRoomSelector : RoomSelector
    {
        internal static TransitionRoomSelector Instance;

        internal override void Initialize(IEnumerable<MapObject> rooms)
        {
            Instance = this;

            base.Initialize(rooms);
        }

        public override void OnMainUpdate(bool active)
        {
            base.OnMainUpdate(active);

            attackHoldTimer.Reset();
        }

        private readonly Stopwatch attackHoldTimer = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        private void Update()
        {
            if (InputHandler.Instance.inputActions.menuSubmit.WasPressed
                && SelectedObjectKey is not NONE_SELECTED)
            {
                attackHoldTimer.Reset();

                RouteManager.TryGetNextRouteTo(SelectedObjectKey);

                RouteText.Instance.Update();
                RouteSummaryText.Instance.Update();
                SelectionPanels.UpdateRoomPanel();
            }

            if (InputHandler.Instance.inputActions.attack.WasPressed)
            {
                attackHoldTimer.Restart();
            }

            if (InputHandler.Instance.inputActions.attack.WasReleased)
            {
                attackHoldTimer.Reset();
            }

            // Disable this benchwarp if the pin selector has already selected a bench
            if (attackHoldTimer.ElapsedMilliseconds >= 500)
            {
                attackHoldTimer.Reset();

                if (!RmmPinSelector.Instance.BenchSelected())
                {
                    TryBenchwarp();
                }
            }
        }

        protected private override bool ActiveByCurrentMode()
        {
            return Conditions.TransitionRandoModeEnabled();
        }

        protected private override bool ActiveByToggle()
        {
            return RandoMapMod.GS.RoomSelectionOn;
        }

        protected override void OnSelectionChanged()
        {
            SelectionPanels.UpdateRoomPanel();
        }

        internal string GetText()
        {
            string instructions = GetInstructionText();
            string transitions = TransitionData.GetUncheckedVisited(SelectedObjectKey);

            if (transitions is "") return instructions;

            return $"{instructions}\n\n{transitions}";
        }

        private static void TryBenchwarp()
        {
            if (Interop.HasBenchwarp() && RouteManager.CurrentRoute is not null && RouteManager.CurrentRoute.RemainingInstructions.First() is StartWarpInstruction or BenchwarpInstruction)
            {
                GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(RouteManager.CurrentRoute.RemainingInstructions.First().Text));
            }
        }

        private static string GetInstructionText()
        {
            string selectedScene = Instance.SelectedObjectKey;
            string text = "";

            text += $"{L.Localize("Selected room")}: {selectedScene}.";

            List<InControl.BindingSource> bindings = new(InputHandler.Instance.inputActions.menuSubmit.Bindings);

            if (selectedScene == Utils.CurrentScene())
            {
                text += $" {L.Localize("You are here")}.";
            }

            text += $"\n\n{L.Localize("Press")} {Utils.GetBindingsText(bindings)}";

            if (RouteManager.CanCycleRoute(selectedScene))
            {
                text += $" {L.Localize("to change starting / final transitions of current route")}.";
            }
            else
            {
                text += $" {L.Localize("to find a new route")}.";
            }

            if (!RmmPinSelector.Instance.BenchSelected() && RouteManager.TryGetBenchwarpKey(out RmmBenchKey _))
            {
                bindings = new(InputHandler.Instance.inputActions.attack.Bindings);

                text += $" {L.Localize("Hold")} {Utils.GetBindingsText(bindings)} {L.Localize("to benchwarp")}.";
            }

            return text;
        }
    }
}
