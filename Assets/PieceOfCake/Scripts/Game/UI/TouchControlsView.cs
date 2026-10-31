using PieceOfCake.Game.Input;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>On-screen buttons for touch devices. Supports multi-touch (move and jump at once).</summary>
    public sealed class TouchControlsView
    {
        private const string PressedClass = "pressed";
        private readonly VisualElement element;
        private readonly TouchControls touch;

        public TouchControlsView(VisualElement root, TouchControls touch)
        {
            this.touch = touch;
            element = root.Required<VisualElement>("touch");
            element.Required<Label>("touch-jump-label").text = Texts.JumpButton;
            element.Required<Label>("touch-cut-label").text = Texts.CutButton;
            element.Required<Label>("touch-eat-label").text = Texts.EatButton;
            Bind("touch-left", TouchButton.Left);
            Bind("touch-right", TouchButton.Right);
            Bind("touch-jump", TouchButton.Jump);
            Bind("touch-cut", TouchButton.Cut);
            Bind("touch-eat", TouchButton.Eat);
        }

        public void SetVisible(bool visible)
        {
            element.SetVisible(visible);
            if (!visible) touch.ReleaseAll();
        }

        private void Bind(string name, TouchButton button)
        {
            var target = element.Required<VisualElement>(name);
            target.RegisterCallback<PointerDownEvent>(e =>
            {
                target.CapturePointer(e.pointerId);
                target.AddToClassList(PressedClass);
                touch.Press(button);
                e.StopPropagation();
            });

            void Release(int pointerId)
            {
                if (target.HasPointerCapture(pointerId)) target.ReleasePointer(pointerId);
                target.RemoveFromClassList(PressedClass);
                touch.Release(button);
            }

            target.RegisterCallback<PointerUpEvent>(e => Release(e.pointerId));
            target.RegisterCallback<PointerCancelEvent>(e => Release(e.pointerId));
            target.RegisterCallback<PointerCaptureOutEvent>(e => Release(e.pointerId));
        }
    }
}
