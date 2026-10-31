namespace PieceOfCake.Core
{
    /// <summary>Speech the chef shows above their head. Texts live in the presentation layer.</summary>
    public enum PlayerMessage
    {
        None,
        CannotCutHere,
        TooHeavy,
        ChocolateTooHard,
        OrderIsNotFood,
        StrawberryPower,
        ChocolatePower,
        Yummy,
        OrderPickedUp,
        CheckpointReached,
        CustomerWaiting,
    }

    public enum DeathCause
    {
        Spike,
        Fall,
        Ant,
        Crushed,
    }

    /// <summary>The pastry chef controlled by the player. Position is the top-left of the body box.</summary>
    public sealed class Player
    {
        internal Player(GridPoint cell)
        {
            X = cell.X * SimConstants.CellSize + (SimConstants.CellSize - SimConstants.PlayerWidth) / 2;
            Y = (cell.Y + 1) * SimConstants.CellSize - SimConstants.PlayerHeight;
            Facing = 1;
        }

        public double X { get; internal set; }
        public double Y { get; internal set; }
        public double VelocityX { get; internal set; }
        public double VelocityY { get; internal set; }
        /// <summary>+1 facing right, -1 facing left.</summary>
        public int Facing { get; internal set; }
        public bool IsGrounded { get; internal set; }
        public bool IsCarryingOrder { get; internal set; }
        public bool IsDead { get; internal set; }
        public double DeadTimer { get; internal set; }
        public DeathCause DeathCause { get; internal set; }

        /// <summary>Remaining seconds of the strawberry jump power-up.</summary>
        public double PowerTimer { get; internal set; }
        /// <summary>Remaining seconds of the chocolate strength power-up.</summary>
        public double StrengthTimer { get; internal set; }
        public bool HasPower => PowerTimer > 0;
        public bool IsStrong => StrengthTimer > 0;
        public double InvulnerableTimer { get; internal set; }

        // Animation timers (read by views)
        public double CutAnimationTimer { get; internal set; }
        public double EatAnimationTimer { get; internal set; }
        /// <summary>Squash-and-stretch amount: positive squashes, negative stretches.</summary>
        public double Squash { get; internal set; }
        public double WalkTimer { get; internal set; }

        public PlayerMessage Message { get; internal set; }
        public double MessageTimer { get; internal set; }
        public bool IsTalking => MessageTimer > 0 && Message != PlayerMessage.None;

        // Internal controller state
        internal double CoyoteTimer;
        internal double JumpBufferTimer;
        internal double PushTimer;
        internal double ActionCooldown;
        internal double MessageCooldown;
        internal double PreviousBottom;
        internal double PreviousVelocityY;
        internal CollisionBox? TouchLeft;
        internal CollisionBox? TouchRight;

        public Box Bounds => new Box(X, Y, SimConstants.PlayerWidth, SimConstants.PlayerHeight);
        public double CenterX => X + SimConstants.PlayerWidth / 2;
        public double Bottom => Y + SimConstants.PlayerHeight;

        internal void Say(PlayerMessage message)
        {
            Message = message;
            MessageTimer = SimConstants.MessageDuration;
        }
    }
}
