using System;
using System.Linq;
using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEditor;
using UnityEngine;

namespace PieceOfCake.EditorTools
{
    /// <summary>
    /// Wires exported models and project materials into the <see cref="GameArt"/> catalogue.
    /// Safe to run repeatedly: it fixes import settings, then (re)assigns every reference.
    /// </summary>
    internal static class ArtCatalogBuilder
    {
        private static readonly (string key, string model)[] Customers =
        {
            ("bear", "Customer_Bear"), ("rabbit", "Customer_Rabbit"), ("cat", "Customer_Cat"),
            ("penguin", "Customer_Penguin"), ("princess", "Customer_Princess"),
        };

        [MenuItem(ProjectPaths.MenuRoot + "Rebuild Art Catalog", priority = 1)]
        public static void Rebuild()
        {
            MaterialLibrary.EnsureAll();
            ConfigureModelImporters();
            var art = LoadOrCreateCatalog();
            Fill(art);
            EditorUtility.SetDirty(art);
            AssetDatabase.SaveAssets();

            var missing = art.MissingReferences();
            if (missing.Length == 0) Debug.Log("[PieceOfCake] Art catalogue rebuilt.", art);
            else Debug.LogError("[PieceOfCake] Art catalogue is incomplete: " + string.Join(", ", missing), art);
        }

        /// <summary>Static props: no cameras, lights, animation or rigs; materials remapped by name.</summary>
        private static void ConfigureModelImporters()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ProjectPaths.Models }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.isReadable = false;
                importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.RecursiveUp);
                importer.SaveAndReimport();
            }
        }

        private static GameArt LoadOrCreateCatalog()
        {
            var art = AssetDatabase.LoadAssetAtPath<GameArt>(ProjectPaths.GameArt);
            if (art != null) return art;
            AssetFolders.Ensure(ProjectPaths.Config);
            art = ScriptableObject.CreateInstance<GameArt>();
            AssetDatabase.CreateAsset(art, ProjectPaths.GameArt);
            return art;
        }

        private static void Fill(GameArt art)
        {
            art.cellMesh = MeshOf("Cell");
            art.creamCapMesh = MeshOf("CreamCap");
            art.creamCapMaterial = MaterialLibrary.Load("Mat_Cream");
            art.strawberryMesh = MeshOf("Strawberry");
            art.strawberryMaterials = MaterialsOf("Strawberry");
            art.orderSliceMesh = MeshOf("OrderSlice");
            art.orderSliceMaterials = MaterialsOf("OrderSlice");
            art.cellMaterials = new[]
            {
                Look(CellType.Sponge, "Mat_Sponge"), Look(CellType.Cream, "Mat_Cream"),
                Look(CellType.Strawberry, "Mat_PinkCream"), Look(CellType.Chocolate, "Mat_Chocolate"),
                Look(CellType.Jelly, "Mat_Jelly"), Look(CellType.Ice, "Mat_Ice"),
            };

            art.chefPrefab = Model("Chef");
            art.antPrefab = Model("Ant");
            art.tablePrefab = Model("Table");
            art.candlePrefab = Model("Candle");
            art.forkPrefab = Model("Fork");
            art.signPrefab = Model("Sign");
            art.customers = Customers.Select(c => new GameArt.CustomerLook { key = c.key, prefab = Model(c.model) }).ToArray();

            art.terrainMaterial = MaterialLibrary.Load("Mat_Terrain");
            art.terrainTopMaterial = MaterialLibrary.Load("Mat_TerrainTop");
            art.sceneryMaterial = MaterialLibrary.Load("Mat_Scenery");
            art.skyMaterial = MaterialLibrary.Load("Mat_Sky");
            art.particleMaterial = MaterialLibrary.Load("Mat_Particle");
            if (art.themes == null || art.themes.Length == 0) art.themes = DefaultThemes();
        }

        private static GameArt.CellLook Look(CellType type, string material) =>
            new GameArt.CellLook { type = type, material = MaterialLibrary.Load(material) };

        private static GameObject Model(string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{ProjectPaths.Models}/{name}.fbx");

        private static Mesh MeshOf(string model) =>
            AssetDatabase.LoadAllAssetsAtPath($"{ProjectPaths.Models}/{model}.fbx").OfType<Mesh>().FirstOrDefault();

        private static Material[] MaterialsOf(string model)
        {
            var prefab = Model(model);
            var renderer = prefab != null ? prefab.GetComponentInChildren<MeshRenderer>() : null;
            return renderer != null ? renderer.sharedMaterials : Array.Empty<Material>();
        }

        /// <summary>Theme colours taken from the HTML prototype's palette.</summary>
        private static ThemeStyle[] DefaultThemes()
        {
            ThemeStyle Theme(string key, string skyTop, string skyBottom, string terrain, string top, string scenery,
                string sun = "#FFF4E6", float intensity = 1.3f, string ambient = "#CDBDC2", bool stars = false) => new ThemeStyle
            {
                key = key, skyTop = Hex(skyTop), skyBottom = Hex(skyBottom), terrain = Hex(terrain), terrainTop = Hex(top),
                scenery = Hex(scenery), sunColor = Hex(sun), sunIntensity = intensity, ambient = Hex(ambient), starrySky = stars,
            };

            return new[]
            {
                Theme("shortcake", "#FFC9D6", "#FFF4E4", "#C98B5A", "#F59AAA", "#F6C9D3"),
                Theme("tart", "#FFCDBF", "#FFF0D0", "#E0A45C", "#F7D08C", "#F3C2B5"),
                Theme("jellychoco", "#BDE8DC", "#FFF1DE", "#8B5634", "#4E2A17", "#BDE6D9"),
                Theme("icecave", "#17213A", "#33496F", "#4A5F8C", "#EAF7FF", "#DDEBFF", "#BFD4FF", 0.9f, "#56658A", true),
                Theme("wedding", "#FFD3E6", "#FFF8EE", "#E9C9D3", "#FFB3CB", "#F8D3E2", intensity: 1.15f),
            };
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
