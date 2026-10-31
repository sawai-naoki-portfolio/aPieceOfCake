using System;

namespace PieceOfCake.Core
{
    /// <summary>Small numeric helpers shared by the simulation and its views.</summary>
    public static class MathUtil
    {
        public static double Clamp(double value, double min, double max) =>
            value < min ? min : value > max ? max : value;

        public static double Lerp(double from, double to, double t) => from + (to - from) * t;

        /// <summary>Moves <paramref name="value"/> towards <paramref name="target"/> by at most <paramref name="delta"/>.</summary>
        public static double Approach(double value, double target, double delta) =>
            value < target ? Math.Min(value + delta, target) : Math.Max(value - delta, target);

        public static int FloorToInt(double value) => (int)Math.Floor(value);
    }
}
