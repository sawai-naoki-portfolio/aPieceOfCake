using PieceOfCake.Game.Content;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Sky gradient (a quad that follows the camera) and far-away cakes or stars that give
    /// parallax for free through the perspective camera.
    /// </summary>
    public sealed class BackgroundView
    {
        private const float SkyDistance = 80f;
        private const float SceneryFade = 0.45f;
        private readonly GameObject root;
        private readonly Material skyMaterial;
        private readonly Material sceneryMaterial;
        private readonly Texture2D gradient;
        private readonly Transform sky;

        public BackgroundView(GameArt art, ThemeStyle theme, in WorldMapper map, Transform parent, Camera camera)
        {
            root = new GameObject("Background");
            root.transform.SetParent(parent, false);

            gradient = CreateGradient(theme.skyTop, theme.skyBottom);
            skyMaterial = new Material(art.skyMaterial) { mainTexture = gradient };
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(quad.GetComponent<Collider>());
            quad.name = "Sky";
            quad.GetComponent<MeshRenderer>().sharedMaterial = skyMaterial;
            quad.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sky = quad.transform;
            sky.SetParent(camera.transform, false);
            sky.localPosition = new Vector3(0f, 0f, SkyDistance);
            float height = 2f * SkyDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.2f;
            sky.localScale = new Vector3(height * 3f, height, 1f);
            camera.backgroundColor = theme.skyBottom;

            // Atmospheric perspective: far scenery fades towards the sky colour.
            sceneryMaterial = new Material(art.sceneryMaterial) { color = Color.Lerp(theme.scenery, theme.skyBottom, SceneryFade) };
            if (theme.starrySky) AddStars(art.cellMesh, map);
            else AddFarCakes(art.cellMesh, map);
        }

        private void AddFarCakes(Mesh mesh, in WorldMapper map)
        {
            var random = new System.Random(map.Width * 31 + map.Height);
            for (float x = -6f; x < map.Width + 6f; x += 7f + (float)random.NextDouble() * 5f)
            {
                float z = 14f + (float)random.NextDouble() * 10f;
                float width = 3f + (float)random.NextDouble() * 3f, height = 1.5f + (float)random.NextDouble() * 2.5f;
                var basePos = new Vector3(x, height / 2f - 1.5f, z);
                Add(mesh, basePos, new Vector3(width, height, 3f));
                Add(mesh, basePos + new Vector3(0f, height / 2f + 0.6f, 0f), new Vector3(width * 0.6f, 1.2f, 2f));
                Add(mesh, basePos + new Vector3(0f, height / 2f + 1.5f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
            }
        }

        private void AddStars(Mesh mesh, in WorldMapper map)
        {
            var random = new System.Random(7);
            for (int i = 0; i < 70; i++)
            {
                var position = new Vector3((float)random.NextDouble() * (map.Width + 20f) - 10f,
                    (float)random.NextDouble() * (map.Height + 10f), 25f + (float)random.NextDouble() * 10f);
                float size = 0.08f + (float)random.NextDouble() * 0.1f;
                Add(mesh, position, new Vector3(size, size, size));
            }
        }

        private void Add(Mesh mesh, Vector3 position, Vector3 scale)
        {
            var go = MeshObject.Create("Scenery", mesh, new[] { sceneryMaterial }, root.transform);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static Texture2D CreateGradient(Color top, Color bottom)
        {
            const int size = 64;
            var texture = new Texture2D(1, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "SkyGradient",
            };
            for (int y = 0; y < size; y++) texture.SetPixel(0, y, Color.Lerp(bottom, top, y / (size - 1f)));
            texture.Apply();
            return texture;
        }

        public void Dispose()
        {
            if (sky != null) Object.Destroy(sky.gameObject);
            Object.Destroy(root);
            Object.Destroy(skyMaterial);
            Object.Destroy(sceneryMaterial);
            Object.Destroy(gradient);
        }
    }
}
