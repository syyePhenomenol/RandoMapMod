using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using UnityEngine;

namespace RandoMapMod.UI;

internal class MapKeyPanel
{
    private readonly Panel _mapKeyPanel;
    private readonly StackLayout _mapKeyContents;
    private readonly GridLayout _pinKey;
    private readonly GridLayout _roomKey;

    internal MapKeyPanel(LayoutRoot layout, StackLayout panelStack)
    {
        _mapKeyPanel = new(
            layout,
            SpriteManager.Instance.GetTexture("GUI.PanelLeft").ToSlicedSprite(200f, 50f, 100f, 50f),
            "Map Key Panel"
        )
        {
            MinHeight = 0f,
            MinWidth = 0f,
            Borders = new(0f, 20f, 20f, 20f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        ((Image)layout.GetElement("Map Key Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        panelStack.Children.Add(_mapKeyPanel);

        _mapKeyContents = new(layout, "Panel Contents")
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
            Spacing = 5f,
        };

        _mapKeyPanel.Child = _mapKeyContents;

        _pinKey = new(layout, "Pin Key")
        {
            MinWidth = 200f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
            },
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1.6f, GridUnit.Proportional),
            },
        };

        _mapKeyContents.Children.Add(_pinKey);

        var counter = 0;

        foreach (var colorSetting in RmmColors.PinBorderColors)
        {
            var pinPanel = new Panel(
                layout,
                SpriteManager.Instance.GetSprite("Pins.Blank"),
                colorSetting.ToString() + "Panel"
            )
            {
                MinHeight = 50f,
                MinWidth = 50f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
                .WithProp(GridLayout.Column, 0)
                .WithProp(GridLayout.Row, counter);

            var pin = new Image(
                layout,
                SpriteManager.Instance.GetSprite("Pins.Border"),
                colorSetting.ToString() + " Pin"
            )
            {
                Width = 50f,
                Height = 50f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
                .WithProp(GridLayout.Column, 0)
                .WithProp(GridLayout.Row, counter);

            ((Image)layout.GetElement(colorSetting.ToString() + " Pin")).Tint = RmmColors.GetColor(colorSetting);

            pinPanel.Child = pin;

            var text = new TextObject(layout, colorSetting.ToString() + " Text")
            {
                Text = Utils.ToCleanName(colorSetting.ToString().Replace("Pin_", "")).L(),
                Padding = new(10f, 0f, 0f, 0f),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
                .WithProp(GridLayout.Column, 1)
                .WithProp(GridLayout.Row, counter);

            _pinKey.Children.Add(pinPanel);
            _pinKey.Children.Add(text);

            counter++;
        }

        _roomKey = new(layout, "Room Key")
        {
            MinWidth = 200f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
            },
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1.6f, GridUnit.Proportional),
            },
        };

        _mapKeyContents.Children.Add(_roomKey);

        var roomCopy = GameManager
            .instance.gameMap.transform.GetChild(12)
            .transform.GetChild(26)
            .GetComponent<SpriteRenderer>()
            .sprite;

        counter = 0;

        foreach (var color in RmmColors.RoomColors)
        {
            var cleanRoomColor = Utils.ToCleanName(color.ToString().Replace("Room_", ""));

            var room = new Image(layout, roomCopy, cleanRoomColor + " Room")
            {
                Width = 40f,
                Height = 40f,
                Tint = RmmColors.GetColor(color),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(0f, 5f, 17f, 5f),
            }
                .WithProp(GridLayout.Column, 0)
                .WithProp(GridLayout.Row, counter);

            var text = new TextObject(layout, cleanRoomColor + " Text")
            {
                Text = cleanRoomColor.L(),
                Padding = new(10f, 0f, 0f, 0f),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
                .WithProp(GridLayout.Column, 1)
                .WithProp(GridLayout.Row, counter);

            _roomKey.Children.Add(room);
            _roomKey.Children.Add(text);

            counter++;
        }

        var highlighted = RmmColors.GetColor(RmmColorSetting.Room_Normal);
        highlighted.w = 1f;

        var roomHighlight = new Image(layout, roomCopy, "Highlighted Room")
        {
            Width = 40f,
            Height = 40f,
            Tint = highlighted,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new(0f, 5f, 17f, 5f),
        }
            .WithProp(GridLayout.Column, 0)
            .WithProp(GridLayout.Row, counter);

        var textHighlight = new TextObject(layout, "Highlighted Text")
        {
            Text = "Contains\nunchecked\ntransitions".L(),
            Padding = new(10f, 0f, 0f, 0f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        }
            .WithProp(GridLayout.Column, 1)
            .WithProp(GridLayout.Row, counter);

        _roomKey.Children.Add(roomHighlight);
        _roomKey.Children.Add(textHighlight);
    }

    internal void Update()
    {
        if (RandoMapMod.GS.MapKeyOn)
        {
            _mapKeyPanel.Visibility = Visibility.Visible;
        }
        else
        {
            _mapKeyPanel.Visibility = Visibility.Collapsed;
        }

        if (Conditions.TransitionRandoModeEnabled())
        {
            _roomKey.Visibility = Visibility.Visible;
        }
        else
        {
            _roomKey.Visibility = Visibility.Collapsed;
        }
    }
}
