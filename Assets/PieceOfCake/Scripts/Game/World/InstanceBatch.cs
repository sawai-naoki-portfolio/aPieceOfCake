using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PieceOfCake.Game.World
{
    /// <summary>Collects transforms for one mesh/sub-mesh/material and draws them with GPU instancing.</summary>
    public sealed class InstanceBatch
    {
        private const int MaxPerCall = 511;
        private readonly Mesh mesh;
        private readonly int subMesh;
        private readonly List<Matrix4x4> matrices = new List<Matrix4x4>(128);
        private RenderParams renderParams;

        public InstanceBatch(Mesh mesh, int subMesh, Material material, bool castShadows = true)
        {
            this.mesh = mesh;
            this.subMesh = subMesh;
            renderParams = new RenderParams(material)
            {
                shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                receiveShadows = true,
            };
        }

        public bool IsValid => mesh != null && renderParams.material != null && subMesh < mesh.subMeshCount;

        public void Clear() => matrices.Clear();

        public void Add(in Matrix4x4 matrix) => matrices.Add(matrix);

        public void Draw(Bounds worldBounds)
        {
            if (matrices.Count == 0 || !IsValid) return;
            renderParams.worldBounds = worldBounds;
            for (int start = 0; start < matrices.Count; start += MaxPerCall)
            {
                int count = Mathf.Min(MaxPerCall, matrices.Count - start);
                Graphics.RenderMeshInstanced(renderParams, mesh, subMesh, matrices, count, start);
            }
        }
    }
}
