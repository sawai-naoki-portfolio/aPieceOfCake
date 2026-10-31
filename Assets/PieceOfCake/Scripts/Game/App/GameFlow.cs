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

namespace PieceOfCake.Game.App
{
    public enum FlowState
    {
        Title,
        StageSelect,
        Playing,
        Paused,
        Result,
    }

    /// <summary>
    /// Screen flow: title → stage select → play ⇄ pause → result. Owns the current
    /// <see cref="StageSession"/> and connects UI actions to it.
    /// </summary>
    public sealed class GameFlow : IDisposable
    {
        private readonly LevelCatalog levels;
        private readonly GameArt art;
        private readonly UiRoot ui;
        private readonly PlayerControls controls;
        private readonly ISfxPlayer sfx;
        private readonly IProgressStore progress;
        private readonly CameraRig camera;
        private readonly Light sun;
        private readonly bool touchDevice;
        private StageSession session;

        public GameFlow(LevelCatalog levels, GameArt art, UiRoot ui, PlayerControls controls, ISfxPlayer sfx,
            IProgressStore progress, CameraRig camera, Light sun, bool touchDevice)
        {
            this.levels = levels;
            this.art = art;
            this.ui = ui;
            this.controls = controls;
            this.sfx = sfx;
            this.progress = progress;
            this.camera = camera;
            this.sun = sun;
            this.touchDevice = touchDevice;
            sfx.Muted = progress.Muted;
            WireUi();
            ShowTitle();
        }

        public FlowState State { get; private set; }

        public void Update(float deltaTime)
        {
            if (State == FlowState.Playing) UpdatePlaying(deltaTime);
            else if (State == FlowState.Paused && controls.PausePressed) Resume();
            if (State != FlowState.Playing) controls.ClearPresses();
        }

        public void Render(float deltaTime)
        {
            if (session == null) return;
            bool frozen = State == FlowState.Paused;
            session.Render(frozen ? 0f : deltaTime);
            bool inStage = State == FlowState.Playing || State == FlowState.Paused || State == FlowState.Result;
            if (inStage) ui.Hud.Render(session.Simulation, frozen ? 0f : deltaTime);
            if (State == FlowState.Playing) ui.Bubbles.Render(session.Simulation, session.View, camera.Camera);
            else ui.Bubbles.Hide();
        }

        private void UpdatePlaying(float deltaTime)
        {
            if (controls.PausePressed)
            {
                Pause();
                return;
            }

            if (controls.RetryPressed) session.RetryFromCheckpoint();
            session.Tick(deltaTime);
            foreach (var e in session.LastEvents)
            {
                if (e.Type == GameEventType.OrderLost) ui.Hud.ShowToast(Texts.OrderLostToast);
            }

            if (session.Simulation.Phase == GamePhase.Completed) ShowResult();
        }

        private void WireUi()
        {
            ui.Title.PlayPressed += OpenStageSelect;
            ui.Select.StageChosen += StartStage;
            ui.Select.BackPressed += ShowTitle;
            ui.Hud.MenuPressed += () =>
            {
                if (State == FlowState.Playing) Pause();
                else if (State == FlowState.Paused) Resume();
            };
            ui.Hud.RetryPressed += () =>
            {
                if (State == FlowState.Playing) session.RetryFromCheckpoint();
            };
            ui.Pause.ResumePressed += Resume;
            ui.Pause.CheckpointPressed += () =>
            {
                Resume();
                session.RetryFromCheckpoint();
            };
            ui.Pause.RestartPressed += () => StartStage(session.Index);
            ui.Pause.SelectPressed += OpenStageSelect;
            ui.Pause.SoundPressed += ToggleSound;
            ui.Result.NextPressed += () =>
            {
                if (levels.IsLast(session.Index)) OpenStageSelect();
                else StartStage(session.Index + 1);
            };
            ui.Result.AgainPressed += () => StartStage(session.Index);
            ui.Result.SelectPressed += OpenStageSelect;
        }

        private void ShowTitle()
        {
            State = FlowState.Title;
            if (session == null) LoadSession(0);
            SetGameplayUi(false);
            ui.ShowOnly(ui.Title);
        }

        private void OpenStageSelect()
        {
            State = FlowState.StageSelect;
            SetGameplayUi(false);
            ui.Select.Populate(levels, progress);
            ui.ShowOnly(ui.Select);
        }

        private void StartStage(int index)
        {
            index = Mathf.Clamp(index, 0, levels.Count - 1);
            LoadSession(index);
            State = FlowState.Playing;
            controls.ClearPresses();
            ui.ShowOnly(null);
            SetGameplayUi(true);
            ui.Hud.BeginStage(index, levels.Stages[index].Definition.Name);
        }

        private void Pause()
        {
            State = FlowState.Paused;
            ui.Touch.SetVisible(false);
            ui.Pause.SetMuted(progress.Muted);
            ui.ShowOnly(ui.Pause);
        }

        private void Resume()
        {
            if (State != FlowState.Paused) return;
            State = FlowState.Playing;
            ui.ShowOnly(null);
            ui.Touch.SetVisible(touchDevice);
        }

        private void ShowResult()
        {
            State = FlowState.Result;
            var result = session.Simulation.Result;
            progress.RecordStars(session.Simulation.Layout.Definition.Id, result.Stars);
            ui.Touch.SetVisible(false);
            ui.Result.Show(result, levels.IsLast(session.Index));
            ui.ShowOnly(ui.Result);
        }

        private void ToggleSound()
        {
            progress.Muted = !progress.Muted;
            sfx.Muted = progress.Muted;
            ui.Pause.SetMuted(progress.Muted);
        }

        private void SetGameplayUi(bool playing)
        {
            ui.Hud.SetVisible(playing);
            ui.Touch.SetVisible(playing && touchDevice);
        }

        private void LoadSession(int index)
        {
            session?.Dispose();
            session = new StageSession(index, levels.Stages[index], art, controls, sfx, camera, sun);
        }

        public void Dispose()
        {
            session?.Dispose();
            session = null;
        }
    }
}
