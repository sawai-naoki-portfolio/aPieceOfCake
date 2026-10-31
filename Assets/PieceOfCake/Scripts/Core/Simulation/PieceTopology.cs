using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Splits a cell grid into connected pieces (4-neighbourhood).</summary>
    internal static class PieceTopology
    {
        private static readonly (int dx, int dy)[] Neighbours = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        /// <summary>
        /// Returns one new piece per connected group of <paramref name="cells"/>, in row-major order of
        /// each group's first cell. New pieces inherit position, motion and state from
        /// <paramref name="template"/>. An empty grid yields an empty list.
        /// </summary>
        public static List<Piece> Components(StageState state, Piece template, Cell[,] cells)
        {
            const int T = SimConstants.CellSize;
            int h = cells.GetLength(0), w = cells.GetLength(1);
            var result = new List<Piece>();
            var seen = new bool[h, w];
            var stack = new Stack<(int x, int y)>();
            var members = new List<(int x, int y)>();

            for (int cy = 0; cy < h; cy++)
            {
                for (int cx = 0; cx < w; cx++)
                {
                    if (cells[cy, cx] == null || seen[cy, cx]) continue;
                    members.Clear();
                    seen[cy, cx] = true;
                    stack.Push((cx, cy));
                    while (stack.Count > 0)
                    {
                        var (x, y) = stack.Pop();
                        members.Add((x, y));
                        foreach (var (dx, dy) in Neighbours)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h || seen[ny, nx] || cells[ny, nx] == null) continue;
                            seen[ny, nx] = true;
                            stack.Push((nx, ny));
                        }
                    }

                    int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
                    foreach (var (x, y) in members)
                    {
                        minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                    }

                    var part = new Cell[maxY - minY + 1, maxX - minX + 1];
                    foreach (var (x, y) in members) part[y - minY, x - minX] = cells[y, x];

                    var piece = state.NewPiece(template.GridX + minX, template.GridY + minY, part, template.IsOrder);
                    piece.PixelX = template.PixelX + minX * T;
                    piece.OffsetY = template.OffsetY;
                    piece.VelocityY = template.VelocityY;
                    piece.State = template.State;
                    piece.Wobble = template.Wobble;
                    result.Add(piece);
                }
            }

            return result;
        }

        /// <summary>Copy of <paramref name="cells"/> keeping only columns where <paramref name="keep"/> is true.</summary>
        public static Cell[,] MaskColumns(Cell[,] cells, Func<int, bool> keep)
        {
            int h = cells.GetLength(0), w = cells.GetLength(1);
            var copy = new Cell[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (keep(x)) copy[y, x] = cells[y, x];
            return copy;
        }
    }
}
