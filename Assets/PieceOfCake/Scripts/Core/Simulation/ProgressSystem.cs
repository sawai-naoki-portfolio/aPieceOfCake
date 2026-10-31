namespace PieceOfCake.Core
{
    /// <summary>Order pickup, checkpoints and delivery to the customer.</summary>
    internal sealed class ProgressSystem
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly CheckpointService checkpoints;
        private readonly GameplayOutcomes outcomes;

        public ProgressSystem(StageState state, CheckpointService checkpoints, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.checkpoints = checkpoints;
            this.outcomes = outcomes;
        }

        public void Update()
        {
            var p = state.Player;
            if (p.IsDead) return;
            PickUpOrder(p);
            TouchCheckpoints(p);
            TouchGoal(p);
        }

        private void PickUpOrder(Player p)
        {
            var reach = new Box(p.X - 4, p.Y - 4, SimConstants.PlayerWidth + 8, SimConstants.PlayerHeight + 8);
            foreach (var piece in state.Pieces)
            {
                if (!piece.IsOrder) continue;
                var slice = new Box(piece.PixelX, piece.PixelY, T, T);
                if (!reach.Overlaps(slice)) continue;
                state.RemovePiece(piece);
                p.IsCarryingOrder = true;
                outcomes.Say(PlayerMessage.OrderPickedUp);
                state.Events.Emit(GameEventType.OrderPickedUp, slice.CenterX, slice.CenterY);
                return;
            }
        }

        private void TouchCheckpoints(Player p)
        {
            foreach (var c in state.Checkpoints)
            {
                if (c.IsActive || !p.Bounds.Overlaps(c.TriggerArea)) continue;
                foreach (var other in state.Checkpoints) other.IsActive = false;
                c.IsActive = true;
                state.RespawnCell = c.Cell;
                checkpoints.Capture();
                outcomes.Say(PlayerMessage.CheckpointReached);
                state.Events.Emit(GameEventType.CheckpointReached, c.Cell.X * T + T / 2.0, c.Cell.Y * T);
            }
        }

        private void TouchGoal(Player p)
        {
            var g = state.Layout.Goal;
            var area = new Box(g.X * T - T * 0.7, g.Y * T - T, T * 2.4, 2 * T);
            if (!p.Bounds.Overlaps(area)) return;
            if (p.IsCarryingOrder)
            {
                Deliver(p);
            }
            else if (p.MessageCooldown <= 0)
            {
                outcomes.Say(PlayerMessage.CustomerWaiting);
                p.MessageCooldown = SimConstants.GoalHintCooldown;
            }
        }

        private void Deliver(Player p)
        {
            if (!state.IsPlaying) return;
            state.Phase = GamePhase.Delivering;
            state.DeliveryTimer = 0;
            p.IsCarryingOrder = false;
            p.VelocityX = 0;
            var g = state.Layout.Goal;
            state.Events.Emit(GameEventType.OrderDelivered, g.X * T + T / 2.0, g.Y * T);
        }
    }
}
