using System.Collections.Generic;
using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>Static and lightly animated scenery tied to the map: forks, signs, candles and the customer.</summary>
    public sealed class PropViews
    {
        private const float FacingCamera = 180f;

        private readonly List<(Checkpoint state, Transform flame)> candles = new List<(Checkpoint, Transform)>();
        private readonly Transform customer;
        private readonly Vector3 customerBase;
        private readonly GameObject deliveredSlice;

        public PropViews(GameArt art, LevelLayout layout, in WorldMapper map, Transform parent,
            IReadOnlyList<Checkpoint> checkpoints)
        {
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    if (layout.TileAt(x, y) == TileType.Spike) Place(art.forkPrefab, map.CellBottom(new GridPoint(x, y)), parent, 0f);
                }
            }

            foreach (var sign in layout.Signs) Place(art.signPrefab, map.CellBottom(sign.Cell, 0.35f), parent, FacingCamera);

            foreach (var checkpoint in checkpoints)
            {
                var candle = Place(art.candlePrefab, map.CellBottom(checkpoint.Cell, 0.1f), parent, 0f);
                candles.Add((checkpoint, candle.Find("Flame")));
            }

            var goal = map.CellBottom(layout.Goal);
            var table = Place(art.tablePrefab, goal + new Vector3(-0.1f, 0f, -0.1f), parent, 0f);
            customer = Place(art.CustomerPrefab(layout.Definition.Customer), goal + new Vector3(0.15f, 0f, 0.75f), parent, FacingCamera);
            customerBase = customer.localPosition;
            deliveredSlice = MeshObject.Create("DeliveredSlice", art.orderSliceMesh, art.orderSliceMaterials, table);
            deliveredSlice.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            deliveredSlice.transform.localRotation = Quaternion.Euler(0f, FacingCamera, 0f);
            deliveredSlice.transform.localScale = Vector3.one * 0.6f;
            deliveredSlice.SetActive(false);
        }

        /// <summary>World position of the customer's head, for speech bubbles.</summary>
        public Vector3 CustomerHead => customer.position + new Vector3(0f, 2.1f, 0f);

        public void Render(bool delivered, float time)
        {
            foreach (var (state, flame) in candles)
            {
                if (flame == null) continue;
                flame.gameObject.SetActive(state.IsActive);
                float flicker = 1f + Mathf.Sin(time * 14f) * 0.1f;
                flame.localScale = new Vector3(1f / flicker, flicker, 1f / flicker);
            }

            float bob = Mathf.Sin(time * 3f) * 0.04f + (delivered ? Mathf.Abs(Mathf.Sin(time * 9f)) * 0.15f : 0f);
            customer.localPosition = customerBase + new Vector3(0f, bob, 0f);
            deliveredSlice.SetActive(delivered);
        }

        private static Transform Place(GameObject prefab, Vector3 position, Transform parent, float yaw)
        {
            var go = Object.Instantiate(prefab, position, Quaternion.Euler(0f, yaw, 0f), parent);
            go.name = prefab.name;
            return go.transform;
        }
    }
}
