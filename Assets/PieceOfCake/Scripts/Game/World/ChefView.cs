using PieceOfCake.Core;
using PieceOfCake.Game.Content;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Poses the chef model from <see cref="Player"/> state: position, facing, squash-and-stretch,
    /// walking feet, knife swing, eating, the carried order, the miss spin and respawn blinking.
    /// </summary>
    public sealed class ChefView
    {
        /// <summary>Three-quarter view towards the camera; facing left mirrors the model.</summary>
        private const float ViewYaw = 125f;
        private const float TurnSpeed = 14f;
        private const float IdleKnifeAngle = -0.9f;

        private readonly GameObject root;
        private readonly Transform model;
        private readonly Transform footLeft;
        private readonly Transform footRight;
        private readonly Transform knife;
        private readonly Transform carried;
        private readonly Renderer[] renderers;
        private readonly Vector3 footLeftBase;
        private readonly Vector3 footRightBase;
        private readonly Quaternion knifeBase;
        private float facing = 1f;

        public ChefView(GameArt art, Transform parent)
        {
            root = new GameObject("ChefView");
            root.transform.SetParent(parent, false);
            model = Object.Instantiate(art.chefPrefab, root.transform).transform;
            model.name = "Chef";
            footLeft = model.Find("FootL");
            footRight = model.Find("FootR");
            knife = model.Find("Knife");
            if (footLeft != null) footLeftBase = footLeft.localPosition;
            if (footRight != null) footRightBase = footRight.localPosition;
            if (knife != null) knifeBase = knife.localRotation;

            carried = MeshObject.Create("CarriedOrder", art.orderSliceMesh, art.orderSliceMaterials, root.transform).transform;
            carried.localScale = Vector3.one * 0.6f;
            renderers = root.GetComponentsInChildren<Renderer>(true);
        }

        public Transform Transform => root.transform;

        public void Render(Player p, in WorldMapper map, float time, float deltaTime)
        {
            root.transform.position = map.Point(p.CenterX, p.Bottom);

            facing = Mathf.MoveTowards(facing, p.Facing, deltaTime * TurnSpeed);
            float squash = (float)p.Squash * 0.18f;
            model.localScale = new Vector3(facing * (1f + squash), 1f - squash, 1f + squash);
            model.localRotation = Quaternion.Euler(0f, ViewYaw, 0f);
            if (p.IsDead)
            {
                model.localRotation = Quaternion.Euler(0f, 0f, -(float)p.DeadTimer * 9f * Mathf.Rad2Deg) * model.localRotation;
            }

            PoseFeet(p);
            PoseKnife(p);
            PoseEating(p);

            carried.gameObject.SetActive(p.IsCarryingOrder && !p.IsDead);
            carried.localPosition = new Vector3(0f, 1.35f + Mathf.Sin(time * 5f) * 0.05f, 0f);
            carried.localRotation = Quaternion.Euler(0f, 180f, 0f);

            bool blinkOff = p.InvulnerableTimer > 0 && Mathf.FloorToInt((float)p.InvulnerableTimer * 12f) % 2 == 1;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = !blinkOff;
            }
        }

        private void PoseFeet(Player p)
        {
            if (footLeft == null || footRight == null) return;
            float walk = p.IsGrounded && Mathf.Abs((float)p.VelocityX) > 20 ? Mathf.Sin((float)p.WalkTimer * 20f) : 0f;
            footLeft.localPosition = footLeftBase + new Vector3(0f, Mathf.Max(0f, walk) * 0.06f, 0f);
            footRight.localPosition = footRightBase + new Vector3(0f, Mathf.Max(0f, -walk) * 0.06f, 0f);
        }

        private void PoseKnife(Player p)
        {
            if (knife == null) return;
            float t = (float)(p.CutAnimationTimer / SimConstants.CutAnimationDuration);
            float angle = p.CutAnimationTimer > 0 ? Mathf.Lerp(-1.7f, 1.0f, 1f - t) : IdleKnifeAngle;
            knife.localRotation = knifeBase * Quaternion.Euler(angle * Mathf.Rad2Deg, 0f, 0f);
        }

        private void PoseEating(Player p)
        {
            if (p.EatAnimationTimer <= 0) return;
            float chew = Mathf.Abs(Mathf.Sin((float)p.EatAnimationTimer * 30f)) * 0.06f;
            model.localScale = Vector3.Scale(model.localScale, new Vector3(1f + chew, 1f - chew, 1f + chew));
        }

        public void Dispose() => Object.Destroy(root);
    }
}
