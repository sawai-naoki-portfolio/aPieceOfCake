namespace PieceOfCake.Game.Input
{
    public enum TouchButton
    {
        Left,
        Right,
        Jump,
        Cut,
        Eat,
    }

    /// <summary>State of the on-screen buttons, written by the UI and read like any other device.</summary>
    public sealed class TouchControls : IControlSource
    {
        private bool left, right, jump, cut, eat;

        public void Press(TouchButton button)
        {
            switch (button)
            {
                case TouchButton.Left: left = true; break;
                case TouchButton.Right: right = true; break;
                case TouchButton.Jump: jump = true; break;
                case TouchButton.Cut: cut = true; break;
                case TouchButton.Eat: eat = true; break;
            }
        }

        public void Release(TouchButton button)
        {
            if (button == TouchButton.Left) left = false;
            else if (button == TouchButton.Right) right = false;
        }

        public void ReleaseAll() => left = right = false;

        public ControlSample Sample()
        {
            var sample = new ControlSample { Left = left, Right = right, Jump = jump, Cut = cut, Eat = eat };
            jump = cut = eat = false;
            return sample;
        }
    }
}
