using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>All mutable state of a running stage, shared by the simulation systems.</summary>
    internal sealed class StageState
    {
        private int nextPieceId = 1;
        private int nextCellId = 1;

        public StageState(LevelLayout layout)
        {
            Layout = layout;
            Occupancy = new OccupancyGrid(layout.Width, layout.Height);
            foreach (var cell in layout.Checkpoints) Checkpoints.Add(new Checkpoint(cell));
            foreach (var spawn in layout.AntSpawns) Ants.Add(new Ant(spawn));
            foreach (var blueprint in layout.Pieces) Pieces.Add(CreatePiece(blueprint));
            Occupancy.Rebuild(Pieces);
            Player = new Player(layout.Start);
            RespawnCell = layout.Start;
        }

        public LevelLayout Layout { get; }
        public OccupancyGrid Occupancy { get; }
        public EventBuffer Events { get; } = new EventBuffer();
        public List<Piece> Pieces { get; private set; } = new List<Piece>();
        public List<Ant> Ants { get; private set; } = new List<Ant>();
        public List<Checkpoint> Checkpoints { get; } = new List<Checkpoint>();
        public Player Player { get; set; }
        public RunStats Stats { get; set; } = new RunStats();

        public GamePhase Phase { get; set; } = GamePhase.Playing;
        public double StageTime { get; set; }
        public int Misses { get; set; }
        /// <summary>Seconds since the order fell off the map, or -1.</summary>
        public double OrderLostTimer { get; set; } = -1;
        public double DeliveryTimer { get; set; }
        public GridPoint RespawnCell { get; set; }

        public bool IsOrderLost => OrderLostTimer >= 0;
        public bool IsPlaying => Phase == GamePhase.Playing;
        /// <summary>The player obeys input only while playing and the order is safe.</summary>
        public bool ControlEnabled => !IsOrderLost && IsPlaying;

        public void ReplaceWorld(List<Piece> pieces, List<Ant> ants)
        {
            Pieces = pieces;
            Ants = ants;
            Occupancy.Rebuild(Pieces);
        }

        public Piece NewPiece(int gridX, int gridY, Cell[,] cells, bool isOrder) =>
            new Piece(nextPieceId++, gridX, gridY, cells, isOrder);

        /// <summary>Swaps one piece for zero or more replacement pieces at the same list position.</summary>
        public void ReplacePiece(Piece piece, List<Piece> replacements)
        {
            int i = Pieces.IndexOf(piece);
            if (i < 0) return;
            Pieces.RemoveAt(i);
            Pieces.InsertRange(i, replacements);
            Occupancy.Rebuild(Pieces);
        }

        public void RemovePiece(Piece piece)
        {
            Pieces.Remove(piece);
            Occupancy.Rebuild(Pieces);
        }

        private Piece CreatePiece(PieceBlueprint blueprint)
        {
            int rows = blueprint.Cells.GetLength(0), cols = blueprint.Cells.GetLength(1);
            var cells = new Cell[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var type = blueprint.Cells[r, c];
                    if (type.HasValue) cells[r, c] = new Cell(nextCellId++, type.Value);
                }
            }

            return NewPiece(blueprint.GridX, blueprint.GridY, cells, blueprint.IsOrder);
        }
    }
}
