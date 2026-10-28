using KANYE.Gameplay;
using UnityEngine;

namespace KANYE.Systems
{
    public static class GameRules
    {
        private static GameRulesConfig config;

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

            SaveManager.Instance?.AddCoins(Config.coinsPerAsteroid);

            var achievements = AchievementManager.Instance;
            if (achievements == null)
                return;

            AddEach(achievements, Config.killAchievementIds, 1);
            if (albums > 0)
                AddEach(achievements, Config.albumAchievementIds, albums);

            if (!wasFull && hype != null && hype.Hype >= hype.HypeMax)
                achievements.UnlockAchievement(Config.hypeFullAchievementId);
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
