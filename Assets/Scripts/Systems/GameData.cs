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
    public class GameData
    {
        public int coins;
        public string equippedSkinId = "default";
        public List<string> unlockedSkinIds = new List<string>();
        public List<AchievementProgressEntry> achievements = new List<AchievementProgressEntry>();
    }
}
