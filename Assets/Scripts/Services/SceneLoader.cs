using KANYE.Gameplay;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KANYE.Services
{
    public class SceneLoader : MonoBehaviour
    {
        public const string GameplayScene = "Gameplay";
        public const string MainMenuScene = "MainMenu";
        public const string ShopScene = "Shop";
        public const string AchievementsScene = "Achievements";

        public void LoadScene(string sceneName)
        {
            LoadSceneByName(sceneName);
        }

        public void LoadGameplay()
        {
            LoadSceneByName(GameplayScene);
        }

        public void LoadMainMenu()
        {
            ReturnToMainMenu();
        }

        public static void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SaveManager.Instance?.Save();
            if (ShouldShowMenuAd())
            {
                var sdk = YandexGamesService.Instance;
                if (sdk != null)
                {
                    sdk.ShowInterstitial(() => LoadSceneByName(MainMenuScene));
                    return;
                }
            }

            LoadSceneByName(MainMenuScene);
        }

        private static bool ShouldShowMenuAd()
        {
            if (SceneManager.GetActiveScene().name != GameplayScene)
                return false;

            var show = PlayerPrefs.GetInt("KANYE.MenuAdFlip", 0) == 0;
            PlayerPrefs.SetInt("KANYE.MenuAdFlip", show ? 1 : 0);
            PlayerPrefs.Save();
            return show;
        }

        public void LoadShop()
        {
            OpenShop();
        }

        public static void OpenShop()
        {
            SaveManager.Instance?.Save();
            LoadSceneByName(ShopScene);
        }

        public static void OpenAchievements()
        {
            SaveManager.Instance?.Save();
            LoadSceneByName(AchievementsScene);
        }

        public static void RestartFromFirstMission()
        {
            Time.timeScale = 1f;
            SaveManager.Instance?.RestartMissions();
            Gameplay.MissionCatalog.Select(0);
            LoadSceneByName(GameplayScene);
        }

        public void QuitGame()
        {
            Quit();
        }

        public static void LoadSceneByName(string sceneName)
        {
            GamePause.Release();
            Debug.Log($"Загружаем сцену: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public static void Quit()
        {
            SaveManager.Instance?.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
