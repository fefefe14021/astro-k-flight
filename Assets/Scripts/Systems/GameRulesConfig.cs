using UnityEngine;

namespace KANYE.Systems
{
    [CreateAssetMenu(fileName = "GameRulesConfig", menuName = "KANYE/Game Rules")]
    public class GameRulesConfig : ScriptableObject
    {
        public int coinsPerAsteroid = 10;
        public string[] killAchievementIds = { "kill_asteroids_1", "kill_asteroids_100" };
        public string[] albumAchievementIds = { "albums_1", "albums_50" };
        public string hypeFullAchievementId = "hype_full";
        public string gameplayStageAchievementId = "stage_neon_city";
    }
}
