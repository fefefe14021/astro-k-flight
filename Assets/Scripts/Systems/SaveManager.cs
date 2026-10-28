using System;
using System.Collections.Generic;
using UnityEngine;

namespace KANYE.Systems
{
    [DefaultExecutionOrder(-200)]
    public class SaveManager : MonoBehaviour
    {
        public const string SaveKey = "KANYE.SaveData";
        public const string DefaultSkinId = "default";

        public static SaveManager Instance { get; private set; }

        public GameData Data { get; private set; }
        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Data = CreateDefault();
                Save();
                return;
            }

            Data = JsonUtility.FromJson<GameData>(PlayerPrefs.GetString(SaveKey));
            if (Data == null)
                Data = CreateDefault();

            if (Data.unlockedSkinIds == null)
                Data.unlockedSkinIds = new List<string>();
            if (Data.achievements == null)
                Data.achievements = new List<AchievementProgressEntry>();
            if (string.IsNullOrEmpty(Data.equippedSkinId))
                Data.equippedSkinId = DefaultSkinId;
            if (!Data.unlockedSkinIds.Contains(DefaultSkinId))
                Data.unlockedSkinIds.Add(DefaultSkinId);
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public void AddCoins(int amount)
        {
            if (amount == 0 || Data == null)
                return;

            Data.coins = Mathf.Max(0, Data.coins + amount);
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Data == null || Data.coins < amount)
                return false;

            Data.coins -= amount;
            Save();
            return true;
        }

        public bool UnlockSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || Data.unlockedSkinIds.Contains(skinId))
                return false;

            Data.unlockedSkinIds.Add(skinId);
            Save();
            return true;
        }

        public bool EquipSkin(string skinId)
        {
            if (!Data.unlockedSkinIds.Contains(skinId))
                return false;

            Data.equippedSkinId = skinId;
            Save();
            return true;
        }

        public int CurrentMission => Data == null ? 0 : Mathf.Max(0, Data.currentMission);

        public void RememberMission(int index)
        {
            if (Data == null)
                return;

            Data.currentMission = Mathf.Max(0, index);
            Save();
        }

        public void RestartMissions()
        {
            RememberMission(0);
        }

        public void RegisterKill()
        {
            if (Data == null)
                return;

            Data.totalKills++;
            Save();
        }

        public AchievementProgressEntry GetAchievement(string id)
        {
            for (var i = 0; i < Data.achievements.Count; i++)
            {
                if (Data.achievements[i].id == id)
                    return Data.achievements[i];
            }

            var entry = new AchievementProgressEntry { id = id };
            Data.achievements.Add(entry);
            return entry;
        }

        private static GameData CreateDefault()
        {
            return new GameData
            {
                coins = 0,
                equippedSkinId = DefaultSkinId,
                unlockedSkinIds = new List<string> { DefaultSkinId },
                achievements = new List<AchievementProgressEntry>()
            };
        }
    }
}
