using System;
using System.Collections.Generic;
using PieceOfCake.Core;
using PieceOfCake.Game.Audio;
using PieceOfCake.Game.Content;
using PieceOfCake.Game.Effects;
using PieceOfCake.Game.Input;
using PieceOfCake.Game.World;
using UnityEngine;

namespace PieceOfCake.Game.App
{
    /// <summary>
    /// A stage being played: runs the simulation at a fixed 60 Hz from variable frame times,
    /// feeds it input, and keeps the views, effects and camera in sync.
    /// </summary>
    public sealed class StageSession : IDisposable
    {
        private const float StepSeconds = (float)SimConstants.FixedDeltaTime;
        /// <summary>Never simulate more than this many steps per frame (avoids a spiral after a hitch).</summary>
        private const int MaxStepsPerFrame = 6;
        private const float MaxFrameTime = 0.1f;

        private readonly PlayerControls controls;
        private readonly CameraRig camera;
        private readonly EffectsPlayer effects;
        private readonly GameEventPresenter presenter;
        private readonly List<GameEvent> events = new List<GameEvent>();
        private float accumulator;
        private float time;
        private Player lastPlayer;

        public StageSession(int index, LevelLayout layout, GameArt art, PlayerControls controls, ISfxPlayer sfx,
            CameraRig camera, Light sun)
        {
            Index = index;
            this.controls = controls;
            this.camera = camera;
            Simulation = new StageSimulation(layout);
            View = new StageView(art, Simulation, camera.Camera, sun);
            effects = new EffectsPlayer(art.particleMaterial, View.Root);
            presenter = new GameEventPresenter(art, sfx, effects, camera, View.Map);
            lastPlayer = Simulation.Player;
            FollowCamera(0f, snap: true);
        }

        public int Index { get; }
        public StageSimulation Simulation { get; }
        public StageView View { get; }

        /// <summary>Events produced during the last <see cref="Tick"/>.</summary>
        public IReadOnlyList<GameEvent> LastEvents => events;

        /// <summary>Advances the simulation by real time <paramref name="deltaTime"/>.</summary>
        public void Tick(float deltaTime)
        {
            accumulator += Mathf.Min(deltaTime, MaxFrameTime);
            int steps = 0;
            while (accumulator >= StepSeconds && steps < MaxStepsPerFrame)
            {
                Simulation.Step(controls.ConsumeFrame());
                accumulator -= StepSeconds;
                steps++;
            }

            if (steps >= MaxStepsPerFrame) accumulator = 0f;

            events.Clear();
            Simulation.DrainEvents(events);
            presenter.Present(events);
            presenter.PresentAmbient(Simulation);
        }

        public bool RetryFromCheckpoint() => Simulation.RetryFromCheckpoint();

        public void Render(float deltaTime)
        {
            time += deltaTime;
            // A respawn replaces the player: cut the camera instead of panning across the map.
            bool respawned = !ReferenceEquals(lastPlayer, Simulation.Player);
            lastPlayer = Simulation.Player;
            View.Render(Simulation, time, deltaTime);
            FollowCamera(deltaTime, respawned);
        }

        private void FollowCamera(float deltaTime, bool snap)
        {
            var p = Simulation.Player;
            camera.Follow(View.Map.Point(p.CenterX, p.Bottom), p.Facing, View.Map, deltaTime, snap);
        }

        public void Dispose()
        {
            effects.Dispose();
            View.Dispose();
        }
    }
}
