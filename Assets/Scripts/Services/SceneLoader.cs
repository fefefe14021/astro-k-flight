using KANYE.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KANYE.Services
{
    public class SceneLoader : MonoBehaviour
    {
        public const string GameplayScene = "Gameplay";
        public const string MainMenuScene = "MainMenu";

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
            SaveManager.Instance?.Save();
            LoadSceneByName(MainMenuScene);
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
