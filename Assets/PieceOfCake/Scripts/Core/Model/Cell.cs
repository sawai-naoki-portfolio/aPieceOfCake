namespace PieceOfCake.Core
{
    /// <summary>One cake cell. The same instance moves between pieces when a piece is cut or split.</summary>
    public sealed class Cell
    {
        public const double NotMelting = -1;

        internal Cell(int id, CellType type)
        {
            Id = id;
            Type = type;
        }

        /// <summary>Stable identity, useful for per-cell visual variation.</summary>
        public int Id { get; }
        public CellType Type { get; }

        /// <summary>Seconds since melting started, or <see cref="NotMelting"/>.</summary>
        public double MeltTimer { get; internal set; } = NotMelting;

        public bool IsMelting => MeltTimer >= 0;

        /// <summary>Melt progress in 0..1 (0 when not melting).</summary>
        public double MeltProgress => IsMelting ? MathUtil.Clamp(MeltTimer / SimConstants.MeltDuration, 0, 1) : 0;

        /// <summary>Copy used for checkpoints: melting is reset, identity is kept.</summary>
        internal Cell CloneFresh() => new Cell(Id, Type);
    }
}
