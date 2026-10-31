using System;
using System.Collections.Generic;
using System.Linq;
using PieceOfCake.Core;
using UnityEngine;

namespace PieceOfCake.Game.Levels
{
    /// <summary>
    /// Reads the level data file (see Docs/05_LevelFormat.md). Structural problems raise
    /// <see cref="LevelFormatException"/>; map contents are validated later by <see cref="LevelParser"/>.
    /// </summary>
    public static class LevelJson
    {
        public const int SupportedVersion = 1;
        /// <summary>Refuse absurdly large files before parsing them.</summary>
        public const int MaxCharacters = 1_000_000;
        public const int MaxStages = 100;

        public static IReadOnlyList<LevelDefinition> Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new LevelFormatException("Level file is empty.");
            if (json.Length > MaxCharacters) throw new LevelFormatException("Level file is too large.");

            LevelFileDto file;
            try
            {
                file = JsonUtility.FromJson<LevelFileDto>(json);
            }
            catch (ArgumentException e)
            {
                throw new LevelFormatException($"Level file is not valid JSON: {e.Message}");
            }

            if (file == null) throw new LevelFormatException("Level file is not valid JSON.");
            if (file.version != SupportedVersion)
                throw new LevelFormatException($"Level file version {file.version} is not supported (expected {SupportedVersion}).");
            if (file.stages == null || file.stages.Length == 0) throw new LevelFormatException("Level file has no stages.");
            if (file.stages.Length > MaxStages) throw new LevelFormatException($"Level file has more than {MaxStages} stages.");

            var duplicate = file.stages.GroupBy(s => s?.id).FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null) throw new LevelFormatException($"Stage id '{duplicate.Key}' is used more than once.");

            return file.stages.Select(ToDefinition).ToList();
        }

        private static LevelDefinition ToDefinition(StageDto s, int index)
        {
            if (s == null) throw new LevelFormatException($"Stage #{index + 1} is empty.");
            return new LevelDefinition(s.id, s.name, s.theme, s.par, s.customer, s.signs, s.map);
        }

        // Field names match the JSON keys, as required by JsonUtility.
#pragma warning disable 0649
        [Serializable]
        private sealed class LevelFileDto
        {
            public int version;
            public StageDto[] stages;
        }

        [Serializable]
        private sealed class StageDto
        {
            public string id;
            public string name;
            public string theme;
            public int par;
            public string customer;
            public string[] signs;
            public string[] map;
        }
#pragma warning restore 0649
    }
}
