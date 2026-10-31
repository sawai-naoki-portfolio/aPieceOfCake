using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>Something noteworthy happened; the presentation turns these into sound, particles and shake.</summary>
    public enum GameEventType
    {
        Jump,
        JellyBounce,
        KnifeSwing,
        CakeCut,
        ChocolateClink,
        CakePushed,
        ToppleLanded,
        PieceLanded,
        CellEaten,
        PowerUp,
        Refused,
        AntSquashed,
        OrderPickedUp,
        CheckpointReached,
        PlayerMissed,
        CellMelted,
        OrderLost,
        OrderDelivered,
    }

    /// <summary>
    /// Event payload in simulation pixels. (X, Y) is the anchor; (SpanX, SpanY) is an optional area
    /// starting at the anchor over which effects may be spread.
    /// </summary>
    public readonly struct GameEvent
    {
        public readonly GameEventType Type;
        public readonly double X;
        public readonly double Y;
        public readonly double SpanX;
        public readonly double SpanY;
        public readonly CellType CellType;

        public GameEvent(GameEventType type, double x, double y, double spanX = 0, double spanY = 0,
            CellType cellType = CellType.Sponge)
        {
            Type = type;
            X = x;
            Y = y;
            SpanX = spanX;
            SpanY = spanY;
            CellType = cellType;
        }
    }

    /// <summary>Collects events between two drains. Bounded so an undrained buffer cannot grow forever.</summary>
    internal sealed class EventBuffer
    {
        private const int Capacity = 512;
        private readonly List<GameEvent> events = new List<GameEvent>(64);

        public void Emit(in GameEvent e)
        {
            if (events.Count < Capacity) events.Add(e);
        }

        public void Emit(GameEventType type, double x, double y, double spanX = 0, double spanY = 0,
            CellType cellType = CellType.Sponge) => Emit(new GameEvent(type, x, y, spanX, spanY, cellType));

        public void DrainTo(List<GameEvent> destination)
        {
            destination.AddRange(events);
            events.Clear();
        }
    }
}
