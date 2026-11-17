using System;
using System.Collections.Generic;

namespace KANYE.Systems
{
    [Serializable]
    public class AchievementProgressEntry
    {
        public string id;
        public int amount;
        public bool unlocked;
    }

    [Serializable]
    public class WeaponUpgradeEntry
    {
        public string weaponId;
        public int level = 1;
    }

    [Serializable]
    public class GameData
    {
        public int coins;
        public int starCores;
        public int hullMetaLevel = 1;
        public int hypeMetaLevel = 1;
        public int magnetMetaLevel = 1;
        public int parryMetaLevel = 1;
        public int dailyLoginDay = 1;
        public string lastDailyClaimDate = "";
        public int totalKills;
        public int currentMission;
        public bool introCutsceneSeen;
        public string equippedSkinId = "default";
        public List<string> unlockedSkinIds = new List<string>();
        public string equippedWeaponId = "twin_blaster";
        public List<string> unlockedWeaponIds = new List<string>();
        public List<WeaponUpgradeEntry> weaponUpgrades = new List<WeaponUpgradeEntry>();
        public List<AchievementProgressEntry> achievements = new List<AchievementProgressEntry>();
    }
}
