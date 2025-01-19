using System.Collections;
using System.Diagnostics;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.UI;
using UnityEngine;

namespace RandoMapMod.Pins
{
    internal class RmmPinSelector : Selector
    {
        internal static RmmPinSelector Instance { get; private set; }

        internal static IEnumerable<ISelectable> HighlightedRooms { get; private set; }

        internal static bool ShowHint { get; private set; }

        private const int HIGHLIGHT_HALF_PERIOD = 25;
        private const int HIGHLIGHT_PERIOD = HIGHLIGHT_HALF_PERIOD * 2;

        private static int highlightAnimationTick = 0;

        private Coroutine animateHighlightedRooms;
        private IEnumerator AnimateHighlightedRooms()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(UpdateWaitSeconds);

                highlightAnimationTick = (highlightAnimationTick + 1) % HIGHLIGHT_PERIOD;

                Vector4 color = RmmColors.GetColor(RmmColorSetting.Room_Highlighted);
                color.w = 0.3f + TriangleWave(highlightAnimationTick) * 0.7f;

                if (HighlightedRooms is null) continue;
                
                foreach (ISelectable room in HighlightedRooms)
                {
                    if (room is ColoredMapObject cmo)
                    {
                        cmo.Color = color;
                    }
                }
            }

            static float TriangleWave(float x)
            {
                return Math.Abs(x - HIGHLIGHT_HALF_PERIOD) / HIGHLIGHT_HALF_PERIOD;
            }
        }

        private void StartAnimateHighlightedRooms()
        {
            animateHighlightedRooms ??= StartCoroutine(AnimateHighlightedRooms());
        }

        private void StopAnimateHighlightedRooms()
        {
            if (animateHighlightedRooms is not null)
            {
                StopCoroutine(AnimateHighlightedRooms());
                animateHighlightedRooms = null;
            }
        }

        internal void Initialize(IEnumerable<RmmPin> pins)
        {
            Instance = this;

            base.Initialize();

            ActiveModifiers.AddRange
            (
                new Func<bool>[]
                { 
                    ActiveByCurrentMode,
                    ActiveByToggle
                }
            );

            foreach (RmmPin pin in pins)
            {
                if (Objects.TryGetValue(pin.name, out List<ISelectable> selectables))
                {
                    selectables.Add(pin);
                    continue;
                }

                Objects[pin.name] = [pin];
            }
        }

        private readonly Stopwatch attackHoldTimer = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        private void Update()
        {
            // Press dream nail to toggle lock selection
            if (InputHandler.Instance.inputActions.dreamNail.WasPressed && Hotkeys.NoCtrl())
            {
                ToggleLockSelection();
                PinSelectionPanel.Instance.Update();
            }

            // Press quick cast for location hint
            if (InputHandler.Instance.inputActions.quickCast.WasPressed && Hotkeys.NoCtrl())
            {
                if (!ShowHint)
                {
                    ShowHint = true;
                    PinSelectionPanel.Instance.Update();
                }
            }

            // Hold attack to benchwarp
            if (InputHandler.Instance.inputActions.attack.WasPressed && Hotkeys.NoCtrl())
            {
                attackHoldTimer.Restart();
            }

            if (InputHandler.Instance.inputActions.attack.WasReleased)
            {
                attackHoldTimer.Reset();
            }

            if (attackHoldTimer.ElapsedMilliseconds >= 500)
            {
                attackHoldTimer.Reset();

                if (VisitedBenchSelected())
                {
                    GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(SelectedObjectKey));
                }
            }
        }

        public override void OnMainUpdate(bool active)
        {
            base.OnMainUpdate(active);

            SpriteObject.SetActive(RandoMapMod.GS.ShowReticle);

            if (active)
            {
                StartAnimateHighlightedRooms();
            }
            else
            {
                StopAnimateHighlightedRooms();
            }
        }

        protected override void Select(ISelectable selectable)
        {
            if (selectable is not RmmPin pin) return;

            // RandoMapMod.Instance.LogDebug($"Selected {pin.name}");
            pin.Selected = true;

            if (pin is AbstractPlacementsPin app && app.HighlightRooms is not null)
            {
                HighlightedRooms = app.HighlightRooms;
            }
        }

        protected override void Deselect(ISelectable selectable)
        {
            if (selectable is not RmmPin pin) return;

            // RandoMapMod.Instance.LogDebug($"Deselected {pin.name}");
            pin.Selected = false;

            if (HighlightedRooms is null) return;

            foreach (ISelectable room in HighlightedRooms)
            {
                if (room is ColoredMapObject cmo)
                {
                    cmo.UpdateColor();
                }
            }
            HighlightedRooms = null;
        }

        protected override void OnSelectionChanged()
        {
            ShowHint = false;
            SelectionPanels.Instance.Update();
        }

        private bool ActiveByCurrentMode()
        {
            return Conditions.RandoMapModEnabled();
        }

        private bool ActiveByToggle()
        {
            return RandoMapMod.GS.PinSelectionOn;
        }

        internal bool VisitedBenchSelected()
        {
            return Interop.HasBenchwarp && BenchwarpInterop.IsVisitedBench(SelectedObjectKey);
        }
    }
}
