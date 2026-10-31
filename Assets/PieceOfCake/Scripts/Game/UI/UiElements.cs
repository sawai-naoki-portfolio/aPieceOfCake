using System;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>Lookup helpers that fail loudly when the UXML and the code disagree.</summary>
    public static class UiElements
    {
        public static T Required<T>(this VisualElement root, string name) where T : VisualElement
        {
            var element = root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"UI element '{name}' ({typeof(T).Name}) is missing from Main.uxml.");
            return element;
        }

        public static void SetVisible(this VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        public static Button Button(this VisualElement root, string name, string text, Action onClick)
        {
            var button = root.Required<Button>(name);
            button.text = text;
            if (onClick != null) button.clicked += onClick;
            return button;
        }
    }

    /// <summary>A full-screen overlay (title, menus, results).</summary>
    public abstract class ScreenView
    {
        protected ScreenView(VisualElement root, string name)
        {
            Element = root.Required<VisualElement>(name);
        }

        public VisualElement Element { get; }

        /// <summary>The control that receives keyboard and gamepad focus when the screen opens.</summary>
        protected abstract Focusable DefaultFocus { get; }

        public void SetVisible(bool visible)
        {
            Element.SetVisible(visible);
            if (visible) Element.schedule.Execute(() => DefaultFocus?.Focus());
        }
    }
}
