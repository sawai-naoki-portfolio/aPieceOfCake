using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>The chef's two buttons: cut with the knife and eat.</summary>
    internal sealed class PlayerActions
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly CakeOperations operations;
        private readonly GameplayOutcomes outcomes;
        private readonly List<int> rows = new List<int>(2);
        private readonly int[] underColumns = new int[3];

        public PlayerActions(StageState state, CakeOperations operations, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.operations = operations;
            this.outcomes = outcomes;
        }

        private Player Player => state.Player;

        /// <summary>
        /// Cuts the cake in front of the body; otherwise, when standing on cake, cuts the cake underfoot
        /// along the leading edge of the feet, letting a thin column beyond the cut topple forward.
        /// </summary>
        public void Cut()
        {
            var p = Player;
            p.ActionCooldown = SimConstants.CutCooldown;
            p.CutAnimationTimer = SimConstants.CutAnimationDuration;
            state.Events.Emit(GameEventType.KnifeSwing, p.CenterX + p.Facing * 16, p.Y + 20);

            int d = p.Facing, front = FrontColumn();
            bool touched = false;
            foreach (int r in BodyRows())
            {
                var q = state.Occupancy.IdleAt(front, r);
                if (q == null || q.IsOrder) continue;
                touched = true;
                if (operations.TrySplit(q, d > 0 ? front + 1 : front, d, false)) return;
                break;
            }

            if (p.IsGrounded)
            {
                int below = FootRow() + 1;
                foreach (int c in UnderColumns())
                {
                    var u = state.Occupancy.IdleAt(c, below);
                    if (u == null || u.IsOrder) continue;
                    touched = true;
                    int line = d > 0
                        ? (int)Math.Ceiling((p.X + SimConstants.PlayerWidth - 2) / T)
                        : MathUtil.FloorToInt((p.X + 2) / T);
                    if (operations.TrySplit(u, line, d, true)) return;
                    break;
                }
            }

            if (touched) outcomes.Refuse(PlayerMessage.CannotCutHere);
        }

        /// <summary>Eats one cell in front of the body, or underfoot when nothing is in front.</summary>
        public void Eat()
        {
            var p = Player;
            p.ActionCooldown = SimConstants.EatCooldown;

            Piece target = null;
            int tx = 0, ty = 0;
            int front = FrontColumn();
            foreach (int r in BodyRows())
            {
                var q = state.Occupancy.IdleAt(front, r);
                if (q == null) continue;
                target = q; tx = front; ty = r;
                break;
            }

            if (target == null && p.IsGrounded)
            {
                int below = FootRow() + 1;
                foreach (int c in UnderColumns())
                {
                    var q = state.Occupancy.IdleAt(c, below);
                    if (q == null) continue;
                    target = q; tx = c; ty = below;
                    break;
                }
            }

            if (target == null) return;
            if (target.IsOrder)
            {
                outcomes.Say(PlayerMessage.OrderIsNotFood);
                state.Events.Emit(GameEventType.Refused, p.CenterX, p.Y);
                return;
            }

            var cell = target.GetCell(tx - target.GridX, ty - target.GridY);
            if (cell == null) return;

            p.EatAnimationTimer = SimConstants.EatAnimationDuration;
            target.SetCell(tx - target.GridX, ty - target.GridY, null);
            state.Stats.Eaten++;
            state.ReplacePiece(target, PieceTopology.Components(state, target, target.Cells));
            state.Events.Emit(GameEventType.CellEaten, tx * T + T / 2.0, ty * T + T / 2.0, cellType: cell.Type);

            switch (cell.Type)
            {
                case CellType.Strawberry:
                    p.PowerTimer = SimConstants.PowerUpDuration;
                    outcomes.Say(PlayerMessage.StrawberryPower);
                    state.Events.Emit(GameEventType.PowerUp, p.CenterX, p.Y, cellType: cell.Type);
                    break;
                case CellType.Chocolate:
                    p.StrengthTimer = SimConstants.PowerUpDuration;
                    outcomes.Say(PlayerMessage.ChocolatePower);
                    state.Events.Emit(GameEventType.PowerUp, p.CenterX, p.Y, cellType: cell.Type);
                    break;
                default:
                    outcomes.Say(PlayerMessage.Yummy);
                    break;
            }
        }

        /// <summary>Rows overlapped by the body (bottom row first when two).</summary>
        private List<int> BodyRows()
        {
            var p = Player;
            int r0 = MathUtil.FloorToInt((p.Y + 3) / T);
            int r1 = MathUtil.FloorToInt((p.Y + SimConstants.PlayerHeight - 3) / T);
            rows.Clear();
            if (r0 == r1)
            {
                rows.Add(r0);
            }
            else
            {
                rows.Add(r1);
                rows.Add(r0);
            }

            return rows;
        }

        private int FootRow() => MathUtil.FloorToInt((Player.Y + SimConstants.PlayerHeight - 1) / T);

        private int FrontColumn()
        {
            var p = Player;
            double x = p.Facing > 0 ? p.X + SimConstants.PlayerWidth + SimConstants.ActionReach : p.X - SimConstants.ActionReach;
            return MathUtil.FloorToInt(x / T);
        }

        /// <summary>Columns under the feet: centre, leading edge, trailing edge.</summary>
        private int[] UnderColumns()
        {
            var p = Player;
            double lead = p.Facing > 0 ? p.X + SimConstants.PlayerWidth - 2 : p.X + 2;
            double trail = p.Facing > 0 ? p.X + 2 : p.X + SimConstants.PlayerWidth - 2;
            underColumns[0] = MathUtil.FloorToInt((p.X + SimConstants.PlayerWidth / 2) / T);
            underColumns[1] = MathUtil.FloorToInt(lead / T);
            underColumns[2] = MathUtil.FloorToInt(trail / T);
            return underColumns;
        }
    }
}
