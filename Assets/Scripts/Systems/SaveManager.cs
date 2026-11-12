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
            if (lvl >= 3) return -1;

            var def = WeaponCatalog.Find(weaponId);
            var basePrice = def != null ? def.Price : 40;
            if (lvl == 1)
                return Mathf.RoundToInt(basePrice * 0.75f) + 35;
            else
                return Mathf.RoundToInt(basePrice * 1.35f) + 65;
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
