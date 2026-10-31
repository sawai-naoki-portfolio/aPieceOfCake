using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Maps each map cell to the piece occupying it (by grid position, ignoring fall offset).</summary>
    internal sealed class OccupancyGrid
    {
        private readonly int width;
        private readonly int height;
        private readonly Piece[] slots;

        public OccupancyGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            slots = new Piece[width * height];
        }

        public void Rebuild(IReadOnlyList<Piece> pieces)
        {
            Array.Clear(slots, 0, slots.Length);
            foreach (var p in pieces)
            {
                for (int cy = 0; cy < p.Height; cy++)
                {
                    for (int cx = 0; cx < p.Width; cx++)
                    {
                        if (p.GetCell(cx, cy) == null) continue;
                        int x = p.GridX + cx, y = p.GridY + cy;
                        if (x < 0 || y < 0 || x >= width || y >= height) continue;
                        slots[y * width + x] = p;
                    }
                }
            }
        }

        public Piece At(int x, int y) =>
            x < 0 || y < 0 || x >= width || y >= height ? null : slots[y * width + x];

        /// <summary>The piece at a cell if it is idle (resting and not animating).</summary>
        public Piece IdleAt(int x, int y)
        {
            var q = At(x, y);
            return q != null && q.IsIdle ? q : null;
        }
    }
}
