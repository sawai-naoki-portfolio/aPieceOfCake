namespace PieceOfCake.Core
{
    /// <summary>
    /// Gameplay tuning. Units are simulation pixels and seconds; one map cell is <see cref="CellSize"/> pixels.
    /// Values are identical to the HTML prototype so that recorded solutions replay frame-exactly.
    /// </summary>
    public static class SimConstants
    {
        public const int CellSize = 40;
        public const double FixedDeltaTime = 1.0 / 60.0;

        // Player body
        public const double PlayerWidth = 26;
        public const double PlayerHeight = 34;

        // Movement
        public const double Gravity = 2000;
        public const double MaxFallSpeed = 900;
        public const double RunSpeed = 180;
        public const double PoweredRunSpeed = 220;
        public const double GroundAcceleration = 2400;
        public const double AirAcceleration = 1600;
        public const double GroundFriction = 2800;
        public const double AirFrictionFactor = 0.7;
        public const double JumpVelocity = 600;
        public const double PoweredJumpVelocity = 760;
        public const double JellyBounceVelocity = 880;
        public const double StompBounceVelocity = 430;
        public const double DeathHopVelocity = 520;
        public const double CoyoteTime = 0.07;
        public const double JumpBufferTime = 0.12;

        // Cake handling
        public const double PushDelay = 0.12;
        public const double SlideDuration = 0.14;
        public const double ToppleDuration = 0.3;
        public const int PushWeightLimit = 4;
        public const int StrongPushWeightLimit = 12;
        public const double MeltDuration = 0.6;
        public const double PowerUpDuration = 12;
        public const double ActionReach = 14;
        public const double CutCooldown = 0.28;
        public const double EatCooldown = 0.3;
        public const double CutAnimationDuration = 0.22;
        public const double EatAnimationDuration = 0.35;
        public const double WobbleDecayRate = 2.2;

        // Ants
        public const double AntWidth = 30;
        public const double AntHeight = 16;
        public const double AntSpeed = 42;

        // Flow
        public const double RespawnDelayAfterMiss = 1.2;
        public const double RespawnDelayAfterOrderLost = 1.6;
        public const double DeliveryCelebrationTime = 1.7;
        public const double RespawnInvulnerability = 1.5;
        public const double FallOutMargin = 60;

        // Speech
        public const double MessageDuration = 1.8;
        public const double MessageCooldown = 1.6;
        public const double GoalHintCooldown = 2.5;
    }
}
