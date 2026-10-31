using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PieceOfCake.EditorTools
{
    /// <summary>
    /// Creates the project's URP materials. Existing materials are left untouched so that artists'
    /// tweaks survive a rebuild; delete a material to regenerate it from the defaults below.
    /// Model materials share names with the Blender materials (ArtSource/build_models.py) so that
    /// imported FBX files remap onto them.
    /// </summary>
    internal static class MaterialLibrary
    {
        private const string LitShader = "Universal Render Pipeline/Lit";
        private const string UnlitShader = "Universal Render Pipeline/Unlit";
        private const string ParticleShader = "Universal Render Pipeline/Particles/Unlit";

        private enum Kind { Opaque, Transparent, Emissive, Unlit, Particle }

        private readonly struct Spec
        {
            public readonly string Color;
            public readonly Kind Kind;
            public readonly float Smoothness;
            public readonly float Metallic;

            public Spec(string color, Kind kind = Kind.Opaque, float smoothness = 0.25f, float metallic = 0f)
            {
                Color = color;
                Kind = kind;
                Smoothness = smoothness;
                Metallic = metallic;
            }
        }

        private static readonly Dictionary<string, Spec> Specs = new Dictionary<string, Spec>
        {
            // Cake
            ["Mat_Sponge"] = new Spec("#F3C46E", smoothness: 0.15f),
            ["Mat_Cream"] = new Spec("#FFF8EE", smoothness: 0.35f),
            ["Mat_PinkCream"] = new Spec("#FFD7DF", smoothness: 0.35f),
            ["Mat_Chocolate"] = new Spec("#6E3D24", smoothness: 0.6f),
            ["Mat_Jelly"] = new Spec("#73D6BFD9", Kind.Transparent, 0.85f),
            ["Mat_Ice"] = new Spec("#D6F1FFEB", Kind.Transparent, 0.7f),
            ["Mat_Berry"] = new Spec("#E8364E", smoothness: 0.55f),
            ["Mat_Leaf"] = new Spec("#4CAF50"),
            ["Mat_Plate"] = new Spec("#F4F4F4", smoothness: 0.6f),
            ["Mat_Ribbon"] = new Spec("#F06D8C", smoothness: 0.4f),
            // Chef
            ["Mat_Coat"] = new Spec("#FFFFFF"),
            ["Mat_Apron"] = new Spec("#F7A1B5"),
            ["Mat_Skin"] = new Spec("#FFDDBF"),
            ["Mat_Shoe"] = new Spec("#6B4433"),
            ["Mat_Eye"] = new Spec("#3B2A22", smoothness: 0.7f),
            ["Mat_Blush"] = new Spec("#FF9AAA"),
            ["Mat_Steel"] = new Spec("#C4CDD6", smoothness: 0.8f, metallic: 0.6f),
            ["Mat_Wood"] = new Spec("#B07A4F"),
            // Creatures
            ["Mat_Ant"] = new Spec("#3A2418", smoothness: 0.5f),
            ["Mat_White"] = new Spec("#FFFFFF"),
            ["Mat_Fur_Bear"] = new Spec("#B9814F"),
            ["Mat_Fur_Rabbit"] = new Spec("#FBF4EE"),
            ["Mat_Fur_Cat"] = new Spec("#F4B268"),
            ["Mat_Fur_Penguin"] = new Spec("#3B4252"),
            ["Mat_InnerEar"] = new Spec("#F7B6C6"),
            ["Mat_Beak"] = new Spec("#F2A33A"),
            ["Mat_Dress"] = new Spec("#F7A1C4"),
            ["Mat_Hair"] = new Spec("#C98A4B"),
            ["Mat_Gold"] = new Spec("#FFD24A", smoothness: 0.7f, metallic: 0.5f),
            // Props
            ["Mat_Candle"] = new Spec("#FFFFFF"),
            ["Mat_CandleStripe"] = new Spec("#7CC6F0"),
            ["Mat_Flame"] = new Spec("#FFB13B", Kind.Emissive),
            ["Mat_SignBoard"] = new Spec("#5E7A4E"),
            // Stage (tinted per theme at runtime)
            ["Mat_Terrain"] = new Spec("#FFFFFF", smoothness: 0.1f),
            ["Mat_TerrainTop"] = new Spec("#FFFFFF", smoothness: 0.3f),
            ["Mat_Scenery"] = new Spec("#FFFFFF", smoothness: 0f),
            ["Mat_Sky"] = new Spec("#FFFFFF", Kind.Unlit),
            ["Mat_Particle"] = new Spec("#FFFFFF", Kind.Particle),
        };

        public static void EnsureAll()
        {
            AssetFolders.Ensure(ProjectPaths.Materials);
            foreach (var pair in Specs) Ensure(pair.Key, pair.Value);
            AssetDatabase.SaveAssets();
        }

        public static Material Load(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>($"{ProjectPaths.Materials}/{name}.mat");

        private static void Ensure(string name, Spec spec)
        {
            string path = $"{ProjectPaths.Materials}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            string shaderName = spec.Kind == Kind.Unlit ? UnlitShader : spec.Kind == Kind.Particle ? ParticleShader : LitShader;
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[PieceOfCake] Shader '{shaderName}' not found; is URP installed?");
                return;
            }

            ColorUtility.TryParseHtmlString(spec.Color, out var color);
            var material = new Material(shader) { name = name, color = color, enableInstancing = true };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", spec.Smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", spec.Metallic);
            if (spec.Kind == Kind.Transparent || spec.Kind == Kind.Particle) MakeTransparent(material);
            if (spec.Kind == Kind.Emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.6f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            AssetDatabase.CreateAsset(material, path);
        }

        private static void MakeTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_SrcBlendAlpha")) material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            if (material.HasProperty("_DstBlendAlpha")) material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    internal static class AssetFolders
    {
        /// <summary>Creates every missing folder along an "Assets/..." path.</summary>
        public static void Ensure(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
