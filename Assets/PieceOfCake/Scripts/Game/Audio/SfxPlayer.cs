using System;
using System.Collections.Generic;
using UnityEngine;

namespace PieceOfCake.Game.Audio
{
    public interface ISfxPlayer
    {
        bool Muted { get; set; }
        void Play(SoundId id);
    }

    /// <summary>Plays synthesised clips through a small pool of 2D audio sources.</summary>
    public sealed class SfxPlayer : ISfxPlayer, IDisposable
    {
        private const int Voices = 8;
        /// <summary>Melting can trigger many times per second; limit how often it is heard.</summary>
        private const float MeltInterval = 0.12f;

        private readonly Dictionary<SoundId, AudioClip> clips = new Dictionary<SoundId, AudioClip>();
        private readonly AudioSource[] sources = new AudioSource[Voices];
        private readonly GameObject host;
        private int nextVoice;
        private float lastMeltTime = -1f;

        public SfxPlayer(Transform parent, bool muted)
        {
            host = new GameObject("Sfx");
            host.transform.SetParent(parent, false);
            for (int i = 0; i < Voices; i++)
            {
                var source = host.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                sources[i] = source;
            }

            foreach (SoundId id in Enum.GetValues(typeof(SoundId))) clips[id] = SfxSynth.Build(id);
            Muted = muted;
        }

        public bool Muted { get; set; }

        public void Play(SoundId id)
        {
            if (Muted) return;
            if (id == SoundId.Melt)
            {
                if (Time.unscaledTime - lastMeltTime < MeltInterval) return;
                lastMeltTime = Time.unscaledTime;
            }

            var source = sources[nextVoice];
            nextVoice = (nextVoice + 1) % Voices;
            source.PlayOneShot(clips[id]);
        }

        public void Dispose()
        {
            foreach (var clip in clips.Values) UnityEngine.Object.Destroy(clip);
            clips.Clear();
            if (host != null) UnityEngine.Object.Destroy(host);
        }
    }
}
