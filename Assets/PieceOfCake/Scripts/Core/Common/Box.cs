namespace PieceOfCake.Core
{
    /// <summary>
    /// Axis-aligned rectangle in simulation pixels (x grows right, y grows down).
    /// </summary>
    public readonly struct Box
    {
        public readonly double X;
        public readonly double Y;
        public readonly double W;
        public readonly double H;

        public Box(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public double Right => X + W;
        public double Bottom => Y + H;
        public double CenterX => X + W / 2;
        public double CenterY => Y + H / 2;

        /// <summary>Strict overlap test: touching edges do not overlap.</summary>
        public bool Overlaps(in Box other) =>
            X < other.X + other.W && X + W > other.X && Y < other.Y + other.H && Y + H > other.Y;

        public Box Shrink(double amount) => new Box(X + amount, Y + amount, W - 2 * amount, H - 2 * amount);
    }
}
