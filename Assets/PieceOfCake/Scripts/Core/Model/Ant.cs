namespace PieceOfCake.Core
{
    /// <summary>Patrolling enemy. Touching it from the side is a miss; landing on it squashes it.</summary>
    public sealed class Ant
    {
        internal Ant(GridPoint spawnCell)
        {
            SpawnX = spawnCell.X * SimConstants.CellSize + 5;
            SpawnY = (spawnCell.Y + 1) * SimConstants.CellSize - SimConstants.AntHeight;
            X = SpawnX;
            Y = SpawnY;
            VelocityX = -SimConstants.AntSpeed;
            IsAlive = true;
        }

        private Ant()
        {
        }

        public double X { get; internal set; }
        public double Y { get; internal set; }
        public double SpawnX { get; private set; }
        public double SpawnY { get; private set; }
        public double VelocityX { get; internal set; }
        public double VelocityY { get; internal set; }
        public bool IsAlive { get; internal set; }
        /// <summary>Seconds since the ant was squashed (for the squash animation).</summary>
        public double DeadTimer { get; internal set; }

        public double Width => SimConstants.AntWidth;
        public double Height => SimConstants.AntHeight;
        public Box Bounds => new Box(X, Y, Width, Height);

        internal void Squash()
        {
            IsAlive = false;
            DeadTimer = 0;
        }

        internal Ant Clone() => new Ant
        {
            X = X, Y = Y, SpawnX = SpawnX, SpawnY = SpawnY,
            VelocityX = VelocityX, VelocityY = VelocityY, IsAlive = IsAlive, DeadTimer = DeadTimer,
        };

        /// <summary>Copy for respawning: living ants return to their spawn point.</summary>
        internal Ant CloneForRespawn()
        {
            var copy = Clone();
            if (!copy.IsAlive) return copy;
            copy.X = SpawnX;
            copy.Y = SpawnY;
            copy.VelocityX = -SimConstants.AntSpeed;
            copy.VelocityY = 0;
            return copy;
        }
    }
}
