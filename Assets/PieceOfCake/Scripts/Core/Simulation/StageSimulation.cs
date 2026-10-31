using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>
    /// Deterministic simulation of one stage. Advance it with <see cref="Step"/> at
    /// <see cref="SimConstants.FixedDeltaTime"/>; read state through the properties; collect
    /// side effects with <see cref="DrainEvents"/>. Has no dependency on Unity.
    /// </summary>
    public sealed class StageSimulation
    {
        private const double Dt = SimConstants.FixedDeltaTime;

        private readonly StageState state;
        private readonly CheckpointService checkpoints;
        private readonly PieceSystem pieces;
        private readonly PlayerSystem player;
        private readonly AntSystem ants;
        private readonly CrushSystem crush;
        private readonly ProgressSystem progress;

        public StageSimulation(LevelLayout layout)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            state = new StageState(layout);
            var queries = new WorldQueries(state);
            var outcomes = new GameplayOutcomes(state);
            var operations = new CakeOperations(state, queries, outcomes);
            var playerActions = new PlayerActions(state, operations, outcomes);
            checkpoints = new CheckpointService(state);
            pieces = new PieceSystem(state, queries, outcomes);
            player = new PlayerSystem(state, queries, operations, playerActions, outcomes, checkpoints);
            ants = new AntSystem(state, queries, outcomes);
            crush = new CrushSystem(state, outcomes);
            progress = new ProgressSystem(state, checkpoints, outcomes);
            checkpoints.Capture();
        }

        public LevelLayout Layout { get; }
        public IReadOnlyList<Piece> Pieces => state.Pieces;
        public IReadOnlyList<Ant> Ants => state.Ants;
        public IReadOnlyList<Checkpoint> Checkpoints => state.Checkpoints;
        public Player Player => state.Player;
        public RunStats Stats => state.Stats;
        public GamePhase Phase => state.Phase;
        public double StageTime => state.StageTime;
        public int Misses => state.Misses;
        public bool IsOrderLost => state.IsOrderLost;
        /// <summary>Available once <see cref="Phase"/> is <see cref="GamePhase.Completed"/>.</summary>
        public StageResult Result { get; private set; }

        /// <summary>Advances the stage by one fixed step. Does nothing once completed.</summary>
        public void Step(in InputFrame input)
        {
            if (state.Phase == GamePhase.Completed) return;
            if (state.IsPlaying) state.StageTime += Dt;

            pieces.Update(Dt);
            player.Update(Dt, input);
            ants.Update(Dt);
            crush.Update();
            if (state.IsPlaying) progress.Update();

            if (state.IsOrderLost)
            {
                state.OrderLostTimer += Dt;
                if (state.OrderLostTimer > SimConstants.RespawnDelayAfterOrderLost) checkpoints.Restore();
            }

            if (state.Phase == GamePhase.Delivering)
            {
                state.DeliveryTimer += Dt;
                if (state.DeliveryTimer > SimConstants.DeliveryCelebrationTime) Complete();
            }
        }

        /// <summary>Returns to the last checkpoint. Only allowed while playing and alive.</summary>
        public bool RetryFromCheckpoint()
        {
            if (!state.IsPlaying || state.Player.IsDead) return false;
            checkpoints.Restore();
            return true;
        }

        /// <summary>Moves events produced since the previous drain into <paramref name="destination"/>.</summary>
        public void DrainEvents(List<GameEvent> destination) => state.Events.DrainTo(destination);

        private void Complete()
        {
            state.Phase = GamePhase.Completed;
            Result = new StageResult(state.Stats.Loss, Layout.Definition.Par, state.StageTime, state.Misses);
        }
    }
}
