using System;
using System.Collections.Generic;
using PieceOfCake.Core;
using UnityEngine;

namespace PieceOfCake.Game.Persistence
{
    /// <summary>Player progress that survives restarts.</summary>
    public interface IProgressStore
    {
        int BestStars(string stageId);
        /// <summary>Keeps the best result; returns true when it improved.</summary>
        bool RecordStars(string stageId, int stars);
        bool Muted { get; set; }
    }

    /// <summary>
    /// Stores progress as versioned JSON in PlayerPrefs. Corrupt or tampered data is discarded or
    /// clamped rather than trusted, so a bad save can never break the game.
    /// </summary>
    public sealed class PlayerPrefsProgressStore : IProgressStore
    {
        public const string Key = "pieceofcake.progress.v1";
        private const int MaxEntries = 256;
        private const int MaxIdLength = 64;

        private readonly Dictionary<string, int> stars = new Dictionary<string, int>();
        private bool muted;

        public PlayerPrefsProgressStore()
        {
            Load();
        }

        public bool Muted
        {
            get => muted;
            set
            {
                if (muted == value) return;
                muted = value;
                Save();
            }
        }

        public int BestStars(string stageId) =>
            stageId != null && stars.TryGetValue(stageId, out int value) ? value : 0;

        public bool RecordStars(string stageId, int value)
        {
            if (!IsValidId(stageId)) return false;
            value = Mathf.Clamp(value, 0, Scoring.MaxStars);
            if (value <= BestStars(stageId)) return false;
            stars[stageId] = value;
            Save();
            return true;
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.entries == null) return;
                muted = data.muted;
                for (int i = 0; i < data.entries.Length && i < MaxEntries; i++)
                {
                    var e = data.entries[i];
                    if (e == null || !IsValidId(e.id)) continue;
                    stars[e.id] = Mathf.Clamp(e.stars, 0, Scoring.MaxStars);
                }
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning($"[PieceOfCake] Discarding unreadable save data: {e.Message}");
                stars.Clear();
            }
        }

        private void Save()
        {
            var entries = new List<StageEntry>(stars.Count);
            foreach (var pair in stars) entries.Add(new StageEntry { id = pair.Key, stars = pair.Value });
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(new SaveData { muted = muted, entries = entries.ToArray() }));
            PlayerPrefs.Save();
        }

        private static bool IsValidId(string id) => !string.IsNullOrEmpty(id) && id.Length <= MaxIdLength;

#pragma warning disable 0649
        [Serializable]
        private sealed class SaveData
        {
            public bool muted;
            public StageEntry[] entries;
        }

        [Serializable]
        private sealed class StageEntry
        {
            public string id;
            public int stars;
        }
#pragma warning restore 0649
    }
}
