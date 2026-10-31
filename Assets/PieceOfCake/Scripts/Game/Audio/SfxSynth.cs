using System;
using UnityEngine;

namespace PieceOfCake.Game.Audio
{
    /// <summary>
    /// Builds the game's sound effects procedurally from tones and filtered noise, matching the
    /// prototype's Web Audio recipes. Keeps the build free of audio assets.
    /// </summary>
    public static class SfxSynth
    {
        public const int SampleRate = 44100;
        private const float MasterGain = 2.2f;

        private enum Wave { Sine, Square, Triangle, Sawtooth }

        public static AudioClip Build(SoundId id)
        {
            var buffer = new Buffer(Duration(id));
            switch (id)
            {
                case SoundId.Jump: buffer.Tone(420, 760, 0.12f, Wave.Square, 0.04f); break;
                case SoundId.PowerUp:
                    float[] power = { 660, 880, 1175 };
                    for (int i = 0; i < power.Length; i++) buffer.Tone(power[i], power[i] * 1.02f, 0.1f, Wave.Triangle, 0.06f, i * 0.07f);
                    break;
                case SoundId.Boing: buffer.Tone(200, 820, 0.24f, Wave.Sine, 0.1f); break;
                case SoundId.Swish: buffer.Noise(0.07f, 0.05f, 3200); break;
                case SoundId.Cut:
                    buffer.Noise(0.05f, 0.14f, 2600);
                    buffer.Noise(0.09f, 0.09f, 1300, 0.04f);
                    break;
                case SoundId.Clink:
                    buffer.Tone(1900, 1800, 0.12f, Wave.Triangle, 0.08f);
                    buffer.Tone(2700, 2600, 0.1f, Wave.Sine, 0.05f, 0.02f);
                    break;
                case SoundId.Eat:
                    buffer.Tone(320, 200, 0.07f, Wave.Square, 0.045f);
                    buffer.Tone(340, 220, 0.07f, Wave.Square, 0.045f, 0.11f);
                    break;
                case SoundId.Push: buffer.Noise(0.07f, 0.07f, 320); break;
                case SoundId.Thud:
                    buffer.Tone(150, 60, 0.2f, Wave.Sine, 0.14f);
                    buffer.Noise(0.12f, 0.07f, 220);
                    break;
                case SoundId.Pickup:
                    float[] pickup = { 523, 659, 784, 1046 };
                    for (int i = 0; i < pickup.Length; i++) buffer.Tone(pickup[i], 0, 0.1f, Wave.Triangle, 0.06f, i * 0.06f);
                    break;
                case SoundId.Checkpoint:
                    buffer.Tone(880, 0, 0.12f, Wave.Triangle, 0.06f);
                    buffer.Tone(1320, 0, 0.15f, Wave.Triangle, 0.05f, 0.08f);
                    break;
                case SoundId.Squish: buffer.Tone(520, 120, 0.15f, Wave.Square, 0.06f); break;
                case SoundId.Miss: buffer.Tone(520, 180, 0.45f, Wave.Triangle, 0.08f); break;
                case SoundId.Melt: buffer.Tone(1400, 700, 0.08f, Wave.Sine, 0.03f); break;
                case SoundId.Refuse: buffer.Tone(190, 150, 0.14f, Wave.Square, 0.045f); break;
                case SoundId.Clear:
                    float[] clear = { 523, 659, 784, 659, 784, 1046 };
                    for (int i = 0; i < clear.Length; i++) buffer.Tone(clear[i], 0, 0.17f, Wave.Triangle, 0.07f, i * 0.11f);
                    break;
                case SoundId.Fail: buffer.Tone(420, 170, 0.5f, Wave.Sawtooth, 0.04f); break;
                default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }

            return buffer.ToClip("sfx_" + id);
        }

        private static float Duration(SoundId id)
        {
            switch (id)
            {
                case SoundId.Clear: return 0.9f;
                case SoundId.Miss:
                case SoundId.Fail: return 0.6f;
                case SoundId.Pickup:
                case SoundId.PowerUp: return 0.4f;
                default: return 0.32f;
            }
        }

        private sealed class Buffer
        {
            private readonly float[] samples;
            private readonly System.Random random = new System.Random(1234);

            public Buffer(float seconds)
            {
                samples = new float[Mathf.CeilToInt(seconds * SampleRate)];
            }

            /// <summary>Exponential pitch glide (f1 = 0 keeps f0) with an exponential decay envelope.</summary>
            public void Tone(float f0, float f1, float duration, Wave wave, float volume, float delay = 0f)
            {
                int start = (int)(delay * SampleRate), length = (int)(duration * SampleRate);
                double phase = 0;
                for (int i = 0; i < length && start + i < samples.Length; i++)
                {
                    float t = i / (float)length;
                    float freq = f1 > 0 ? f0 * Mathf.Pow(f1 / f0, t) : f0;
                    phase += freq / SampleRate;
                    float p = (float)(phase - Math.Floor(phase));
                    samples[start + i] += Oscillator(wave, p) * Envelope(volume, t);
                }
            }

            /// <summary>White noise through a band-pass filter (Q = 0.9) with an exponential decay.</summary>
            public void Noise(float duration, float volume, float frequency, float delay = 0f)
            {
                int start = (int)(delay * SampleRate), length = (int)(duration * SampleRate);
                double w0 = 2 * Math.PI * frequency / SampleRate, alpha = Math.Sin(w0) / (2 * 0.9);
                double a0 = 1 + alpha, b0 = alpha / a0, b2 = -alpha / a0, a1 = -2 * Math.Cos(w0) / a0, a2 = (1 - alpha) / a0;
                double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
                for (int i = 0; i < length && start + i < samples.Length; i++)
                {
                    double x = random.NextDouble() * 2 - 1;
                    double y = b0 * x + b2 * x2 - a1 * y1 - a2 * y2;
                    x2 = x1; x1 = x; y2 = y1; y1 = y;
                    samples[start + i] += (float)y * Envelope(volume, i / (float)length);
                }
            }

            public AudioClip ToClip(string name)
            {
                for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Clamp(samples[i] * MasterGain, -1f, 1f);
                var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }

            private static float Envelope(float volume, float t) => volume * Mathf.Pow(0.0001f / volume, t);

            private static float Oscillator(Wave wave, float p)
            {
                switch (wave)
                {
                    case Wave.Square: return p < 0.5f ? 1f : -1f;
                    case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
                    case Wave.Sawtooth: return 2f * p - 1f;
                    default: return Mathf.Sin(2f * Mathf.PI * p);
                }
            }
        }
    }
}
