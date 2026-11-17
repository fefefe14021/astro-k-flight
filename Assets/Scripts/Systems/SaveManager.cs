using System;
using System.Collections.Generic;
using KANYE.Gameplay;
using UnityEngine;

namespace KANYE.Systems
{
    [DefaultExecutionOrder(-200)]
    public class SaveManager : MonoBehaviour
    {
        public const string SaveKey = "KANYE.SaveData";
        public const string DefaultSkinId = "default";
        public const string DefaultWeaponId = "twin_blaster";

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
            if (Data.unlockedWeaponIds == null)
                Data.unlockedWeaponIds = new List<string>();
            if (Data.weaponUpgrades == null)
                Data.weaponUpgrades = new List<WeaponUpgradeEntry>();
            if (Data.achievements == null)
                Data.achievements = new List<AchievementProgressEntry>();

            if (string.IsNullOrEmpty(Data.equippedSkinId))
                Data.equippedSkinId = DefaultSkinId;
            if (!Data.unlockedSkinIds.Contains(DefaultSkinId))
                Data.unlockedSkinIds.Add(DefaultSkinId);

            if (string.IsNullOrEmpty(Data.equippedWeaponId))
                Data.equippedWeaponId = DefaultWeaponId;
            if (!Data.unlockedWeaponIds.Contains(DefaultWeaponId))
                Data.unlockedWeaponIds.Add(DefaultWeaponId);
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

        public void AddStarCores(int amount)
        {
            if (amount == 0 || Data == null)
                return;

            Data.starCores = Mathf.Max(0, Data.starCores + amount);
            Save();
        }

        public bool TrySpendStarCores(int amount)
        {
            if (amount < 0 || Data == null || Data.starCores < amount)
                return false;

            Data.starCores -= amount;
            Save();
            return true;
        }

        public int GetMetaLevel(string metaId)
        {
            if (Data == null) return 1;
            switch (metaId)
            {
                case "hull": return Mathf.Max(1, Data.hullMetaLevel);
                case "hype": return Mathf.Max(1, Data.hypeMetaLevel);
                case "magnet": return Mathf.Max(1, Data.magnetMetaLevel);
                case "parry": return Mathf.Max(1, Data.parryMetaLevel);
                default: return 1;
            }
        }

        public bool CanUpgradeMeta(string metaId)
        {
            if (Data == null) return false;
            var def = MetaTechCatalog.Find(metaId);
            var currentLvl = GetMetaLevel(metaId);
            if (currentLvl >= def.MaxLevel) return false;
            var (costCredits, costCores) = def.GetCost(currentLvl);
            return Data.coins >= costCredits && Data.starCores >= costCores;
        }

        public bool TryUpgradeMeta(string metaId)
        {
            if (!CanUpgradeMeta(metaId)) return false;
            var def = MetaTechCatalog.Find(metaId);
            var currentLvl = GetMetaLevel(metaId);
            var (costCredits, costCores) = def.GetCost(currentLvl);
            return UpgradeMeta(metaId, costCredits, costCores);
        }

        public bool UpgradeMeta(string metaId, int costCredits, int costCores = 0)
        {
            if (Data == null) return false;
            if (Data.coins < costCredits || Data.starCores < costCores) return false;

            Data.coins -= costCredits;
            Data.starCores -= costCores;

            switch (metaId)
            {
                case "hull": Data.hullMetaLevel = Mathf.Min(5, Data.hullMetaLevel + 1); break;
                case "hype": Data.hypeMetaLevel = Mathf.Min(5, Data.hypeMetaLevel + 1); break;
                case "magnet": Data.magnetMetaLevel = Mathf.Min(4, Data.magnetMetaLevel + 1); break;
                case "parry": Data.parryMetaLevel = Mathf.Min(3, Data.parryMetaLevel + 1); break;
            }

            Save();
            return true;
        }

        #region Daily Login Retention Loop

        public bool IsDailyRewardAvailable()
        {
            if (Data == null) return true;
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return string.IsNullOrEmpty(Data.lastDailyClaimDate) || Data.lastDailyClaimDate != today;
        }

        public int GetCurrentDailyDay()
        {
            if (Data == null) return 1;
            return Mathf.Clamp(Data.dailyLoginDay, 1, 7);
        }

