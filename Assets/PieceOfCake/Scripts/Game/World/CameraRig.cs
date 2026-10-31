using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Side-on perspective camera that frames 13.5 cells vertically (as the prototype), follows the
    /// chef with a little look-ahead, stays inside the map and supports screen shake.
    /// </summary>
    public sealed class CameraRig
    {
        public const float ViewHeight = 13.5f;
        public const float MinViewWidth = 10.5f;
        private const float FieldOfView = 32f;
        private const float Pitch = 4f;
        private const float LookAhead = 1f;
        private const float FollowSharpness = 7f;
        private const float ShakeDecayPerSecond = 30f;
        private const float ShakeUnitsPerPixel = 1f / 40f;

        private readonly Camera camera;
        private Vector2 focus;
        private float shake;

        public CameraRig(Camera camera)
        {
            this.camera = camera;
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        public Camera Camera => camera;

        /// <summary>Adds screen shake measured in prototype pixels.</summary>
        public void Shake(float pixels) => shake = Mathf.Max(shake, pixels);

        /// <summary>Moves towards the chef; <paramref name="snap"/> jumps there immediately.</summary>
        public void Follow(Vector3 chef, int facing, in WorldMapper map, float deltaTime, bool snap)
        {
            float halfHeight = ViewHeight / 2f;
            float aspect = Mathf.Max(0.1f, camera.aspect);
            if (halfHeight * aspect * 2f < MinViewWidth) halfHeight = MinViewWidth / 2f / aspect;
            float halfWidth = halfHeight * aspect;

            var target = new Vector2(chef.x + facing * LookAhead, chef.y + 0.05f * halfHeight * 2f + 0.4f);
            target.x = map.Width <= halfWidth * 2f ? map.Width / 2f : Mathf.Clamp(target.x, halfWidth, map.Width - halfWidth);
            target.y = map.Height <= halfHeight * 2f ? halfHeight : Mathf.Clamp(target.y, halfHeight, map.Height - halfHeight);

            focus = snap ? target : Vector2.Lerp(focus, target, 1f - Mathf.Exp(-deltaTime * FollowSharpness));

            shake = Mathf.Max(0f, shake - deltaTime * ShakeDecayPerSecond);
            var jitter = shake > 0 ? Random.insideUnitCircle * shake * ShakeUnitsPerPixel : Vector2.zero;

            float distance = halfHeight / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var rotation = Quaternion.Euler(Pitch, 0f, 0f);
            var lookAt = new Vector3(focus.x + jitter.x, focus.y + jitter.y, 0f);
            camera.transform.SetPositionAndRotation(lookAt - rotation * Vector3.forward * distance, rotation);
        }
    }
}
