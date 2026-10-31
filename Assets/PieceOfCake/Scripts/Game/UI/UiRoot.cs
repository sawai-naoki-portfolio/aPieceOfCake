using PieceOfCake.Game.Input;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>Entry point to the UI document: owns every screen and HUD component.</summary>
    public sealed class UiRoot
    {
        private readonly ScreenView[] screens;

        public UiRoot(UIDocument document, TouchControls touch, bool touchDevice)
        {
            var root = document.rootVisualElement;
            FontLoader.Apply(root);
            Title = new TitleScreen(root, touchDevice);
            Select = new StageSelectScreen(root);
            Pause = new PauseScreen(root);
            Result = new ResultScreen(root);
            Error = new ErrorScreen(root);
            Hud = new HudView(root);
            Bubbles = new BubbleLayer(root);
            Touch = new TouchControlsView(root, touch);
            screens = new ScreenView[] { Title, Select, Pause, Result, Error };
            ShowOnly(null);
            Hud.SetVisible(false);
            Touch.SetVisible(false);
        }

        public TitleScreen Title { get; }
        public StageSelectScreen Select { get; }
        public PauseScreen Pause { get; }
        public ResultScreen Result { get; }
        public ErrorScreen Error { get; }
        public HudView Hud { get; }
        public BubbleLayer Bubbles { get; }
        public TouchControlsView Touch { get; }

        /// <summary>Shows one overlay screen (or none) and hides the rest.</summary>
        public void ShowOnly(ScreenView screen)
        {
            foreach (var s in screens) s.SetVisible(s == screen);
        }
    }
}
