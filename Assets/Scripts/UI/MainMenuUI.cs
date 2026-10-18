using UnityEngine;
using UnityEngine.UI;
using KANYE.Services;

namespace KANYE.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Ссылки на объекты")]
        [SerializeField] private Button playButton;
        [SerializeField] private SceneLoader sceneLoader;

        private void Start()
        {
            if (playButton != null && sceneLoader != null)
            {
                playButton.onClick.AddListener(sceneLoader.LoadGameplay);
            }
        }
    }
}