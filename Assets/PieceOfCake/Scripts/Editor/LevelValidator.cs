using System.IO;
using PieceOfCake.Core;
using PieceOfCake.Game.Levels;
using UnityEditor;
using UnityEngine;

namespace PieceOfCake.EditorTools
{
    /// <summary>Checks the level file without entering Play mode (for level designers).</summary>
    internal static class LevelValidator
    {
        [MenuItem(ProjectPaths.MenuRoot + "Validate Levels", priority = 20)]
        public static void Validate()
        {
            try
            {
                var definitions = LevelJson.Parse(File.ReadAllText(ProjectPaths.Levels));
                foreach (var definition in definitions)
                {
                    var layout = LevelParser.Parse(definition);
                    Debug.Log($"[PieceOfCake] {definition.Id} \"{definition.Name}\": {layout.Width}x{layout.Height}, " +
                              $"{layout.Pieces.Count} pieces, {layout.AntSpawns.Count} ants, {layout.Checkpoints.Count} checkpoints, par {definition.Par}.");
                }

                Debug.Log($"[PieceOfCake] {definitions.Count} stages are valid.");
            }
            catch (LevelFormatException e)
            {
                Debug.LogError("[PieceOfCake] " + e.Message);
            }
        }
    }
}
