using System;
using UnityEngine.InputSystem;

namespace PieceOfCake.Game.Input
{
    /// <summary>Keyboard and gamepad through the "Gameplay" map of the project-wide input actions.</summary>
    public sealed class InputSystemControls : IControlSource, IDisposable
    {
        public const string MapName = "Gameplay";
        private const float AxisThreshold = 0.5f;

        private readonly InputActionMap map;
        private readonly InputAction move;
        private readonly InputAction jump;
        private readonly InputAction cut;
        private readonly InputAction eat;
        private readonly InputAction pause;
        private readonly InputAction retry;

        public InputSystemControls(InputActionAsset actions)
        {
            if (actions == null) throw new ArgumentNullException(nameof(actions), "Project-wide input actions are not set.");
            map = actions.FindActionMap(MapName, throwIfNotFound: true);
            move = map.FindAction("Move", true);
            jump = map.FindAction("Jump", true);
            cut = map.FindAction("Cut", true);
            eat = map.FindAction("Eat", true);
            pause = map.FindAction("Pause", true);
            retry = map.FindAction("Retry", true);
            map.Enable();
        }

        public ControlSample Sample()
        {
            float x = move.ReadValue<float>();
            return new ControlSample
            {
                Left = x < -AxisThreshold,
                Right = x > AxisThreshold,
                Jump = jump.WasPressedThisFrame(),
                Cut = cut.WasPressedThisFrame(),
                Eat = eat.WasPressedThisFrame(),
                Pause = pause.WasPressedThisFrame(),
                Retry = retry.WasPressedThisFrame(),
            };
        }

        public void Dispose() => map.Disable();
    }
}
