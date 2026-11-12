using KANYE.Gameplay;
using UnityEngine;

namespace KANYE.Systems
{
    public enum GameMode
    {
        Campaign,
        RogueExpedition
    }

    public static class GameRules
    {
        private static GameRulesConfig config;

        public static GameMode CurrentMode = GameMode.Campaign;

        public static int ParriesThisRun { get; private set; }
        public static int SupersUsedThisRun { get; private set; }
        public static float MissionStartTime { get; private set; }

        public static int ActiveMissionProgress =>
            Mathf.Max(MissionCatalog.Selected, SaveManager.Instance != null ? SaveManager.Instance.CurrentMission : 0);

        public static bool IsParryUnlocked => CurrentMode == GameMode.RogueExpedition || ActiveMissionProgress >= 1;
        public static bool IsEXUnlocked => CurrentMode == GameMode.RogueExpedition || ActiveMissionProgress >= 3;
        public static bool IsSuperUnlocked => CurrentMode == GameMode.RogueExpedition || ActiveMissionProgress >= 6;

        public static void ResetMissionStats()
        {
            ParriesThisRun = 0;
            SupersUsedThisRun = 0;
            MissionStartTime = Time.time;
        }

        public static void NotifyParry()
        {
            ParriesThisRun++;
            SaveManager.Instance?.AddCoins(3);
        }

        public static void NotifySuperUsed()
        {
            SupersUsedThisRun++;
        }

        public static string CalculateGrade(int currentHp, int maxHp, int parries, float clearTime)
        {
            if (currentHp >= maxHp && parries >= 2)
                return "S";
            if (currentHp >= maxHp)
                return "A+";
            if (currentHp >= 2)
                return "A";
            if (currentHp >= 1)
                return "B";
            return "C";
        }

        public static GameRulesConfig Config
        {
            get
            {
                if (config == null)
                    config = Resources.Load<GameRulesConfig>("GameRulesConfig");
                if (config == null)
                    config = ScriptableObject.CreateInstance<GameRulesConfig>();
                return config;
            }
        }

        public static void NotifyAsteroidDestroyed(int albums)
        {
            var hype = Object.FindAnyObjectByType<HypeState>();
            var wasFull = hype != null && hype.Hype >= hype.HypeMax;
            if (hype != null && albums > 0)
                hype.AddAlbum(albums);

            var coinDrop = albums >= 2 ? 2 : 1;
            SaveManager.Instance?.AddCoins(coinDrop);
            RegisterKill();

            var achievements = AchievementManager.Instance;
            if (achievements == null)
                return;

            AddEach(achievements, Config.killAchievementIds, 1);
            AddEach(achievements, RankIds, 1);
            if (albums > 0)
                AddEach(achievements, Config.albumAchievementIds, albums);

            if (!wasFull && hype != null && hype.Hype >= hype.HypeMax)
                achievements.UnlockAchievement(Config.hypeFullAchievementId);
        }

        public static void NotifyMissileDestroyed()
        {
            SaveManager.Instance?.AddCoins(2);
            RegisterKill();
            var achievements = AchievementManager.Instance;
            if (achievements == null)
                return;

            AddEach(achievements, RankIds, 1);
            achievements.AddProgress("missile_hunter", 1);
        }

        public static void NotifyMissionCleared(int missionIndex, bool untouched)
        {
            var achievements = AchievementManager.Instance;
            if (achievements == null)
                return;

            if (missionIndex == 0)
                achievements.UnlockAchievement("stage_neon_city");
            else if (missionIndex == 1)
                achievements.UnlockAchievement("stage_ocean");
            else if (missionIndex == 2)
                achievements.UnlockAchievement("stage_ice_planet");
            else if (missionIndex == 3)
                achievements.UnlockAchievement("stage_deep_space");
            else if (missionIndex == 4)
                achievements.UnlockAchievement("stage_home_base");

            if (untouched)
                achievements.UnlockAchievement("clean_run");
        }

        private static readonly string[] RankIds =
        {
            "rank_warrior_1",
            "rank_warrior_2",
            "rank_warrior_3",
            "rank_veteran",
            "rank_ace"
        };

        private static void RegisterKill()
        {
            SaveManager.Instance?.RegisterKill();
        }

        public static void NotifyGameplayStarted()
        {
            AchievementManager.Instance?.UnlockAchievement(Config.gameplayStageAchievementId);
        }

        public static void NotifyStageReached(string achievementId)
        {
            AchievementManager.Instance?.UnlockAchievement(achievementId);
        }

        private static void AddEach(AchievementManager achievements, string[] ids, int amount)
        {
            if (ids == null)
                return;

            for (var i = 0; i < ids.Length; i++)
                achievements.AddProgress(ids[i], amount);
        }
    }
}
