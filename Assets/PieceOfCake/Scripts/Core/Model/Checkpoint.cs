namespace PieceOfCake.Core
{
    /// <summary>Candle checkpoint. Touching it saves the stage state for retries.</summary>
    public sealed class Checkpoint
    {
        internal Checkpoint(GridPoint cell)
        {
            Cell = cell;
        }

        public GridPoint Cell { get; }
        public bool IsActive { get; internal set; }

        /// <summary>Trigger area: the candle cell and the cell above it.</summary>
        internal Box TriggerArea => new Box(
            Cell.X * SimConstants.CellSize, Cell.Y * SimConstants.CellSize - SimConstants.CellSize,
            SimConstants.CellSize, 2 * SimConstants.CellSize);
    }
}
