using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>A tutorial sign placed in a level.</summary>
    public sealed class SignInfo
    {
        public SignInfo(GridPoint cell, string text)
        {
            Cell = cell;
            Text = text;
        }

        public GridPoint Cell { get; }
        public string Text { get; }
    }

    /// <summary>Initial shape and position of one cake piece. Null entries are empty.</summary>
    public sealed class PieceBlueprint
    {
        public PieceBlueprint(int gridX, int gridY, CellType?[,] cells, bool isOrder)
        {
            GridX = gridX;
            GridY = gridY;
            Cells = cells;
            IsOrder = isOrder;
        }

        public int GridX { get; }
        public int GridY { get; }
        /// <summary>Indexed [row, column].</summary>
        public CellType?[,] Cells { get; }
        public bool IsOrder { get; }
    }

    /// <summary>Validated, immutable static data of a stage.</summary>
    public sealed class LevelLayout
    {
        private readonly TileType[] tiles;

        internal LevelLayout(LevelDefinition definition, int width, int height, TileType[] tiles,
            GridPoint start, GridPoint goal, IReadOnlyList<GridPoint> checkpoints, IReadOnlyList<GridPoint> antSpawns,
            IReadOnlyList<SignInfo> signs, IReadOnlyList<PieceBlueprint> pieces)
        {
            Definition = definition;
            Width = width;
            Height = height;
            this.tiles = tiles;
            Start = start;
            Goal = goal;
            Checkpoints = checkpoints;
            AntSpawns = antSpawns;
            Signs = signs;
            Pieces = pieces;
        }

        public LevelDefinition Definition { get; }
        public int Width { get; }
        public int Height { get; }
        public GridPoint Start { get; }
        public GridPoint Goal { get; }
        public IReadOnlyList<GridPoint> Checkpoints { get; }
        public IReadOnlyList<GridPoint> AntSpawns { get; }
        public IReadOnlyList<SignInfo> Signs { get; }
        public IReadOnlyList<PieceBlueprint> Pieces { get; }

        public double PixelWidth => Width * SimConstants.CellSize;
        public double PixelHeight => Height * SimConstants.CellSize;

        public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        /// <summary>Terrain at a cell; outside the map is <see cref="TileType.Empty"/>.</summary>
        public TileType TileAt(int x, int y) => Contains(x, y) ? tiles[y * Width + x] : TileType.Empty;
    }
}
