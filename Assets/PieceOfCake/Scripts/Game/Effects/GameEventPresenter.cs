using System.Collections.Generic;
using PieceOfCake.Core;
using PieceOfCake.Game.Audio;
using PieceOfCake.Game.Content;
using PieceOfCake.Game.World;
using UnityEngine;

namespace PieceOfCake.Game.Effects
{
    /// <summary>Turns simulation events into sound, particles and camera shake.</summary>
    public sealed class GameEventPresenter
    {
        private static readonly Color White = Color.white;
        private static readonly Color Pink = new Color(1f, 0.56f, 0.66f);
        private static readonly Color Yellow = new Color(1f, 0.89f, 0.54f);
        private static readonly Color Cocoa = new Color(0.71f, 0.48f, 0.29f);
        private static readonly Color Gleam = new Color(1f, 0.96f, 0.78f);
        private static readonly Color Water = new Color(0.61f, 0.85f, 1f);
        private static readonly Color Heart = new Color(1f, 0.44f, 0.56f);

        private readonly GameArt art;
        private readonly ISfxPlayer sfx;
        private readonly EffectsPlayer effects;
        private readonly CameraRig camera;
        private readonly WorldMapper map;

        public GameEventPresenter(GameArt art, ISfxPlayer sfx, EffectsPlayer effects, CameraRig camera, in WorldMapper map)
        {
            this.art = art;
            this.sfx = sfx;
            this.effects = effects;
            this.camera = camera;
            this.map = map;
        }

        public void Present(IReadOnlyList<GameEvent> events)
        {
            foreach (var e in events) Present(e);
        }

        /// <summary>Ambient effects that depend on state rather than events.</summary>
        public void PresentAmbient(StageSimulation sim)
        {
            var p = sim.Player;
            var chest = map.Point(p.CenterX, p.Y + 10);
            if (p.HasPower && Random.value < 0.3f) effects.Burst(ParticleKind.Spark, chest + RandomOffset(), Pink, 1);
            if (p.IsStrong && Random.value < 0.3f) effects.Burst(ParticleKind.Spark, chest + RandomOffset(), Cocoa, 1);
            if (sim.Phase == GamePhase.Delivering && Random.value < 0.2f)
            {
                var goal = map.CellBottom(sim.Layout.Goal) + new Vector3(Random.Range(-0.6f, 0.6f), 2.2f, 0f);
                effects.Burst(ParticleKind.Heart, goal, Heart, 1);
            }
        }

        private void Present(in GameEvent e)
        {
            var at = map.Point(e.X, e.Y);
            var span = new Vector3(WorldMapper.Length(e.SpanX), WorldMapper.Length(e.SpanY), 0f);
            switch (e.Type)
            {
                case GameEventType.Jump: sfx.Play(SoundId.Jump); break;
                case GameEventType.JellyBounce: sfx.Play(SoundId.Boing); break;
                case GameEventType.KnifeSwing: sfx.Play(SoundId.Swish); break;
                case GameEventType.CakeCut:
                    sfx.Play(SoundId.Cut);
                    // The span runs downwards in simulation space; start the burst at its bottom.
                    effects.Burst(ParticleKind.Crumb, at - new Vector3(0f, span.y, 0f), CakeColor(CellType.Sponge), 14, new Vector3(0f, span.y, 0f));
                    break;
                case GameEventType.ChocolateClink:
                    sfx.Play(SoundId.Clink);
                    camera.Shake(3f);
                    effects.Burst(ParticleKind.Spark, at, Gleam, 6);
                    break;
                case GameEventType.CakePushed: sfx.Play(SoundId.Push); break;
                case GameEventType.ToppleLanded:
                    sfx.Play(SoundId.Thud);
                    camera.Shake(6f);
                    effects.Burst(ParticleKind.Crumb, at, CakeColor(CellType.Sponge), 16, span);
                    break;
                case GameEventType.PieceLanded:
                    sfx.Play(SoundId.Thud);
                    effects.Burst(ParticleKind.Crumb, at, CakeColor(CellType.Cream), 8, span);
                    break;
                case GameEventType.CellEaten:
                    if (e.CellType != CellType.Strawberry && e.CellType != CellType.Chocolate) sfx.Play(SoundId.Eat);
                    effects.Burst(ParticleKind.Crumb, at, CakeColor(e.CellType), 10);
                    break;
                case GameEventType.PowerUp:
                    sfx.Play(SoundId.PowerUp);
                    effects.Burst(ParticleKind.Spark, at, e.CellType == CellType.Strawberry ? Pink : Cocoa, 10);
                    break;
                case GameEventType.Refused: sfx.Play(SoundId.Refuse); break;
                case GameEventType.AntSquashed:
                    sfx.Play(SoundId.Squish);
                    effects.Burst(ParticleKind.Spark, at, White, 8);
                    break;
                case GameEventType.OrderPickedUp:
                    sfx.Play(SoundId.Pickup);
                    effects.Burst(ParticleKind.Spark, at, Pink, 5);
                    effects.Burst(ParticleKind.Spark, at, Yellow, 5);
                    effects.Burst(ParticleKind.Spark, at, White, 4);
                    break;
                case GameEventType.CheckpointReached: sfx.Play(SoundId.Checkpoint); break;
                case GameEventType.PlayerMissed: sfx.Play(SoundId.Miss); break;
                case GameEventType.CellMelted:
                    sfx.Play(SoundId.Melt);
                    effects.Burst(ParticleKind.Drop, at, Water, 6);
                    break;
                case GameEventType.OrderLost: sfx.Play(SoundId.Fail); break;
                case GameEventType.OrderDelivered:
                    sfx.Play(SoundId.Clear);
                    effects.Burst(ParticleKind.Heart, at + new Vector3(0f, 2f, 0f), Heart, 10, new Vector3(1f, 0.5f, 0f));
                    break;
            }
        }

        private Color CakeColor(CellType type)
        {
            var material = type == CellType.Order ? null : art.CellMaterial(type);
            return material != null ? material.color : White;
        }

        private static Vector3 RandomOffset() => new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.4f, 0.4f), 0f);
    }
}
