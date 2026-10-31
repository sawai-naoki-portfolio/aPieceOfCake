namespace PieceOfCake.Core
{
    /// <summary>Waste counters for scoring.</summary>
    public sealed class RunStats
    {
        public int Eaten { get; internal set; }
        public int Lost { get; internal set; }
        /// <summary>Total wasted cells: eaten plus dropped off the map.</summary>
        public int Loss => Eaten + Lost;

        internal RunStats Clone() => new RunStats { Eaten = Eaten, Lost = Lost };
    }

    /// <summary>Outcome of a completed stage.</summary>
    public sealed class StageResult
    {
        public StageResult(int loss, int par, double time, int misses)
        {
            Loss = loss;
            Par = par;
            Time = time;
            Misses = misses;
            Stars = Scoring.StarsFor(loss, par);
        }

        public int Loss { get; }
        public int Par { get; }
        public double Time { get; }
        public int Misses { get; }
        public int Stars { get; }
    }

    public static class Scoring
    {
        public const int MaxStars = 3;
        /// <summary>Extra waste beyond par that still earns two stars.</summary>
        public const int TwoStarMargin = 3;

        public static int StarsFor(int loss, int par) =>
            loss <= par ? 3 : loss <= par + TwoStarMargin ? 2 : 1;
    }
}
