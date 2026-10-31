using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>
    /// Ants walk, turn at walls and ledges, get crushed by moving cake, and either squash under a
    /// falling player or cause a miss on contact.
    /// </summary>
    internal sealed class AntSystem
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly WorldQueries queries;
        private readonly GameplayOutcomes outcomes;
        private readonly List<CollisionBox> boxes = new List<CollisionBox>();

        public AntSystem(StageState state, WorldQueries queries, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.queries = queries;
            this.outcomes = outcomes;
        }

        public void Update(double dt)
        {
            var body = state.Player.Bounds;
            double mapWidth = state.Layout.PixelWidth;
            foreach (var a in state.Ants)
            {
                if (!a.IsAlive)
                {
                    a.DeadTimer += dt;
                    continue;
                }

                a.VelocityY = Math.Min(a.VelocityY + SimConstants.Gravity * dt, SimConstants.MaxFallSpeed);
                queries.CollectBoxesAround(new Box(a.X - T, a.Y - T, a.Width + 2 * T, a.Height + 2 * T), boxes);

                if (IsCrushedByMovingCake(a))
                {
                    outcomes.SquashAnt(a);
                    continue;
                }

                MoveHorizontally(a, dt, mapWidth);
                MoveVertically(a, dt);

                if (a.Y > state.Layout.PixelHeight + SimConstants.FallOutMargin)
                {
                    a.IsAlive = false;
                    a.DeadTimer = 9;
                }

                TouchPlayer(a, body);
            }
        }

        private bool IsCrushedByMovingCake(Ant a)
        {
            var core = new Box(a.X + 1, a.Y + 3, a.Width - 2, a.Height - 6);
            foreach (var b in boxes)
            {
                if (b.Piece != null && (b.Piece.Animation != null || b.Piece.State == PieceState.Falling) && core.Overlaps(b.Box))
                    return true;
            }

            return false;
        }

        private void MoveHorizontally(Ant a, double dt, double mapWidth)
        {
            a.X += a.VelocityX * dt;
            bool hit = false;
            if (a.X < 0)
            {
                a.X = 0;
                hit = true;
            }

            if (a.X + a.Width > mapWidth)
            {
                a.X = mapWidth - a.Width;
                hit = true;
            }

            foreach (var b in boxes)
            {
                if (!a.Bounds.Overlaps(b.Box)) continue;
                a.X = a.X + a.Width / 2 < b.Box.CenterX ? b.Box.X - a.Width : b.Box.Right;
                hit = true;
            }

            if (hit) a.VelocityX = -a.VelocityX;
        }

        private void MoveVertically(Ant a, double dt)
        {
            a.Y += a.VelocityY * dt;
            bool grounded = false;
            foreach (var b in boxes)
            {
                if (!a.Bounds.Overlaps(b.Box)) continue;
                if (a.VelocityY > 0)
                {
                    a.Y = b.Box.Y - a.Height;
                    grounded = true;
                }
                else
                {
                    a.Y = b.Box.Bottom;
                }

                a.VelocityY = 0;
            }

            if (!grounded) return;
            // Turn around at ledges.
            double probeX = a.VelocityX > 0 ? a.X + a.Width + 3 : a.X - 3;
            if (!queries.IsSolidPoint(probeX, a.Y + a.Height + 4)) a.VelocityX = -a.VelocityX;
        }

        private void TouchPlayer(Ant a, in Box body)
        {
            var p = state.Player;
            if (p.IsDead || !state.IsPlaying || !a.Bounds.Overlaps(body)) return;
            bool stomp = (p.PreviousVelocityY > 20 && p.PreviousBottom <= a.Y + 8)
                         || (p.VelocityY > 30 && p.Bottom - a.Y < 14);
            if (stomp)
            {
                outcomes.SquashAnt(a);
                p.VelocityY = -SimConstants.StompBounceVelocity;
            }
            else if (p.InvulnerableTimer <= 0)
            {
                outcomes.KillPlayer(DeathCause.Ant);
            }
        }
    }
}
