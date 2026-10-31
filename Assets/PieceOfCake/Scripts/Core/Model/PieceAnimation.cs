using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>A short scripted motion of a piece. While animating, a piece cannot be pushed, cut or eaten.</summary>
    public abstract class PieceAnimation
    {
        protected PieceAnimation(double duration)
        {
            Duration = duration;
        }

        public double Duration { get; }
        public double Elapsed { get; internal set; }
        public double Progress => MathUtil.Clamp(Elapsed / Duration, 0, 1);
        public bool IsFinished => Elapsed >= Duration;
    }

    /// <summary>Horizontal slide by one cell after a push.</summary>
    public sealed class SlideAnimation : PieceAnimation
    {
        internal SlideAnimation(double fromX) : base(SimConstants.SlideDuration)
        {
            FromX = fromX;
        }

        public double FromX { get; }
    }

    /// <summary>
    /// A one-cell-wide column falling over to become a horizontal bridge. The piece already holds its
    /// final (horizontal) cells; this records the original column for drawing the rotation.
    /// </summary>
    public sealed class ToppleAnimation : PieceAnimation
    {
        internal ToppleAnimation(int direction, IReadOnlyList<Cell> originalColumn, double pivotX, double pivotY)
            : base(SimConstants.ToppleDuration)
        {
            Direction = direction;
            OriginalColumn = originalColumn;
            PivotX = pivotX;
            PivotY = pivotY;
        }

        /// <summary>+1 falls to the right, -1 to the left.</summary>
        public int Direction { get; }
        /// <summary>Cells of the standing column from top to bottom.</summary>
        public IReadOnlyList<Cell> OriginalColumn { get; }
        /// <summary>Centre of the bottom cell, in simulation pixels.</summary>
        public double PivotX { get; }
        public double PivotY { get; }

        /// <summary>Rotation in radians (eased), positive is clockwise on screen for a rightward fall.</summary>
        public double Angle => Direction * System.Math.PI / 2 * Progress * Progress;
    }
}
