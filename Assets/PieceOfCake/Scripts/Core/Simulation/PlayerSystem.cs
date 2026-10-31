using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Player movement, collision, special floors (jelly, ice), pushing, actions and hazards.</summary>
    internal sealed class PlayerSystem
    {
        private const int T = SimConstants.CellSize;
        private const double PW = SimConstants.PlayerWidth;
        private const double PH = SimConstants.PlayerHeight;

        private readonly StageState state;
        private readonly WorldQueries queries;
        private readonly CakeOperations operations;
        private readonly PlayerActions actions;
        private readonly GameplayOutcomes outcomes;
        private readonly CheckpointService checkpoints;
        private readonly List<CollisionBox> boxes = new List<CollisionBox>();
        private readonly List<CollisionBox> underfoot = new List<CollisionBox>();

        public PlayerSystem(StageState state, WorldQueries queries, CakeOperations operations, PlayerActions actions,
            GameplayOutcomes outcomes, CheckpointService checkpoints)
        {
            this.state = state;
            this.queries = queries;
            this.operations = operations;
            this.actions = actions;
            this.outcomes = outcomes;
            this.checkpoints = checkpoints;
        }

        public void Update(double dt, in InputFrame input)
        {
            var p = state.Player;
            TickTimers(p, dt);
            if (p.IsDead)
            {
                UpdateDead(p, dt);
                return;
            }

            bool control = state.ControlEnabled;
            int dir = control ? input.Direction : 0;
            ApplyRunAndJump(p, dt, dir, control && input.JumpPressed);

            queries.CollectBoxesAround(new Box(p.X - T * 2, p.Y - T * 2, PW + T * 4, PH + T * 4), boxes);
            MoveHorizontally(p, dt);
            bool wasGrounded = p.IsGrounded;
            MoveVertically(p, dt);
            if (p.IsGrounded && !wasGrounded) p.Squash = 0.7;
            ApplySpecialFloors(p);
            UpdatePushing(p, dt, dir, control);

            if (control && p.ActionCooldown <= 0)
            {
                if (input.CutPressed) actions.Cut();
                else if (input.EatPressed) actions.Eat();
            }

            if (p.IsGrounded && Math.Abs(p.VelocityX) > 10) p.WalkTimer += dt;
            CheckHazards(p);
        }

        private static void TickTimers(Player p, double dt)
        {
            p.MessageTimer -= dt;
            p.MessageCooldown -= dt;
            p.CutAnimationTimer = Math.Max(0, p.CutAnimationTimer - dt);
            p.EatAnimationTimer = Math.Max(0, p.EatAnimationTimer - dt);
            p.Squash = MathUtil.Approach(p.Squash, 0, dt * 4);
            p.ActionCooldown -= dt;
            p.InvulnerableTimer = Math.Max(0, p.InvulnerableTimer - dt);
            if (p.PowerTimer > 0) p.PowerTimer -= dt;
            if (p.StrengthTimer > 0) p.StrengthTimer -= dt;
            p.PreviousBottom = p.Y + PH;
            p.PreviousVelocityY = p.VelocityY;
        }

        private void UpdateDead(Player p, double dt)
        {
            p.DeadTimer += dt;
            p.VelocityY = Math.Min(p.VelocityY + SimConstants.Gravity * dt, SimConstants.MaxFallSpeed);
            p.Y += p.VelocityY * dt;
            if (p.DeadTimer > SimConstants.RespawnDelayAfterMiss) checkpoints.Restore();
        }

        private void ApplyRunAndJump(Player p, double dt, int dir, bool jumpPressed)
        {
            if (dir != 0) p.Facing = dir;
            double speed = p.HasPower ? SimConstants.PoweredRunSpeed : SimConstants.RunSpeed;
            if (dir != 0)
            {
                double accel = p.IsGrounded ? SimConstants.GroundAcceleration : SimConstants.AirAcceleration;
                p.VelocityX = MathUtil.Approach(p.VelocityX, dir * speed, accel * dt);
            }
            else
            {
                double friction = p.IsGrounded
                    ? SimConstants.GroundFriction
                    : SimConstants.AirAcceleration * SimConstants.AirFrictionFactor;
                p.VelocityX = MathUtil.Approach(p.VelocityX, 0, friction * dt);
            }

            p.CoyoteTimer = p.IsGrounded ? SimConstants.CoyoteTime : p.CoyoteTimer - dt;
            if (jumpPressed) p.JumpBufferTimer = SimConstants.JumpBufferTime;
            else p.JumpBufferTimer -= dt;

            if (p.JumpBufferTimer > 0 && p.CoyoteTimer > 0)
            {
                p.VelocityY = -(p.HasPower ? SimConstants.PoweredJumpVelocity : SimConstants.JumpVelocity);
                p.JumpBufferTimer = 0;
                p.CoyoteTimer = 0;
                p.IsGrounded = false;
                p.Squash = -0.6;
                state.Events.Emit(GameEventType.Jump, p.CenterX, p.Bottom);
            }

            p.VelocityY = Math.Min(p.VelocityY + SimConstants.Gravity * dt, SimConstants.MaxFallSpeed);
        }

        private void MoveHorizontally(Player p, double dt)
        {
            double mapWidth = state.Layout.PixelWidth;
            p.X += p.VelocityX * dt;
            p.TouchLeft = null;
            p.TouchRight = null;
            if (p.X < 0)
            {
                p.X = 0;
                p.VelocityX = 0;
            }

            if (p.X + PW > mapWidth)
            {
                p.X = mapWidth - PW;
                p.VelocityX = 0;
            }

            foreach (var b in boxes)
            {
                if (!p.Bounds.Overlaps(b.Box)) continue;
                if (p.VelocityX > 0 || (p.VelocityX == 0 && p.X + PW / 2 < b.Box.CenterX))
                {
                    p.X = b.Box.X - PW;
                    p.TouchRight = b;
                }
                else
                {
                    p.X = b.Box.Right;
                    p.TouchLeft = b;
                }

                p.VelocityX = 0;
            }

            // Also count boxes we are flush against (for pushing while standing still against cake).
            if (p.TouchRight == null) p.TouchRight = FirstOverlap(new Box(p.X + 1, p.Y + 2, PW, PH - 4));
            if (p.TouchLeft == null) p.TouchLeft = FirstOverlap(new Box(p.X - 1, p.Y + 2, PW, PH - 4));
        }

        private CollisionBox? FirstOverlap(in Box probe)
        {
            foreach (var b in boxes)
            {
                if (probe.Overlaps(b.Box)) return b;
            }

            return null;
        }

        private void MoveVertically(Player p, double dt)
        {
            p.Y += p.VelocityY * dt;
            p.IsGrounded = false;
            var body = p.Bounds;
            double minTop = double.PositiveInfinity, maxBottom = double.NegativeInfinity;
            foreach (var b in boxes)
            {
                if (!body.Overlaps(b.Box)) continue;
                if (p.VelocityY >= 0) minTop = Math.Min(minTop, b.Box.Y);
                else maxBottom = Math.Max(maxBottom, b.Box.Bottom);
            }

            if (minTop < double.PositiveInfinity)
            {
                p.Y = minTop - PH;
                p.VelocityY = 0;
                p.IsGrounded = true;
            }

            if (maxBottom > double.NegativeInfinity)
            {
                p.Y = maxBottom;
                p.VelocityY = 0;
            }

            underfoot.Clear();
            if (!p.IsGrounded) return;
            var feet = new Box(p.X + 1, p.Y + PH, PW - 2, 2);
            foreach (var b in boxes)
            {
                if (feet.Overlaps(b.Box)) underfoot.Add(b);
            }
        }

        /// <summary>Jelly bounces the player; ice starts melting when stood on.</summary>
        private void ApplySpecialFloors(Player p)
        {
            if (!p.IsGrounded) return;
            CollisionBox? jelly = null;
            foreach (var b in underfoot)
            {
                if (b.Cell == null) continue;
                if (b.Cell.Type == CellType.Jelly) jelly = b;
                if (b.Cell.Type == CellType.Ice && !b.Cell.IsMelting) b.Cell.MeltTimer = 0;
            }

            if (jelly == null) return;
            p.VelocityY = -SimConstants.JellyBounceVelocity;
            p.IsGrounded = false;
            p.CoyoteTimer = 0;
            jelly.Value.Piece.Wobble = 1;
            p.Squash = -0.8;
            state.Events.Emit(GameEventType.JellyBounce, p.CenterX, p.Bottom);
        }

        private void UpdatePushing(Player p, double dt, int dir, bool control)
        {
            var touch = dir > 0 ? p.TouchRight : dir < 0 ? p.TouchLeft : null;
            var piece = touch?.Piece;
            if (control && p.IsGrounded && piece != null && piece.IsIdle)
            {
                p.PushTimer += dt;
                if (p.PushTimer < SimConstants.PushDelay) return;
                p.PushTimer = 0;
                operations.TryPush(piece, dir);
            }
            else
            {
                p.PushTimer = 0;
            }
        }

        private void CheckHazards(Player p)
        {
            var body = p.Bounds;
            int x0 = MathUtil.FloorToInt(p.X / T), x1 = MathUtil.FloorToInt((p.X + PW) / T);
            int y0 = MathUtil.FloorToInt(p.Y / T), y1 = MathUtil.FloorToInt((p.Y + PH) / T);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (state.Layout.TileAt(x, y) != TileType.Spike) continue;
                    // Only the fork tines hurt, not the whole cell.
                    if (body.Overlaps(new Box(x * T + 5, y * T + 14, T - 10, T - 14))) outcomes.KillPlayer(DeathCause.Spike);
                }
            }

            if (p.Y > state.Layout.PixelHeight + SimConstants.FallOutMargin) outcomes.KillPlayer(DeathCause.Fall);
        }
    }
}
