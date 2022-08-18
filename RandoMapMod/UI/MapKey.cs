using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class MapKey : WorldMapStack
    {
        private static Panel panel;
        private static StackLayout panelContents;
        private static GridLayout pinKey;
        private static GridLayout roomKey;

        protected override void BuildStack()
        {
            panel = new(Root, SpriteManager.Instance.GetTexture("GUI.PanelLeft").ToSlicedSprite(200f, 50f, 100f, 50f), "Panel")
            {
                MinHeight = 0f,
                MinWidth = 0f,
                Borders = new(0f, 20f, 20f, 20f),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            ((Image)Root.GetElement("Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            Stack.Children.Add(panel);

            panelContents = new(Root, "Panel Contents")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Horizontal,
                Spacing = 5f
            };

            panel.Child = panelContents;

            pinKey = new(Root, "Pin Key")
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
                        new GridDimension(1, GridUnit.Proportional)
                    },
                ColumnDefinitions =
                    {
                        new GridDimension(1, GridUnit.Proportional),
                        new GridDimension(1.6f, GridUnit.Proportional)
                    },
            };

            panelContents.Children.Add(pinKey);

            int counter = 0;

            foreach (RmmColorSetting colorSetting in RmmColors.PinColors)
            {
                Panel pinPanel = new Panel(Root, SpriteManager.Instance.GetSprite("Pins.Blank"), colorSetting.ToString() + "Panel")
                {
                    MinHeight = 50f,
                    MinWidth = 50f,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, counter);

                Image pin = new Image(Root, SpriteManager.Instance.GetSprite("Pins.Border"), colorSetting.ToString() + " Pin")
                {
                    Width = 50f,
                    Height = 50f,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, counter);

                ((Image)Root.GetElement(colorSetting.ToString() + " Pin")).Tint = RmmColors.GetColor(colorSetting);

                pinPanel.Child = pin;

                TextObject text = new TextObject(Root, colorSetting.ToString() + " Text")
                {
                    Text = L.Localize(Utils.ToCleanName(colorSetting.ToString().Replace("Pin_", ""))),
                    Padding = new(10f, 0f, 0f, 0f),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, counter);

                pinKey.Children.Add(pinPanel);
                pinKey.Children.Add(text);

                counter++;
            }

            roomKey = new(Root, "Room Key")
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
                        new GridDimension(1, GridUnit.Proportional)
                    },
                ColumnDefinitions =
                    {
                        new GridDimension(1, GridUnit.Proportional),
                        new GridDimension(1.6f, GridUnit.Proportional)
                    },
            };

            panelContents.Children.Add(roomKey);

            Sprite roomCopy = GameManager.instance.gameMap.transform.GetChild(12).transform.GetChild(26).GetComponent<SpriteRenderer>().sprite;

            counter = 0;

            foreach (RmmColorSetting color in RmmColors.RoomColors)
            {
                string cleanRoomColor = Utils.ToCleanName(color.ToString().Replace("Room_", ""));

                Image room = new Image(Root, roomCopy, cleanRoomColor + " Room")
                {
                    Width = 40f,
                    Height = 40f,
                    Tint = RmmColors.GetColor(color),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new(0f, 5f, 17f, 5f),
                }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, counter);

                TextObject text = new TextObject(Root, cleanRoomColor + " Text")
                {
                    Text = L.Localize(cleanRoomColor),
                    Padding = new(10f, 0f, 0f, 0f),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, counter);

                roomKey.Children.Add(room);
                roomKey.Children.Add(text);

                counter++;
            }

            Vector4 highlighted = RmmColors.GetColor(RmmColorSetting.Room_Normal);
            highlighted.w = 1f;

            Image roomHighlight = new Image(Root, roomCopy, "Highlighted Room")
            {
                Width = 40f,
                Height = 40f,
                Tint = highlighted,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(0f, 5f, 17f, 5f),
            }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, counter);

            TextObject textHighlight = new TextObject(Root, "Highlighted Text")
            {
                Text = L.Localize("Contains\nunchecked\ntransitions"),
                Padding = new(10f, 0f, 0f, 0f),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, counter);

            roomKey.Children.Add(roomHighlight);
            roomKey.Children.Add(textHighlight);
        }

        protected override bool Condition()
        {
            return base.Condition() && Conditions.RandoMapModEnabled();
        }

        public override void Update()
        {
            if (RandoMapMod.GS.MapKeyOn)
            {
                panel.Visibility = Visibility.Visible;
            }
            else
            {
                panel.Visibility = Visibility.Hidden;
            }

            if (Conditions.TransitionRandoModeEnabled())
            {
                roomKey.Visibility = Visibility.Visible;
            }
            else
            {
                roomKey.Visibility = Visibility.Collapsed;
            }
        }
    }
}
