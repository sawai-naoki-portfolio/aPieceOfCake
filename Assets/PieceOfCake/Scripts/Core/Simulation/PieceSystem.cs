using System;
using System.Collections.Generic;
using System.Linq;

namespace PieceOfCake.Core
{
    /// <summary>Advances piece animations, melting ice, and gravity for unsupported pieces.</summary>
    internal sealed class PieceSystem
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly WorldQueries queries;
        private readonly GameplayOutcomes outcomes;
        private readonly List<CollisionBox> boxes = new List<CollisionBox>();

        public PieceSystem(StageState state, WorldQueries queries, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.queries = queries;
            this.outcomes = outcomes;
        }

        public void Update(double dt)
        {
            UpdateAnimations(dt);
            UpdateMelting(dt);
            StartFalling();
            UpdateFalling(dt);
            state.Occupancy.Rebuild(state.Pieces);
        }

        private void UpdateAnimations(double dt)
        {
            foreach (var p in state.Pieces)
            {
                var anim = p.Animation;
                if (anim != null)
                {
                    SquashAntsUnder(p);
                    anim.Elapsed += dt;
                    if (anim is SlideAnimation slide)
                        p.PixelX = MathUtil.Lerp(slide.FromX, p.GridX * T, anim.Progress);
                    if (anim.IsFinished)
                    {
                        if (anim is ToppleAnimation)
                            state.Events.Emit(GameEventType.ToppleLanded, p.PixelX, p.GridY * T + T, p.Width * T);
                        p.Animation = null;
                        p.PixelX = p.GridX * T;
                        SquashAntsUnder(p);
                    }
                }

                if (p.Wobble > 0) p.Wobble = Math.Max(0, p.Wobble - dt * SimConstants.WobbleDecayRate);
            }
        }

        private void UpdateMelting(double dt)
        {
            foreach (var p in state.Pieces.ToArray())
            {
                bool melted = false;
                for (int cy = 0; cy < p.Height; cy++)
                {
                    for (int cx = 0; cx < p.Width; cx++)
                    {
                        var c = p.GetCell(cx, cy);
                        if (c == null || !c.IsMelting) continue;
                        c.MeltTimer += dt;
                        if (c.MeltTimer < SimConstants.MeltDuration) continue;
                        p.SetCell(cx, cy, null);
                        melted = true;
                        state.Events.Emit(GameEventType.CellMelted, p.PixelX + cx * T + T / 2.0, p.PixelY + cy * T + T / 2.0);
                    }
                }

                if (melted) state.ReplacePiece(p, PieceTopology.Components(state, p, p.Cells));
            }
        }

        private void StartFalling()
        {
            foreach (var p in state.Pieces)
            {
                if (!p.IsIdle || queries.IsSupported(p)) continue;
                p.State = PieceState.Falling;
                p.VelocityY = 0;
                p.OffsetY = 0;
            }
        }

        private void UpdateFalling(double dt)
        {
            // Lowest pieces first so stacks fall together. OrderByDescending is a stable sort.
            var falling = state.Pieces.Where(p => p.State == PieceState.Falling)
                .OrderByDescending(p => p.GridY + p.Height).ToList();
            var layout = state.Layout;

            foreach (var p in falling)
            {
                p.VelocityY = Math.Min(p.VelocityY + SimConstants.Gravity * dt, SimConstants.MaxFallSpeed);
                p.OffsetY += p.VelocityY * dt;
                while (p.OffsetY >= T)
                {
                    bool landed = false;
                    double waitOffset = double.PositiveInfinity, waitVelocity = double.PositiveInfinity;
                    for (int cy = 0; cy < p.Height; cy++)
                    {
                        for (int cx = 0; cx < p.Width; cx++)
                        {
                            if (p.GetCell(cx, cy) == null) continue;
                            int x = p.GridX + cx, y = p.GridY + cy + 1;
                            if (y >= layout.Height || y < 0) continue;
                            if (layout.TileAt(x, y) != TileType.Empty)
                            {
                                landed = true;
                                continue;
                            }

                            var q = state.Occupancy.At(x, y);
                            if (q == null || q == p) continue;
                            if (q.State == PieceState.Resting)
                            {
                                landed = true;
                            }
                            else
                            {
                                waitOffset = Math.Min(waitOffset, q.OffsetY);
                                waitVelocity = Math.Min(waitVelocity, q.VelocityY);
                            }
                        }
                    }

                    if (landed)
                    {
                        p.OffsetY = 0;
                        p.VelocityY = 0;
                        p.State = PieceState.Resting;
                        state.Events.Emit(GameEventType.PieceLanded, p.PixelX, (p.GridY + p.Height) * T, p.Width * T);
                        break;
                    }

                    if (waitOffset < double.PositiveInfinity)
                    {
                        // The piece below is still falling: stay just above it.
                        p.OffsetY = Math.Min(Math.Min(p.OffsetY, waitOffset), T - 0.01);
                        p.VelocityY = Math.Min(p.VelocityY, waitVelocity);
                        break;
                    }

                    p.GridY++;
                    p.OffsetY -= T;
                    state.Occupancy.Rebuild(state.Pieces);
                }

                if (p.GridY > layout.Height + 1)
                {
                    state.RemovePiece(p);
                    if (p.IsOrder) LoseOrder(p);
                    else state.Stats.Lost += p.CellCount;
                }
            }
        }

        private void LoseOrder(Piece p)
        {
            if (state.IsOrderLost) return;
            state.OrderLostTimer = 0;
            state.Events.Emit(GameEventType.OrderLost, p.PixelX, p.PixelY);
        }

        private void SquashAntsUnder(Piece p)
        {
            boxes.Clear();
            WorldQueries.AddPieceBoxes(p, boxes);
            foreach (var b in boxes)
            {
                foreach (var a in state.Ants)
                {
                    if (a.IsAlive && new Box(a.X + 1, a.Y + 3, a.Width - 2, a.Height - 6).Overlaps(b.Box))
                        outcomes.SquashAnt(a);
                }
            }
        }
    }
}
