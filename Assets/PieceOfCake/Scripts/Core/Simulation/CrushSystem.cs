using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Moving cake squashes ants; falling cake landing on the player's head is a miss.</summary>
    internal sealed class CrushSystem
    {
        private const int T = SimConstants.CellSize;
        private readonly StageState state;
        private readonly GameplayOutcomes outcomes;
        private readonly List<CollisionBox> boxes = new List<CollisionBox>();

        public CrushSystem(StageState state, GameplayOutcomes outcomes)
        {
            this.state = state;
            this.outcomes = outcomes;
        }

        public void Update()
        {
            var body = state.Player.Bounds.Shrink(4);
            foreach (var p in state.Pieces)
            {
                if (p.State != PieceState.Falling && p.Animation == null) continue;
                boxes.Clear();
                WorldQueries.AddPieceBoxes(p, boxes);
                foreach (var b in boxes)
                {
                    foreach (var a in state.Ants)
                    {
                        if (a.IsAlive && a.Bounds.Shrink(3).Overlaps(b.Box)) outcomes.SquashAnt(a);
                    }

                    var player = state.Player;
                    bool fromAbove = b.Box.Y + T / 2.0 < player.Y + SimConstants.PlayerHeight / 2;
                    if (p.State == PieceState.Falling && !player.IsDead && body.Overlaps(b.Box) && fromAbove)
                        outcomes.KillPlayer(DeathCause.Crushed);
                }
            }
        }
    }
}
