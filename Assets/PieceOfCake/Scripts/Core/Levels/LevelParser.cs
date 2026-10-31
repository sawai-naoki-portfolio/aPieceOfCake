using System;
using System.Collections.Generic;
using System.Linq;

namespace PieceOfCake.Core
{
    /// <summary>
    /// Validates a <see cref="LevelDefinition"/> and converts it into a <see cref="LevelLayout"/>.
    /// Any problem raises <see cref="LevelFormatException"/>; a layout is never partially valid.
    /// </summary>
    public static class LevelParser
    {
        /// <summary>Upper bound that keeps a corrupt or hostile data file from allocating huge grids.</summary>
        public const int MaxWidth = 512;
        public const int MaxHeight = 128;

        private enum Group : byte { None, Joined, Loose, Order }

        public static LevelLayout Parse(LevelDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            string label = string.IsNullOrWhiteSpace(definition.Id) ? "<no id>" : definition.Id;
            Validate(definition, label);

            var map = definition.Map;
            int height = map.Count, width = map[0].Length;
            var tiles = new TileType[width * height];
            var groups = new Group[width * height];
            var types = new CellType[width * height];
            var checkpoints = new List<GridPoint>();
            var ants = new List<GridPoint>();
            var signCells = new List<GridPoint>();
            GridPoint? start = null, goal = null;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char ch = map[y][x];
                    int i = y * width + x;
                    switch (ch)
                    {
                        case LevelSymbols.Empty: break;
                        case LevelSymbols.Solid: tiles[i] = TileType.Solid; break;
                        case LevelSymbols.Spike: tiles[i] = TileType.Spike; break;
                        case LevelSymbols.Start: start = Single(start, x, y, "start '@'", label); break;
                        case LevelSymbols.Goal: goal = Single(goal, x, y, "goal 'G'", label); break;
                        case LevelSymbols.Checkpoint: checkpoints.Add(new GridPoint(x, y)); break;
                        case LevelSymbols.Ant: ants.Add(new GridPoint(x, y)); break;
                        case LevelSymbols.Sign: signCells.Add(new GridPoint(x, y)); break;
                        default:
                            if (!LevelSymbols.TryGetCellType(ch, out var type, out bool loose))
                                throw Error(label, $"unknown symbol '{ch}' at ({x},{y})");
                            types[i] = type;
                            groups[i] = type == CellType.Order ? Group.Order : loose ? Group.Loose : Group.Joined;
                            break;
                    }
                }
            }

            if (start == null) throw Error(label, "missing start '@'");
            if (goal == null) throw Error(label, "missing goal 'G'");

            var pieces = BuildPieces(width, height, groups, types);
            if (!pieces.Any(p => p.IsOrder)) throw Error(label, "no order slice 'O'");

            var orderedSigns = signCells.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            if (orderedSigns.Count != definition.Signs.Count)
                throw Error(label, $"{orderedSigns.Count} sign symbols but {definition.Signs.Count} sign texts");
            var signs = orderedSigns.Select((p, k) => new SignInfo(p, definition.Signs[k] ?? string.Empty)).ToList();

            return new LevelLayout(definition, width, height, tiles, start.Value, goal.Value,
                checkpoints, ants, signs, pieces);
        }

        private static void Validate(LevelDefinition d, string label)
        {
            if (string.IsNullOrWhiteSpace(d.Id)) throw Error(label, "id is empty");
            if (string.IsNullOrWhiteSpace(d.Name)) throw Error(label, "name is empty");
            if (d.Par < 0) throw Error(label, "par must be zero or more");
            if (d.Map.Count == 0) throw Error(label, "map is empty");
            if (d.Map.Count > MaxHeight) throw Error(label, $"map is taller than {MaxHeight}");
            int width = d.Map[0]?.Length ?? 0;
            if (width == 0 || width > MaxWidth) throw Error(label, $"map width must be 1..{MaxWidth}");
            for (int y = 0; y < d.Map.Count; y++)
            {
                if (d.Map[y] == null || d.Map[y].Length != width)
                    throw Error(label, $"row {y} length differs from row 0 ({width})");
            }
        }

        private static GridPoint Single(GridPoint? existing, int x, int y, string what, string label)
        {
            if (existing != null) throw Error(label, $"more than one {what}");
            return new GridPoint(x, y);
        }

        /// <summary>Flood-fills touching cells of the same group into pieces, scanning row-major.</summary>
        private static List<PieceBlueprint> BuildPieces(int width, int height, Group[] groups, CellType[] types)
        {
            var result = new List<PieceBlueprint>();
            var seen = new bool[width * height];
            var stack = new Stack<int>();
            var members = new List<int>();
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] == Group.None || seen[i]) continue;
                var group = groups[i];
                members.Clear();
                seen[i] = true;
                stack.Push(i);
                while (stack.Count > 0)
                {
                    int j = stack.Pop();
                    members.Add(j);
                    if (group == Group.Order) continue;
                    int x = j % width, y = j / width;
                    TryVisit(x + 1, y);
                    TryVisit(x - 1, y);
                    TryVisit(x, y + 1);
                    TryVisit(x, y - 1);
                }

                int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
                foreach (int j in members)
                {
                    int x = j % width, y = j / width;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }

                var cells = new CellType?[maxY - minY + 1, maxX - minX + 1];
                foreach (int j in members) cells[j / width - minY, j % width - minX] = types[j];
                result.Add(new PieceBlueprint(minX, minY, cells, group == Group.Order));

                void TryVisit(int nx, int ny)
                {
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) return;
                    int k = ny * width + nx;
                    if (seen[k] || groups[k] != group) return;
                    seen[k] = true;
                    stack.Push(k);
                }
            }

            return result;
        }

        private static LevelFormatException Error(string label, string problem) =>
            new LevelFormatException($"Level '{label}': {problem}.");
    }
}
