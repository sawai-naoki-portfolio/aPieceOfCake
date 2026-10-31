namespace PieceOfCake.Core
{
    /// <summary>Applies small outcomes shared by several systems: speech, misses and squashed ants.</summary>
    internal sealed class GameplayOutcomes
    {
        private readonly StageState state;

        public GameplayOutcomes(StageState state)
        {
            this.state = state;
        }

        public void Say(PlayerMessage message) => state.Player.Say(message);

        /// <summary>A refusal message that is rate-limited so holding a button does not spam it.</summary>
        public void Refuse(PlayerMessage message)
        {
            var p = state.Player;
            if (p.MessageCooldown > 0) return;
            p.Say(message);
            p.MessageCooldown = SimConstants.MessageCooldown;
            state.Events.Emit(GameEventType.Refused, p.CenterX, p.Y);
        }

        /// <summary>Starts the miss sequence. Ignored if already dead or no longer playing.</summary>
        public void KillPlayer(DeathCause cause)
        {
            var p = state.Player;
            if (p.IsDead || !state.IsPlaying) return;
            p.IsDead = true;
            p.DeathCause = cause;
            p.DeadTimer = 0;
            p.VelocityY = -SimConstants.DeathHopVelocity;
            p.VelocityX = 0;
            state.Misses++;
            state.Events.Emit(GameEventType.PlayerMissed, p.CenterX, p.Y + SimConstants.PlayerHeight / 2);
        }

        public void SquashAnt(Ant ant)
        {
            ant.Squash();
            state.Events.Emit(GameEventType.AntSquashed, ant.X + ant.Width / 2, ant.Y + ant.Height / 2);
        }
    }
}
