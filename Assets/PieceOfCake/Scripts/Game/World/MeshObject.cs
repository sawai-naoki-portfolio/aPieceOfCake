using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>Creates a plain GameObject that renders a mesh with the given materials.</summary>
    public static class MeshObject
    {
        public static GameObject Create(string name, Mesh mesh, Material[] materials, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = materials;
            return go;
        }
    }
}