        public (int credits, int cores) GetDailyRewardValues(int day)
        {
            switch (day)
            {
                case 1: return (50, 0);
                case 2: return (75, 0);
                case 3: return (100, 1);
                case 4: return (150, 0);
                case 5: return (200, 1);
                case 6: return (250, 0);
                case 7: return (500, 3);
                default: return (50, 0);
            }
        }

        public bool ClaimDailyReward(out int credits, out int cores)
        {
            credits = 0;
            cores = 0;
            if (!IsDailyRewardAvailable()) return false;

            var day = GetCurrentDailyDay();
            var reward = GetDailyRewardValues(day);
            credits = reward.credits;
            cores = reward.cores;

            Data.coins += credits;
            Data.starCores += cores;
            Data.lastDailyClaimDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            Data.dailyLoginDay = (day % 7) + 1;

            Save();
            return true;
        }

        #endregion

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

        public bool UnlockWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId) || Data.unlockedWeaponIds.Contains(weaponId))
                return false;

            Data.unlockedWeaponIds.Add(weaponId);
            Save();
            return true;
        }

        public bool EquipWeapon(string weaponId)
        {
            if (!Data.unlockedWeaponIds.Contains(weaponId))
                return false;

            Data.equippedWeaponId = weaponId;
            Save();
            return true;
        }

        public int GetWeaponLevel(string weaponId)
        {
            if (Data == null || Data.weaponUpgrades == null || string.IsNullOrEmpty(weaponId))
                return 1;

            for (var i = 0; i < Data.weaponUpgrades.Count; i++)
            {
                if (Data.weaponUpgrades[i].weaponId == weaponId)
                    return Mathf.Clamp(Data.weaponUpgrades[i].level, 1, 3);
            }

            return 1;
        }

        public int GetUpgradeCost(string weaponId)
        {
            var lvl = GetWeaponLevel(weaponId);
            return WeaponCatalog.GetUpgradeCost(weaponId, lvl);
        }

        public bool IsWeaponAccessible(string weaponId)
        {
            var def = WeaponCatalog.Find(weaponId);
            if (def == null) return false;
            return CurrentMission >= def.RequiredMissionIndex;
        }

        public bool IsSkinAccessible(string skinId)
        {
            var s = ShipSkinCatalog.Find(skinId);
            if (s == null) return false;
            return CurrentMission >= s.RequiredMissionIndex;
        }

        public bool UpgradeWeapon(string weaponId)
        {
            var cost = GetUpgradeCost(weaponId);
            if (cost < 0) return false;

            if (!TrySpendCoins(cost))
                return false;

            if (Data.weaponUpgrades == null)
                Data.weaponUpgrades = new List<WeaponUpgradeEntry>();

            WeaponUpgradeEntry entry = null;
            for (var i = 0; i < Data.weaponUpgrades.Count; i++)
            {
                if (Data.weaponUpgrades[i].weaponId == weaponId)
                {
                    entry = Data.weaponUpgrades[i];
                    break;
                }
            }

            if (entry == null)
            {
                entry = new WeaponUpgradeEntry { weaponId = weaponId, level = 1 };
                Data.weaponUpgrades.Add(entry);
            }

            entry.level = Mathf.Clamp(entry.level + 1, 1, 3);
            Save();
            return true;
        }

        public int CurrentMission => Data == null ? 0 : Mathf.Max(0, Data.currentMission);
        public bool HasSeenIntro => Data != null && Data.introCutsceneSeen;

        public void MarkIntroSeen(bool seen = true)
        {
            if (Data == null) return;
            Data.introCutsceneSeen = seen;
            Save();
        }

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

        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey("TutorialShown");
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
            Data = CreateDefault();
            Save();
            Changed?.Invoke();
        }

        private static GameData CreateDefault()
        {
            return new GameData
            {
                coins = 0,
                equippedSkinId = DefaultSkinId,
                unlockedSkinIds = new List<string> { DefaultSkinId },
                equippedWeaponId = DefaultWeaponId,
                unlockedWeaponIds = new List<string> { DefaultWeaponId },
                weaponUpgrades = new List<WeaponUpgradeEntry>(),
                achievements = new List<AchievementProgressEntry>()
            };
        }
    }
}
