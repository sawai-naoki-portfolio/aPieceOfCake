namespace PieceOfCake.Core
{
    /// <summary>A solid one-cell box: either terrain (Piece == null) or a cake cell.</summary>
    internal readonly struct CollisionBox
    {
        public readonly Box Box;
        public readonly Piece Piece;
        public readonly Cell Cell;

        public CollisionBox(in Box box, Piece piece, Cell cell)
        {
            Box = box;
            Piece = piece;
            Cell = cell;
        }
    }
}
