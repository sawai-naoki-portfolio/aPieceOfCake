using System.Collections.Generic;
using System.Linq;

namespace PieceOfCake.Core
{
    /// <summary>Rules for changing cake pieces: toppling, pushing and cutting.</summary>
    internal sealed class CakeOperations
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly WorldQueries queries;
        private readonly GameplayOutcomes outcomes;
        private readonly List<CollisionBox> scratch = new List<CollisionBox>();

        public CakeOperations(StageState state, WorldQueries queries, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.queries = queries;
            this.outcomes = outcomes;
        }

        /// <summary>
        /// Tips a resting one-cell-wide column (height ≥ 2) over in <paramref name="direction"/>, turning it
        /// into a horizontal bridge on its bottom row. Fails if the landing cells are blocked.
        /// </summary>
        public bool TryTopple(Piece p, int direction)
        {
            if (p.Width != 1 || p.Height < 2 || !p.IsIdle) return false;
            int baseY = p.GridY + p.Height - 1, x = p.GridX;
            var ignore = new HashSet<Piece> { p };
            for (int k = 1; k < p.Height; k++)
            {
                if (queries.IsBlocked(x + direction * k, baseY, ignore)) return false;
            }

            int height = p.Height;
            var column = new Cell[height];
            for (int r = 0; r < height; r++) column[r] = p.GetCell(0, r);

            var row = new Cell[1, height];
            for (int i = 0; i < height; i++)
            {
                // Falling right puts the bottom cell at the pivot and the top cell furthest right.
                row[0, i] = direction > 0 ? column[height - 1 - i] : column[i];
            }

            p.ReplaceCells(row);
            p.GridX = direction > 0 ? x : x - (height - 1);
            p.GridY = baseY;
            p.PixelX = p.GridX * T;
            p.OffsetY = 0;
            p.Animation = new ToppleAnimation(direction, column, x * T + T / 2.0, baseY * T + T / 2.0);
            state.Occupancy.Rebuild(state.Pieces);

            // Lift the player onto the bridge if it swung into them.
            var player = state.Player;
            var body = player.Bounds;
            scratch.Clear();
            WorldQueries.AddPieceBoxes(p, scratch);
            if (scratch.Any(b => body.Overlaps(b.Box)))
            {
                player.Y = baseY * T - SimConstants.PlayerHeight;
                player.VelocityY = 0;
            }

            state.Events.Emit(GameEventType.KnifeSwing, x * T + T / 2.0, baseY * T);
            return true;
        }

        /// <summary>
        /// Pushes a resting piece (plus anything stacked on it) one cell. Tall thin columns topple
        /// instead. The total weight must not exceed the player's push limit.
        /// </summary>
        public void TryPush(Piece q, int direction)
        {
            if (!q.IsIdle) return;
            if (!q.IsOrder && q.Width == 1 && q.Height >= 2)
            {
                TryTopple(q, direction);
                return;
            }

            var group = queries.CollectRiders(q);
            int weight = group.Sum(p => p.CellCount);
            int limit = state.Player.IsStrong ? SimConstants.StrongPushWeightLimit : SimConstants.PushWeightLimit;
            if (weight > limit)
            {
                outcomes.Refuse(PlayerMessage.TooHeavy);
                return;
            }

            var ignore = new HashSet<Piece>(group);
            foreach (var p in group)
            {
                for (int cy = 0; cy < p.Height; cy++)
                {
                    for (int cx = 0; cx < p.Width; cx++)
                    {
                        if (p.GetCell(cx, cy) != null && queries.IsBlocked(p.GridX + cx + direction, p.GridY + cy, ignore))
                            return;
                    }
                }
            }

            foreach (var p in group)
            {
                p.Animation = new SlideAnimation(p.PixelX);
                p.GridX += direction;
            }

            state.Occupancy.Rebuild(state.Pieces);
            state.Events.Emit(GameEventType.CakePushed, q.PixelX + (direction > 0 ? q.Width * T : 0), q.PixelY + q.Height * T);
        }

        /// <summary>
        /// Cuts <paramref name="q"/> along the vertical grid line <paramref name="line"/>. Returns false when
        /// the line does not separate any touching cells (nothing to cut). Returns true when the knife hit
        /// the cake, even if chocolate stopped it. With <paramref name="flop"/>, a resulting thin column on
        /// the far side topples over in <paramref name="direction"/>.
        /// </summary>
        public bool TrySplit(Piece q, int line, int direction, bool flop)
        {
            int lx = line - q.GridX;
            if (lx <= 0 || lx >= q.Width) return false;

            bool joined = false, chocolate = false;
            for (int cy = 0; cy < q.Height; cy++)
            {
                var a = q.GetCell(lx - 1, cy);
                var b = q.GetCell(lx, cy);
                if (a == null || b == null) continue;
                joined = true;
                if (a.Type == CellType.Chocolate || b.Type == CellType.Chocolate) chocolate = true;
            }

            if (!joined) return false;
            if (chocolate)
            {
                outcomes.Say(PlayerMessage.ChocolateTooHard);
                state.Events.Emit(GameEventType.ChocolateClink, line * T, (q.GridY + 0.5) * T);
                return true;
            }

            var left = PieceTopology.MaskColumns(q.Cells, c => c < lx);
            var right = PieceTopology.MaskColumns(q.Cells, c => c >= lx);
            var parts = PieceTopology.Components(state, q, left);
            parts.AddRange(PieceTopology.Components(state, q, right));
            state.ReplacePiece(q, parts);
            state.Events.Emit(GameEventType.CakeCut, line * T, q.GridY * T, 0, q.Height * T);

            if (flop)
            {
                foreach (var p in parts)
                {
                    bool beyondCut = direction > 0 ? p.GridX >= line : p.GridX + p.Width <= line;
                    if (beyondCut && p.Width == 1 && p.Height >= 2) TryTopple(p, direction);
                }
            }

            return true;
        }
    }
}
