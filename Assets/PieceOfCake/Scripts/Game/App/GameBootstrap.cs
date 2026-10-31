using System;
using PieceOfCake.Core;
using PieceOfCake.Game.Audio;
using PieceOfCake.Game.Content;
using PieceOfCake.Game.Input;
using PieceOfCake.Game.Levels;
using PieceOfCake.Game.Persistence;
using PieceOfCake.Game.UI;
using PieceOfCake.Game.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.App
{
    /// <summary>
    /// Composition root. The only MonoBehaviour of the game: it validates scene references,
    /// builds every service once, then forwards Unity's frame callbacks to <see cref="GameFlow"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset levelFile;
        [SerializeField] private GameArt art;
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Light sun;
        [Tooltip("Show on-screen buttons even without a touch screen (for testing).")]
        [SerializeField] private bool forceTouchControls;

        private PlayerControls controls;
        private InputSystemControls deviceControls;
        private SfxPlayer sfx;
        private GameFlow flow;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            if (!ValidateReferences(out string problem))
            {
                Fail(problem);
                return;
            }

            // Desktop PCs may report a touch screen; on-screen buttons are for phones and tablets.
            bool touchDevice = forceTouchControls || Application.isMobilePlatform;
            var touch = new TouchControls();
            controls = new PlayerControls();
            deviceControls = new InputSystemControls(InputSystem.actions);
            controls.Add(deviceControls);
            controls.Add(touch);

            var ui = new UiRoot(uiDocument, touch, touchDevice);
            LevelCatalog levels;
            try
            {
                levels = LevelCatalog.Load(levelFile);
            }
            catch (LevelFormatException e)
            {
                ui.Error.Show(e.Message);
                Debug.LogError("[PieceOfCake] " + e.Message, this);
                enabled = false;
                return;
            }

            var progress = new PlayerPrefsProgressStore();
            sfx = new SfxPlayer(transform, progress.Muted);
            flow = new GameFlow(levels, art, ui, controls, sfx, progress, new CameraRig(worldCamera), sun, touchDevice);
        }

        private void Update()
        {
            if (flow == null) return;
            controls.Poll();
            flow.Update(Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            flow?.Render(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            flow?.Dispose();
            sfx?.Dispose();
            deviceControls?.Dispose();
        }

        private bool ValidateReferences(out string problem)
        {
            problem = null;
            if (levelFile == null) problem = "Level file is not assigned.";
            else if (art == null) problem = "Game Art catalogue is not assigned.";
            else if (uiDocument == null) problem = "UI Document is not assigned.";
            else if (worldCamera == null) problem = "World camera is not assigned.";
            else if (InputSystem.actions == null) problem = "Project-wide input actions are not set.";
            else
            {
                var missing = art.MissingReferences();
                if (missing.Length > 0) problem = "Game Art is missing: " + string.Join(", ", missing);
            }

            return problem == null;
        }

        private void Fail(string problem)
        {
            Debug.LogError("[PieceOfCake] " + problem + " Run Tools ▸ Piece of Cake ▸ Rebuild Art Catalog.", this);
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                try
                {
                    new ErrorScreen(uiDocument.rootVisualElement).Show(problem);
                }
                catch (InvalidOperationException)
                {
                    // The UI itself is broken; the console message above is all we can do.
                }
            }

            enabled = false;
        }
    }
}
