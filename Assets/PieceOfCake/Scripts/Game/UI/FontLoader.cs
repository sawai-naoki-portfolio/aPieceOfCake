using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>
    /// Uses an installed Japanese system font so the build ships no font files. The first family
    /// found becomes the main font; the others are glyph fallbacks.
    /// </summary>
    public static class FontLoader
    {
        private static readonly string[] Families =
        {
            "BIZ UDPGothic", "Yu Gothic UI", "Meiryo UI", "Meiryo",
            "Hiragino Maru Gothic ProN", "Hiragino Sans",
            "Noto Sans CJK JP", "Noto Sans JP", "Source Han Sans JP", "MS PGothic",
        };

        private static readonly string[] Styles = { "Bold", "Regular" };

        private static FontAsset loaded;

        /// <summary>Applies the font to <paramref name="root"/> and its children. Returns false if none was found.</summary>
        public static bool Apply(VisualElement root)
        {
            var font = Load();
            if (font == null)
            {
                Debug.LogWarning("[PieceOfCake] No Japanese system font found; text may not render. See Docs/07_Operations.md.");
                return false;
            }

            root.style.unityFontDefinition = FontDefinition.FromSDFFont(font);
            return true;
        }

        private static FontAsset Load()
        {
            if (loaded != null) return loaded;
            var found = new List<FontAsset>();
            foreach (var family in Families)
            {
                var asset = TryCreate(family);
                if (asset != null) found.Add(asset);
                if (found.Count >= 3) break;
            }

            if (found.Count == 0) return null;
            loaded = found[0];
            loaded.fallbackFontAssetTable = found.GetRange(1, found.Count - 1);
            return loaded;
        }

        private static FontAsset TryCreate(string family)
        {
            foreach (var style in Styles)
            {
                var asset = FontAsset.CreateFontAsset(family, style);
                if (asset != null) return asset;
            }

            return null;
        }
    }
}
