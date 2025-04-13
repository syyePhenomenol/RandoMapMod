using MagicUI.Core;
using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Transition;
using UnityEngine;

namespace RandoMapMod.UI;

internal class QuickMapTransitions : MapUILayer
{
    private static StackLayout _stack;

    protected override bool Condition()
    {
        return Conditions.TransitionRandoModeEnabled() && States.QuickMapOpen;
    }

    public override void BuildLayout()
    {
        _stack = new(Root, "Quick Map Transitions")
        {
            Padding = new(0f, 20f, 20f, 0f),
            Orientation = Orientation.Vertical,
            Spacing = 20f,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };
    }

    public override void Update()
    {
        _stack.Children.Clear();

        if (!Condition())
        {
            return;
        }

        var tsd = new TransitionStringDef(Utils.CurrentScene());

        MakeTransitionsWithCompasses(tsd.Unchecked);

        foreach (var tsl in new TransitionStringList[] { tsd.VisitedOut, tsd.VisitedIn, tsd.VanillaOut, tsd.VanillaIn })
        {
            if (RandoMapMod.GS.ShowQuickMapCompasses is Settings.QuickMapCompassSetting.All)
            {
                MakeTransitionsWithCompasses(tsl);
                continue;
            }

            if (tsl.Placements.Any())
            {
                _stack.Children.Add(RightCenteredText($"Text {tsl.Header}", tsl.GetFullText()));
                continue;
            }
        }
    }

    private void MakeTransitionsWithCompasses(TransitionStringList tsl)
    {
        if (!tsl.Placements.Any())
        {
            return;
        }

        StackLayout listStack =
            new(Root, $"Quick Map Stack {tsl.Header}")
            {
                Orientation = Orientation.Vertical,
                Spacing = 10f,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };

        _stack.Children.Add(listStack);

        listStack.Children.Add(RightCenteredText($"Header {tsl.Header}", tsl.FormattedHeader));

        foreach (var placement in tsl.Placements)
        {
            StackLayout lineStack =
                new(Root, $"Quick Map Unchecked Line Stack {placement}")
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10f,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };

            listStack.Children.Add(lineStack);

            lineStack.Children.Add(
                RightCenteredText(
                    $"Unchecked Placement {placement.Key.Name}",
                    tsl.GetFormattedPlacement(placement.Key, placement.Value)
                )
            );

            if (RmmPathfinder.SD.TransitionActions.TryGetValue(placement.Key.Name, out var ta))
            {
                lineStack.Children.Add(
                    new QuickMapCompass(Root, placement.Key.Name, new TransitionCompassPosition(ta.CompassObj))
                );
            }
        }
    }

    private TextObject RightCenteredText(string name, string text)
    {
        return new TextObject(Root, name)
        {
            Text = text,
            ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = HorizontalAlignment.Right,
            Font = MagicUI.Core.UI.TrajanNormal,
            FontSize = 14,
        };
    }

    private class QuickMapCompass : ArrangableElement, IGameObjectWrapper
    {
        private static readonly Vector2 _correction = new(15.5f, 8.7f);
        private readonly TransitionCompassPosition _tcp;

        private readonly Vector2 _playerPosition;

        public QuickMapCompass(LayoutRoot onLayout, string transition, TransitionCompassPosition tcp)
            : base(onLayout, $"Quick Map Compass {transition}")
        {
            _tcp = tcp;
            _playerPosition = (Vector2)HeroController.instance?.gameObject?.transform?.position;

            GameObject = new(Name, typeof(SpriteRenderer));

            var sr = GameObject.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = "HUD";
            sr.sprite = new EmbeddedSprite("GUI.ArrowInCircle").Value;

            GameObject.layer = 5;
            UnityEngine.Object.DontDestroyOnLoad(GameObject);
            GameObject.SetActive(false);
        }

        public GameObject GameObject { get; }

        protected override void ArrangeOverride(Vector2 alignedTopLeftCorner)
        {
            var dir = (Vector2)_tcp.Value - _playerPosition;
            var angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90f;

            var screenPos = alignedTopLeftCorner + new Vector2(15f, 15f);
            var viewportPos = Camera.main.ScreenToViewportPoint(screenPos);
            Vector3 scaledPos =
                new(
                    (viewportPos.x * _correction.x * 2f) - _correction.x,
                    _correction.y - (viewportPos.y * _correction.y * 2f),
                    -25f
                );

            GameObject.transform.eulerAngles = new Vector3(0, 0, angle);
            GameObject.transform.position = scaledPos;
            GameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            GameObject.SetActive(IsEffectivelyVisible);
        }

        protected override void DestroyOverride()
        {
            UnityEngine.Object.Destroy(GameObject);
        }

        protected override Vector2 MeasureOverride()
        {
            return new(30f, 30f);
        }
    }
}
