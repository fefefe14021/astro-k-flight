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
            LoadSceneByName(MainMenuScene);
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

        public void QuitGame()
        {
            Quit();
        }

        public static void LoadSceneByName(string sceneName)
        {
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
