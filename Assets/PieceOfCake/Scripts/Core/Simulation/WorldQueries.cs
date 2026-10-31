using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Read-only spatial questions about terrain and pieces.</summary>
    internal sealed class WorldQueries
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;

        public WorldQueries(StageState state)
        {
            this.state = state;
        }

        private LevelLayout Layout => state.Layout;

        /// <summary>
        /// True if a cell cannot be entered by a moving piece. The map sides are walls, above and below
        /// the map is open, and spikes block.
        /// </summary>
        public bool IsBlocked(int x, int y, ICollection<Piece> ignore)
        {
            if (x < 0 || x >= Layout.Width) return true;
            if (y < 0 || y >= Layout.Height) return false;
            if (Layout.TileAt(x, y) != TileType.Empty) return true;
            var q = state.Occupancy.At(x, y);
            return q != null && (ignore == null || !ignore.Contains(q));
        }

        /// <summary>True if any cell of the piece rests on terrain (spikes included) or on a resting piece.</summary>
        public bool IsSupported(Piece p)
        {
            for (int cy = 0; cy < p.Height; cy++)
            {
                for (int cx = 0; cx < p.Width; cx++)
                {
                    if (p.GetCell(cx, cy) == null) continue;
                    int x = p.GridX + cx, y = p.GridY + cy + 1;
                    if (y >= Layout.Height || y < 0) continue;
                    if (Layout.TileAt(x, y) != TileType.Empty) return true;
                    var q = state.Occupancy.At(x, y);
                    if (q != null && q != p && q.State == PieceState.Resting) return true;
                }
            }

            return false;
        }

        /// <summary>Point test used by ants to look for ground ahead: terrain of any kind or any piece.</summary>
        public bool IsSolidPoint(double px, double py)
        {
            int x = MathUtil.FloorToInt(px / T), y = MathUtil.FloorToInt(py / T);
            if (x < 0 || x >= Layout.Width) return true;
            if (y < 0 || y >= Layout.Height) return false;
            return Layout.TileAt(x, y) != TileType.Empty || state.Occupancy.At(x, y) != null;
        }

        /// <summary>
        /// Solid boxes near an area: solid terrain cells (row-major) followed by every cell of each
        /// piece whose bounds overlap the area (in piece order). Spikes are not solid for bodies.
        /// </summary>
        public void CollectBoxesAround(in Box area, List<CollisionBox> into)
        {
            into.Clear();
            int x0 = MathUtil.FloorToInt(area.X / T), x1 = MathUtil.FloorToInt((area.X + area.W) / T);
            int y0 = MathUtil.FloorToInt(area.Y / T), y1 = MathUtil.FloorToInt((area.Y + area.H) / T);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (!Layout.Contains(x, y) || Layout.TileAt(x, y) != TileType.Solid) continue;
                    into.Add(new CollisionBox(new Box(x * T, y * T, T, T), null, null));
                }
            }

            foreach (var p in state.Pieces)
            {
                if (!p.Bounds.Overlaps(area)) continue;
                AddPieceBoxes(p, into);
            }
        }

        public static void AddPieceBoxes(Piece p, List<CollisionBox> into)
        {
            double bx = p.PixelX, by = p.PixelY;
            for (int cy = 0; cy < p.Height; cy++)
            {
                for (int cx = 0; cx < p.Width; cx++)
                {
                    var c = p.GetCell(cx, cy);
                    if (c != null) into.Add(new CollisionBox(new Box(bx + cx * T, by + cy * T, T, T), p, c));
                }
            }
        }

        /// <summary>The piece plus every resting piece stacked on top of it (transitively).</summary>
        public List<Piece> CollectRiders(Piece root)
        {
            var found = new List<Piece> { root };
            var set = new HashSet<Piece> { root };
            var queue = new Queue<Piece>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                for (int cy = 0; cy < p.Height; cy++)
                {
                    for (int cx = 0; cx < p.Width; cx++)
                    {
                        if (p.GetCell(cx, cy) == null) continue;
                        int x = p.GridX + cx, y = p.GridY + cy - 1;
                        if (y < 0) continue;
                        var above = state.Occupancy.At(x, y);
                        if (above == null || set.Contains(above) || above.State != PieceState.Resting) continue;
                        set.Add(above);
                        found.Add(above);
                        queue.Enqueue(above);
                    }
                }
            }

            return found;
        }
    }
}
