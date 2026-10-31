using System;
using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PieceOfCake.Game.World
{
    /// <summary>Everything visible for one stage. Created when a stage starts, disposed when it ends.</summary>
    public sealed class StageView : IDisposable
    {
        private readonly GameObject root;
        private readonly Mesh terrainMesh;
        private readonly Material terrainMaterial;
        private readonly Material terrainTopMaterial;
        private readonly CakeRenderer cake;
        private readonly AntViews ants;
        private readonly BackgroundView background;

        public StageView(GameArt art, StageSimulation sim, Camera camera, Light sun)
        {
            var layout = sim.Layout;
            Map = new WorldMapper(layout);
            var theme = art.Theme(layout.Definition.Theme);
            root = new GameObject("Stage " + layout.Definition.Id);

            terrainMesh = TerrainBuilder.Build(layout);
            terrainMaterial = new Material(art.terrainMaterial) { color = theme.terrain };
            terrainTopMaterial = new Material(art.terrainTopMaterial) { color = theme.terrainTop };
            MeshObject.Create("Terrain", terrainMesh, new[] { terrainMaterial, terrainTopMaterial }, root.transform);

            cake = new CakeRenderer(art, Map);
            Chef = new ChefView(art, root.transform);
            ants = new AntViews(art, root.transform, sim.Ants.Count);
            Props = new PropViews(art, layout, Map, root.transform, sim.Checkpoints);
            background = new BackgroundView(art, theme, Map, root.transform, camera);
            ApplyLighting(theme, sun);
        }

        public WorldMapper Map { get; }
        public ChefView Chef { get; }
        public PropViews Props { get; }
        public Transform Root => root.transform;

        public void Render(StageSimulation sim, float time, float deltaTime)
        {
            cake.Render(sim.Pieces, time);
            Chef.Render(sim.Player, Map, time, deltaTime);
            ants.Render(sim.Ants, Map, time);
            Props.Render(sim.Phase != GamePhase.Playing, time);
        }

        private static void ApplyLighting(ThemeStyle theme, Light sun)
        {
            if (sun != null)
            {
                sun.color = theme.sunColor;
                sun.intensity = theme.sunIntensity;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = theme.ambient;
        }

        public void Dispose()
        {
            background.Dispose();
            Object.Destroy(root);
            Object.Destroy(terrainMesh);
            Object.Destroy(terrainMaterial);
            Object.Destroy(terrainTopMaterial);
        }
    }
}
