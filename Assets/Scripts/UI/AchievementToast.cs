using KANYE.Systems;
using UnityEngine;

namespace KANYE.UI
{
    public class AchievementToast : MonoBehaviour
    {
        private string message;
        private float hideAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindAnyObjectByType<AchievementToast>() != null)
                return;

            var toast = new GameObject("AchievementToast");
            DontDestroyOnLoad(toast);
            toast.AddComponent<AchievementToast>();
        }

        private bool hooked;

        private void Update()
        {
            if (hooked || AchievementManager.Instance == null)
                return;

            AchievementManager.Instance.Unlocked += Show;
            hooked = true;
        }

        private void OnDestroy()
        {
            if (hooked && AchievementManager.Instance != null)
                AchievementManager.Instance.Unlocked -= Show;
        }

        private void Show(AchievementData data)
        {
            message = data.displayName + "\n" + data.description;
            hideAt = Time.unscaledTime + 3.2f;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(message) || Time.unscaledTime > hideAt)
                return;

            var box = new Rect(Screen.width - 360f, 24f, 330f, 90f);
            GUI.Box(box, "Ачивка");
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(box.x + 12f, box.y + 28f, box.width - 24f, 54f), message, style);
        }
    }
}
