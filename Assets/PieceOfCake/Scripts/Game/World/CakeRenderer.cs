using System.Collections.Generic;
using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Draws every cake cell with GPU instancing each frame, straight from simulation state.
    /// There are no per-cell GameObjects, so cutting, eating and melting need no view bookkeeping.
    /// </summary>
    public sealed class CakeRenderer
    {
        private const float StrawberryScale = 0.55f;
        private const float MeltSquash = 0.45f;

        private readonly WorldMapper map;
        private readonly Dictionary<CellType, InstanceBatch> cellBatches = new Dictionary<CellType, InstanceBatch>();
        private readonly InstanceBatch capBatch;
        private readonly List<InstanceBatch> strawberryBatches = new List<InstanceBatch>();
        private readonly List<InstanceBatch> sliceBatches = new List<InstanceBatch>();
        private readonly List<InstanceBatch> all = new List<InstanceBatch>();

        public CakeRenderer(GameArt art, WorldMapper map)
        {
            this.map = map;
            foreach (CellType type in System.Enum.GetValues(typeof(CellType)))
            {
                if (type == CellType.Order) continue;
                bool opaque = type != CellType.Jelly && type != CellType.Ice;
                cellBatches[type] = Track(new InstanceBatch(art.cellMesh, 0, art.CellMaterial(type), opaque));
            }

            capBatch = Track(new InstanceBatch(art.creamCapMesh, 0, art.creamCapMaterial));
            AddSubMeshBatches(art.strawberryMesh, art.strawberryMaterials, strawberryBatches);
            AddSubMeshBatches(art.orderSliceMesh, art.orderSliceMaterials, sliceBatches);
        }

        public void Render(IReadOnlyList<Piece> pieces, float time)
        {
            foreach (var batch in all) batch.Clear();
            foreach (var piece in pieces)
            {
                if (piece.Animation is ToppleAnimation topple) AddToppling(topple);
                else AddResting(piece, time);
            }

            var bounds = map.MapBounds;
            foreach (var batch in all) batch.Draw(bounds);
        }

        private void AddResting(Piece p, float time)
        {
            const int T = SimConstants.CellSize;
            float wobble = (float)(System.Math.Sin(p.Wobble * 18) * p.Wobble * 0.18);
            var squash = new Vector3(1 + wobble, 1 - wobble, 1 + wobble);
            var anchor = map.Point(p.PixelX + p.Width * T / 2.0, p.PixelY + p.Height * T);
            for (int cy = 0; cy < p.Height; cy++)
            {
                for (int cx = 0; cx < p.Width; cx++)
                {
                    var cell = p.GetCell(cx, cy);
                    if (cell == null) continue;
                    var center = map.Point(p.PixelX + cx * T + T / 2.0, p.PixelY + cy * T + T / 2.0);
                    if (wobble != 0) center = anchor + Vector3.Scale(center - anchor, squash);
                    bool covered = cy > 0 && p.GetCell(cx, cy - 1) != null;
                    AddCell(cell, center, Quaternion.identity, squash, covered, time);
                }
            }
        }

        /// <summary>Draws the original standing column rotating about its bottom cell.</summary>
        private void AddToppling(ToppleAnimation topple)
        {
            var pivot = map.Point(topple.PivotX, topple.PivotY);
            var rotation = Quaternion.Euler(0f, 0f, -(float)(topple.Angle * Mathf.Rad2Deg));
            int n = topple.OriginalColumn.Count;
            for (int i = 0; i < n; i++)
            {
                var cell = topple.OriginalColumn[n - 1 - i];
                if (cell == null) continue;
                AddCell(cell, pivot + rotation * new Vector3(0f, i, 0f), rotation, Vector3.one, i < n - 1, 0f);
            }
        }

        private void AddCell(Cell cell, Vector3 center, Quaternion rotation, Vector3 scale, bool covered, float time)
        {
            if (cell.Type == CellType.Order)
            {
                AddOrderSlice(center, time);
                return;
            }

            if (cell.IsMelting)
            {
                float keep = 1f - MeltSquash * (float)cell.MeltProgress;
                center += rotation * new Vector3(0f, -0.5f * (1f - keep) * scale.y, 0f);
                scale.y *= keep;
            }

            cellBatches[cell.Type].Add(Matrix4x4.TRS(center, rotation, scale));

            if (cell.Type == CellType.Strawberry)
            {
                var berry = Matrix4x4.TRS(center + rotation * new Vector3(0f, -0.42f * scale.y, -0.5f), rotation,
                    scale * StrawberryScale);
                foreach (var batch in strawberryBatches) batch.Add(berry);
            }

            if (!covered && (cell.Type == CellType.Cream || cell.Type == CellType.Sponge))
                capBatch.Add(Matrix4x4.TRS(center + rotation * new Vector3(0f, 0.5f * scale.y, 0f), rotation, scale));
        }

        private void AddOrderSlice(Vector3 center, float time)
        {
            var position = center + new Vector3(0f, -0.48f + Mathf.Sin(time * 4f) * 0.04f, 0f);
            var rotation = Quaternion.Euler(0f, 180f + Mathf.Sin(time * 2f) * 25f, 0f);
            var matrix = Matrix4x4.TRS(position, rotation, Vector3.one * 0.95f);
            foreach (var batch in sliceBatches) batch.Add(matrix);
        }

        private void AddSubMeshBatches(Mesh mesh, Material[] materials, List<InstanceBatch> into)
        {
            if (mesh == null || materials == null) return;
            for (int i = 0; i < mesh.subMeshCount && i < materials.Length; i++)
                into.Add(Track(new InstanceBatch(mesh, i, materials[i])));
        }

        private InstanceBatch Track(InstanceBatch batch)
        {
            all.Add(batch);
            return batch;
        }
    }
}
