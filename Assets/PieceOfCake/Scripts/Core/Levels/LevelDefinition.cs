using System;
using System.Collections.Generic;

namespace PieceOfCake.Core
{
    /// <summary>
    /// Raw, unvalidated description of one stage as authored in the level data file.
    /// Turn it into a playable <see cref="LevelLayout"/> with <see cref="LevelParser"/>.
    /// </summary>
    public sealed class LevelDefinition
    {
        public LevelDefinition(string id, string name, string theme, int par, string customer,
            IReadOnlyList<string> signs, IReadOnlyList<string> map)
        {
            Id = id;
            Name = name;
            Theme = theme;
            Par = par;
            Customer = customer;
            Signs = signs ?? Array.Empty<string>();
            Map = map ?? Array.Empty<string>();
        }

        public string Id { get; }
        public string Name { get; }
        /// <summary>Visual theme key (resolved by the presentation layer).</summary>
        public string Theme { get; }
        /// <summary>Maximum wasted cells (eaten + lost) that still earns three stars.</summary>
        public int Par { get; }
        /// <summary>Customer key (resolved by the presentation layer).</summary>
        public string Customer { get; }
        /// <summary>Tutorial texts, assigned to sign symbols ordered by column then row.</summary>
        public IReadOnlyList<string> Signs { get; }
        /// <summary>Map rows from top to bottom. All rows must have the same length.</summary>
        public IReadOnlyList<string> Map { get; }
    }
}
