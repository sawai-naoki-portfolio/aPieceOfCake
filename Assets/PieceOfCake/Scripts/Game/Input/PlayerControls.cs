using System.Collections.Generic;
using PieceOfCake.Core;

namespace PieceOfCake.Game.Input
{
    /// <summary>One device's state for the current frame. Button fields mean "pressed this frame".</summary>
    public struct ControlSample
    {
        public bool Left;
        public bool Right;
        public bool Jump;
        public bool Cut;
        public bool Eat;
        public bool Pause;
        public bool Retry;
    }

    /// <summary>A source of player input (keyboard and gamepad, on-screen touch buttons, ...).</summary>
    public interface IControlSource
    {
        ControlSample Sample();
    }

    /// <summary>
    /// Merges every input source. Button presses are latched until the simulation consumes them,
    /// so a press is never lost between render frames and fixed simulation steps.
    /// </summary>
    public sealed class PlayerControls
    {
        private readonly List<IControlSource> sources = new List<IControlSource>();
        private bool left, right, jump, cut, eat;

        public bool PausePressed { get; private set; }
        public bool RetryPressed { get; private set; }

        public void Add(IControlSource source) => sources.Add(source);

        /// <summary>Call once per rendered frame before the game updates.</summary>
        public void Poll()
        {
            left = right = false;
            PausePressed = RetryPressed = false;
            foreach (var source in sources)
            {
                var s = source.Sample();
                left |= s.Left;
                right |= s.Right;
                jump |= s.Jump;
                cut |= s.Cut;
                eat |= s.Eat;
                PausePressed |= s.Pause;
                RetryPressed |= s.Retry;
            }
        }

        /// <summary>Input for one simulation step; latched presses are cleared afterwards.</summary>
        public InputFrame ConsumeFrame()
        {
            var frame = new InputFrame(left, right, jump, cut, eat);
            jump = cut = eat = false;
            return frame;
        }

        /// <summary>Drops latched presses (used while menus are open).</summary>
        public void ClearPresses() => jump = cut = eat = false;
    }
}
