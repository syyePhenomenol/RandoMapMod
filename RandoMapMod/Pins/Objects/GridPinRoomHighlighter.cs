using System.Collections.ObjectModel;
using MapChanger.MonoBehaviours;
using UnityEngine;

namespace RandoMapMod.Pins;

internal class GridPinRoomHighlighter : MonoBehaviour, IPeriodicUpdater
{
    private const int HIGHLIGHT_HALF_PERIOD = 25;
    private const int HIGHLIGHT_PERIOD = HIGHLIGHT_HALF_PERIOD * 2;

    private GridPin _selectedGridPin;
    private int _highlightAnimationTick = 0;
    private Coroutine _animateHighlightedRooms;

    public float UpdateWaitSeconds => 0.02f;

    internal GridPin SelectedGridPin
    {
        get => _selectedGridPin;
        set
        {
            if (value == _selectedGridPin)
            {
                return;
            }

            if (_selectedGridPin?.HighlightRooms is ReadOnlyCollection<ColoredMapObject> highlightRooms)
            {
                foreach (var r in highlightRooms)
                {
                    r.UpdateColor();
                }
            }

            _selectedGridPin = value;
        }
    }

    public System.Collections.IEnumerator PeriodicUpdate()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(UpdateWaitSeconds);

            _highlightAnimationTick = (_highlightAnimationTick + 1) % HIGHLIGHT_PERIOD;

            var brightness = 0.3f + (TriangleWave(_highlightAnimationTick) * 0.7f);

            if (_selectedGridPin?.HighlightRooms is ReadOnlyCollection<ColoredMapObject> rooms)
            {
                foreach (var room in rooms)
                {
                    room.Color = new(room.Color.x, room.Color.y, room.Color.z, brightness);
                }
            }
        }

        static float TriangleWave(float x) => Math.Abs(x - HIGHLIGHT_HALF_PERIOD) / HIGHLIGHT_HALF_PERIOD;
    }

    internal void StartAnimateHighlightedRooms()
    {
        _animateHighlightedRooms ??= StartCoroutine(PeriodicUpdate());
    }

    internal void StopAnimateHighlightedRooms()
    {
        if (_animateHighlightedRooms is not null)
        {
            StopCoroutine(PeriodicUpdate());
            _animateHighlightedRooms = null;
        }
    }
}
