namespace PieceOfCake.Core
{
    /// <summary>Material of one cake cell. Each type has distinct gameplay rules.</summary>
    public enum CellType
    {
        /// <summary>Plain sponge layer.</summary>
        Sponge,
        /// <summary>Whipped cream layer.</summary>
        Cream,
        /// <summary>Strawberry: eating it grants the jump power-up.</summary>
        Strawberry,
        /// <summary>Chocolate: cannot be cut; eating it grants the strength power-up.</summary>
        Chocolate,
        /// <summary>Jelly: standing on it bounces the player high.</summary>
        Jelly,
        /// <summary>Ice cream: melts shortly after the player stands on it.</summary>
        Ice,
        /// <summary>The ordered slice that must be delivered. Cannot be eaten or cut.</summary>
        Order,
    }

    /// <summary>Static terrain of a map cell.</summary>
    public enum TileType : byte
    {
        Empty = 0,
        Solid = 1,
        /// <summary>Fork spikes: lethal, and they support cake but not the player.</summary>
        Spike = 2,
    }
}
