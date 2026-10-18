using UnityEngine;
using UnityEngine.SceneManagement;

namespace KANYE.Services
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            Debug.Log($"Загружаем сцену: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadGameplay()
        {
            LoadScene("Gameplay");
        }
    }
}