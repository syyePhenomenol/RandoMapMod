using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace RandoMapMod.UI
{
    internal abstract class ControlPanelText
    {
        protected private abstract string Name { get; }

        internal TextObject TextObject { get; private set; }

        internal void Make(LayoutRoot layout, StackLayout panelStack)
        {
            TextObject = new(layout, Name)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = HorizontalAlignment.Left,
                Font = MagicUI.Core.UI.TrajanNormal,
                FontSize = 14,
                Padding = new(0f, 2f, 0f, 2f)
            };

            panelStack.Children.Add(TextObject);
        }

        internal void Update()
        {
            if (ActiveCondition())
            {
                TextObject.Visibility = Visibility.Visible;
                TextObject.Text = GetText();
                TextObject.ContentColor = GetColor();
            }
            else
            {
                TextObject.Visibility = Visibility.Collapsed;
            }
        }

        protected private abstract bool ActiveCondition();

        protected private abstract Vector4 GetColor();

        protected private abstract string GetText();
    }
}
