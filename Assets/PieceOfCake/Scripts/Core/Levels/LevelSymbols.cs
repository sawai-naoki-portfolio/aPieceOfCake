namespace PieceOfCake.Core
{
    /// <summary>
    /// Characters used in level map strings. See Docs/05_LevelFormat.md for the full table.
    /// Upper-case cake letters that touch each other form one piece; lower-case letters form
    /// a separate piece even when they touch upper-case cells.
    /// </summary>
    public static class LevelSymbols
    {
        public const char Empty = '.';
        public const char Solid = '#';
        public const char Spike = '^';
        public const char Start = '@';
        public const char Checkpoint = 'F';
        public const char Goal = 'G';
        public const char Ant = 'a';
        public const char Sign = 't';

        public static bool TryGetCellType(char symbol, out CellType type, out bool isLoose)
        {
            isLoose = char.IsLower(symbol);
            switch (char.ToUpperInvariant(symbol))
            {
                case 'S': type = CellType.Sponge; return true;
                case 'C': type = CellType.Cream; return true;
                case 'B': type = CellType.Strawberry; return true;
                case 'K': type = CellType.Chocolate; return true;
                case 'J': type = CellType.Jelly; return true;
                case 'I': type = CellType.Ice; return true;
                case 'O':
                    type = CellType.Order;
                    // 'o' is not a valid symbol: order slices are always single-cell pieces.
                    return !isLoose;
                default: type = default; return false;
            }
        }
    }
}
