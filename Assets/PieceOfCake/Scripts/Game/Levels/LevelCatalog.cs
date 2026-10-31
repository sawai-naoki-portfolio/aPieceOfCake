using System.Collections.Generic;
using System.Linq;
using PieceOfCake.Core;
using UnityEngine;

namespace PieceOfCake.Game.Levels
{
    /// <summary>All stages of the game, validated once at startup.</summary>
    public sealed class LevelCatalog
    {
        private LevelCatalog(IReadOnlyList<LevelLayout> stages)
        {
            Stages = stages;
        }

        public IReadOnlyList<LevelLayout> Stages { get; }
        public int Count => Stages.Count;

        /// <summary>Parses and validates every stage. Throws <see cref="LevelFormatException"/> on bad data.</summary>
        public static LevelCatalog Load(TextAsset levelFile)
        {
            if (levelFile == null) throw new LevelFormatException("No level file is assigned.");
            var definitions = LevelJson.Parse(levelFile.text);
            return new LevelCatalog(definitions.Select(LevelParser.Parse).ToList());
        }

        public bool IsLast(int index) => index == Stages.Count - 1;
    }
}
