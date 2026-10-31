namespace PieceOfCake.Core
{
    /// <summary>
    /// Player intent for one fixed simulation step. "Pressed" flags are edge-triggered and must be
    /// true for exactly one step per button press.
    /// </summary>
    public readonly struct InputFrame
    {
        public readonly bool Left;
        public readonly bool Right;
        public readonly bool JumpPressed;
        public readonly bool CutPressed;
        public readonly bool EatPressed;

        public InputFrame(bool left, bool right, bool jumpPressed, bool cutPressed, bool eatPressed)
        {
            Left = left;
            Right = right;
            JumpPressed = jumpPressed;
            CutPressed = cutPressed;
            EatPressed = eatPressed;
        }

        /// <summary>-1, 0 or +1.</summary>
        public int Direction => (Right ? 1 : 0) - (Left ? 1 : 0);
    }

    public enum GamePhase
    {
        /// <summary>Normal play.</summary>
        Playing,
        /// <summary>Order delivered; a short celebration runs before the result.</summary>
        Delivering,
        /// <summary>Stage finished; <see cref="StageSimulation.Result"/> is available.</summary>
        Completed,
    }
}
