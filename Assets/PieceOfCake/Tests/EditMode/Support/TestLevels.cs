using System.Collections.Generic;
using System.IO;
using System.Linq;
using PieceOfCake.Core;
using PieceOfCake.Game.Levels;
using UnityEngine;

namespace PieceOfCake.Tests
{
    /// <summary>Builds levels for tests: the shipped stages, or small inline maps.</summary>
    internal static class TestLevels
    {
        public const string LevelFilePath = "PieceOfCake/Data/Levels/levels.json";

        private static IReadOnlyList<LevelDefinition> shipped;

        public static IReadOnlyList<LevelDefinition> Shipped =>
            shipped ??= LevelJson.Parse(File.ReadAllText(Path.Combine(Application.dataPath, LevelFilePath)));

        public static LevelLayout Stage(int index) => LevelParser.Parse(Shipped[index]);

        /// <summary>
        /// Inline map, rows top to bottom. Like real stages it needs a start '@', a goal 'G' and an order 'O';
        /// every sign 't' gets a placeholder text.
        /// </summary>
        public static LevelLayout Map(params string[] rows)
        {
            int signs = rows.Sum(r => r.Count(c => c == LevelSymbols.Sign));
            return LevelParser.Parse(new LevelDefinition("test", "test", "shortcake", 0, "bear",
                Enumerable.Repeat("sign", signs).ToArray(), rows));
        }

        public static StageSimulation Simulate(params string[] rows) => new StageSimulation(Map(rows));
    }
}
