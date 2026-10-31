using System.Collections.Generic;
using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>One model per ant, matched by index (the ant list keeps its order across respawns).</summary>
    public sealed class AntViews
    {
        private const float SquashedVisibleTime = 0.5f;
        private readonly List<Transform> ants = new List<Transform>();

        public AntViews(GameArt art, Transform parent, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ant = Object.Instantiate(art.antPrefab, parent);
                ant.name = "Ant" + i;
                ants.Add(ant.transform);
            }
        }

        public void Render(IReadOnlyList<Ant> state, in WorldMapper map, float time)
        {
            for (int i = 0; i < ants.Count && i < state.Count; i++)
            {
                var a = state[i];
                var view = ants[i];
                bool visible = a.IsAlive || a.DeadTimer < SquashedVisibleTime;
                view.gameObject.SetActive(visible);
                if (!visible) continue;

                float bob = a.IsAlive ? Mathf.Abs(Mathf.Sin(time * 20f + i)) * 0.02f : 0f;
                view.position = map.Point(a.X + a.Width / 2, a.Y + a.Height) + new Vector3(0f, bob, 0f);
                // The model's head points +X with its eyes away from the camera: turn it round, mirror for rightward.
                view.rotation = Quaternion.Euler(0f, 180f, 0f);
                float flip = a.VelocityX > 0 ? -1f : 1f;
                view.localScale = a.IsAlive
                    ? new Vector3(flip, 1f, 1f)
                    : new Vector3(flip * 1.3f, 0.3f * (1f - (float)a.DeadTimer / SquashedVisibleTime), 1.3f);
            }
        }
    }
}
