// Assets/Scripts/UI/MenuFlow.cs
// Namespace: KANYE.UI
// Flow coordinator ensuring proper UI screens are present for scenes.

using KANYE.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace KANYE.UI
{
    public class MenuFlow : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying)
                return;

            if (scene.name == "Bootstrap")
                return;

            EnsureEventSystem();

            if (scene.name == SceneLoader.MainMenuScene)
                EnsureMenu();
            else if (scene.name == SceneLoader.ShopScene)
                EnsureShop();
            else if (scene.name == SceneLoader.AchievementsScene)
                EnsureAchievements();
        }

        private static void EnsureMenu()
        {
            if (Object.FindAnyObjectByType<MainMenuScreen>() != null)
                return;

            new GameObject("MainMenuScreen").AddComponent<MainMenuScreen>();
        }

        private static void EnsureShop()
        {
            if (Object.FindAnyObjectByType<ShopScreen>() != null)
                return;

            new GameObject("ShopScreen").AddComponent<ShopScreen>();
        }

        private static void EnsureAchievements()
        {
            if (Object.FindAnyObjectByType<AchievementsScreen>() != null)
                return;

            new GameObject("AchievementsScreen").AddComponent<AchievementsScreen>();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
