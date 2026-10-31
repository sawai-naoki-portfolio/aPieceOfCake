using System.Collections.Generic;
using PieceOfCake.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Builds one mesh for all solid terrain of a stage: block faces in sub-mesh 0 and a decorative
    /// frosting slab on every exposed top in sub-mesh 1. Hidden faces between blocks are skipped.
    /// </summary>
    public static class TerrainBuilder
    {
        public const float FrontZ = -0.5f;
        public const float BackZ = 2.2f;
        private const float TopThickness = 0.14f;
        private const float TopOverhang = 0.04f;

        public static Mesh Build(LevelLayout layout)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var body = new List<int>();
            var tops = new List<int>();

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    if (layout.TileAt(x, y) != TileType.Solid) continue;
                    float left = x, right = x + 1, top = layout.Height - y, bottom = top - 1;
                    bool openAbove = !IsSolid(layout, x, y - 1);
                    bool openBelow = y + 1 < layout.Height && !IsSolid(layout, x, y + 1);

                    AddQuad(vertices, normals, body, Vector3.back,
                        new Vector3(left, bottom, FrontZ), new Vector3(left, top, FrontZ),
                        new Vector3(right, top, FrontZ), new Vector3(right, bottom, FrontZ));
                    if (!IsSolid(layout, x - 1, y))
                        AddQuad(vertices, normals, body, Vector3.left,
                            new Vector3(left, bottom, BackZ), new Vector3(left, top, BackZ),
                            new Vector3(left, top, FrontZ), new Vector3(left, bottom, FrontZ));
                    if (!IsSolid(layout, x + 1, y))
                        AddQuad(vertices, normals, body, Vector3.right,
                            new Vector3(right, bottom, FrontZ), new Vector3(right, top, FrontZ),
                            new Vector3(right, top, BackZ), new Vector3(right, bottom, BackZ));
                    if (openBelow)
                        AddQuad(vertices, normals, body, Vector3.down,
                            new Vector3(left, bottom, BackZ), new Vector3(left, bottom, FrontZ),
                            new Vector3(right, bottom, FrontZ), new Vector3(right, bottom, BackZ));
                    if (openAbove) AddTopSlab(vertices, normals, tops, left, right, top, layout, x, y);
                }
            }

            var mesh = new Mesh { name = "Terrain", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(body, 0);
            mesh.SetTriangles(tops, 1);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static bool IsSolid(LevelLayout layout, int x, int y) => layout.TileAt(x, y) == TileType.Solid;

        private static void AddTopSlab(List<Vector3> v, List<Vector3> n, List<int> tris, float left, float right, float top,
            LevelLayout layout, int x, int y)
        {
            // Overhang only at the ends of a ledge so neighbouring slabs join seamlessly.
            float l = IsSolid(layout, x - 1, y) && !IsSolid(layout, x - 1, y - 1) ? left : left - TopOverhang;
            float r = IsSolid(layout, x + 1, y) && !IsSolid(layout, x + 1, y - 1) ? right : right + TopOverhang;
            float y0 = top - TopThickness * 0.5f, y1 = top + TopThickness * 0.5f;
            float z0 = FrontZ - TopOverhang, z1 = BackZ;
            AddQuad(v, n, tris, Vector3.up, new Vector3(l, y1, z0), new Vector3(l, y1, z1), new Vector3(r, y1, z1), new Vector3(r, y1, z0));
            AddQuad(v, n, tris, Vector3.back, new Vector3(l, y0, z0), new Vector3(l, y1, z0), new Vector3(r, y1, z0), new Vector3(r, y0, z0));
            AddQuad(v, n, tris, Vector3.left, new Vector3(l, y0, z1), new Vector3(l, y1, z1), new Vector3(l, y1, z0), new Vector3(l, y0, z0));
            AddQuad(v, n, tris, Vector3.right, new Vector3(r, y0, z0), new Vector3(r, y1, z0), new Vector3(r, y1, z1), new Vector3(r, y0, z1));
        }

        /// <summary>Adds a quad given clockwise corners as seen from the side it faces.</summary>
        private static void AddQuad(List<Vector3> v, List<Vector3> n, List<int> tris, Vector3 normal,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            n.Add(normal); n.Add(normal); n.Add(normal); n.Add(normal);
            tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
        }
    }
}
