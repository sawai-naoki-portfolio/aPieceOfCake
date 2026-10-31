namespace PieceOfCake.Core
{
    public enum PieceState
    {
        Resting,
        Falling,
    }

    /// <summary>
    /// A rigid group of cells that moves as one. Cells are stored [row, column] relative to
    /// (<see cref="GridX"/>, <see cref="GridY"/>); empty slots are null.
    /// </summary>
    public sealed class Piece
    {
        private Cell[,] cells;

        internal Piece(int id, int gridX, int gridY, Cell[,] cells, bool isOrder)
        {
            Id = id;
            GridX = gridX;
            GridY = gridY;
            this.cells = cells;
            IsOrder = isOrder;
            PixelX = gridX * SimConstants.CellSize;
        }

        public int Id { get; }
        public bool IsOrder { get; }
        public int GridX { get; internal set; }
        public int GridY { get; internal set; }
        public int Width => cells.GetLength(1);
        public int Height => cells.GetLength(0);

        /// <summary>Left edge in pixels; differs from GridX * CellSize only while sliding.</summary>
        public double PixelX { get; internal set; }
        /// <summary>Downward offset within the current row while falling (0..CellSize).</summary>
        public double OffsetY { get; internal set; }
        public double VelocityY { get; internal set; }
        public PieceState State { get; internal set; }
        public PieceAnimation Animation { get; internal set; }
        /// <summary>Jelly squash amount after being bounced on (1 → 0).</summary>
        public double Wobble { get; internal set; }

        public double PixelY => GridY * SimConstants.CellSize + OffsetY;
        public Box Bounds => new Box(PixelX, PixelY, Width * SimConstants.CellSize, Height * SimConstants.CellSize);

        /// <summary>True when the piece is still and can be interacted with.</summary>
        public bool IsIdle => State == PieceState.Resting && Animation == null;

        public Cell GetCell(int column, int row) => cells[row, column];

        public int CellCount
        {
            get
            {
                int n = 0;
                foreach (var c in cells) if (c != null) n++;
                return n;
            }
        }

        internal Cell[,] Cells => cells;

        internal void SetCell(int column, int row, Cell cell) => cells[row, column] = cell;

        internal void ReplaceCells(Cell[,] newCells) => cells = newCells;

        /// <summary>Copy used for checkpoints: same id and state, fresh cells, no animation.</summary>
        internal Piece CloneForSnapshot()
        {
            var copy = new Cell[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    copy[r, c] = cells[r, c]?.CloneFresh();
            return new Piece(Id, GridX, GridY, copy, IsOrder)
            {
                OffsetY = OffsetY,
                VelocityY = VelocityY,
                State = State,
                Wobble = Wobble,
            };
        }
    }
}
