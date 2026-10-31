using UnityEngine;

namespace PieceOfCake.Game.Effects
{
    public enum ParticleKind
    {
        /// <summary>Cake crumbs: thrown up, fall fast.</summary>
        Crumb,
        /// <summary>Sparkles: burst outwards, drift.</summary>
        Spark,
        /// <summary>Melt-water drops: fall.</summary>
        Drop,
        /// <summary>Hearts from a happy customer: float up.</summary>
        Heart,
    }

    /// <summary>Particle bursts. One particle system per kind; each burst sets its own colour.</summary>
    public sealed class EffectsPlayer
    {
        private readonly GameObject root;
        private readonly ParticleSystem[] systems = new ParticleSystem[4];
        private readonly Texture2D dot;
        private readonly Material material;

        public EffectsPlayer(Material particleMaterial, Transform parent)
        {
            root = new GameObject("Effects");
            root.transform.SetParent(parent, false);
            dot = CreateDot();
            material = new Material(particleMaterial) { mainTexture = dot };
            systems[(int)ParticleKind.Crumb] = CreateSystem("Crumbs", gravity: 35f / 9.81f);
            systems[(int)ParticleKind.Spark] = CreateSystem("Sparks", gravity: 5f / 9.81f);
            systems[(int)ParticleKind.Drop] = CreateSystem("Drops", gravity: 22.5f / 9.81f);
            systems[(int)ParticleKind.Heart] = CreateSystem("Hearts", gravity: 0f);
        }

        public void Burst(ParticleKind kind, Vector3 position, Color color, int count, Vector3 spread = default)
        {
            var system = systems[(int)kind];
            for (int i = 0; i < count; i++)
            {
                var offset = new Vector3(Random.Range(0f, spread.x), Random.Range(0f, spread.y), 0f);
                var emit = new ParticleSystem.EmitParams
                {
                    position = position + offset + new Vector3(0f, 0f, -0.6f),
                    velocity = Velocity(kind),
                    startColor = color,
                    startSize = Size(kind),
                    startLifetime = Lifetime(kind),
                };
                system.Emit(emit, 1);
            }
        }

        private static Vector3 Velocity(ParticleKind kind)
        {
            switch (kind)
            {
                case ParticleKind.Crumb: return new Vector3(Random.Range(-2.75f, 2.75f), Random.Range(1f, 7.5f), Random.Range(-1f, 0.5f));
                case ParticleKind.Spark: return new Vector3(Random.Range(-3f, 3f), Random.Range(-1.2f, 4.2f), Random.Range(-1f, 0.5f));
                case ParticleKind.Drop: return new Vector3(Random.Range(-1f, 1f), Random.Range(-1.5f, 0f), 0f);
                default: return new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(1.5f, 2.5f), 0f);
            }
        }

        private static float Size(ParticleKind kind) =>
            kind == ParticleKind.Heart ? 0.32f : kind == ParticleKind.Drop ? 0.12f : Random.Range(0.08f, 0.16f);

        private static float Lifetime(ParticleKind kind) =>
            kind == ParticleKind.Heart ? 1.4f : kind == ParticleKind.Crumb ? Random.Range(0.6f, 0.9f) : 0.65f;

        private ParticleSystem CreateSystem(string name, float gravity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = gravity;
            main.maxParticles = 600;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;
            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Play();
            return ps;
        }

        private static Texture2D CreateDot()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "ParticleDot", wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 3f)));
                }
            }

            texture.Apply();
            return texture;
        }

        public void Dispose()
        {
            Object.Destroy(root);
            Object.Destroy(material);
            Object.Destroy(dot);
        }
    }
}
