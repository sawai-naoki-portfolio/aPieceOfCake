using System.Collections.Generic;
using System.Linq;

namespace PieceOfCake.Core
{
    /// <summary>Saves the stage at a checkpoint and restores it after a miss or a retry.</summary>
    internal sealed class CheckpointService
    {
        private readonly StageState state;
        private List<Piece> savedPieces;
        private List<Ant> savedAnts;
        private RunStats savedStats;
        private bool savedCarry;
        private GridPoint savedCell;

        public CheckpointService(StageState state)
        {
            this.state = state;
        }

        public void Capture()
        {
            savedPieces = state.Pieces.Select(p => p.CloneForSnapshot()).ToList();
            savedAnts = state.Ants.Select(a => a.Clone()).ToList();
            savedStats = state.Stats.Clone();
            savedCarry = state.Player.IsCarryingOrder;
            savedCell = state.RespawnCell;
        }

        /// <summary>
        /// Rebuilds the saved world. Living ants return to their spawn points; the miss counter and
        /// stage timer keep running.
        /// </summary>
        public void Restore()
        {
            state.ReplaceWorld(
                savedPieces.Select(p => p.CloneForSnapshot()).ToList(),
                savedAnts.Select(a => a.CloneForRespawn()).ToList());
            state.Stats = savedStats.Clone();
            state.Player = new Player(savedCell)
            {
                IsCarryingOrder = savedCarry,
                InvulnerableTimer = SimConstants.RespawnInvulnerability,
            };
            state.OrderLostTimer = -1;
        }
    }
}
