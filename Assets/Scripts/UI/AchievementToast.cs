using KANYE.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class AchievementToast : MonoBehaviour
    {
        CanvasGroup group;
        Text title;
        Text body;
        float hideAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (FindAnyObjectByType<AchievementToast>() != null)
                return;

            var toast = new GameObject("AchievementToast");
            DontDestroyOnLoad(toast);
            toast.AddComponent<AchievementToast>();
        }

        void Awake()
        {
            var canvasObject = new GameObject("ToastCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            card.transform.SetParent(canvasObject.transform, false);
            var rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);
            rect.sizeDelta = new Vector2(420f, 110f);
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = White();
            cardImage.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);
            group = card.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accent.transform.SetParent(card.transform, false);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(6f, 0f);
            var accentImage = accent.GetComponent<Image>();
            accentImage.sprite = White();
            accentImage.color = new Color(1f, 0.72f, 0.2f, 1f);

            title = Label(card.transform, "Title", "Ачивка", 22, new Vector2(16f, -12f), new Color(1f, 0.86f, 0.45f));
            body = Label(card.transform, "Body", "", 18, new Vector2(16f, -48f), Color.white);
        }

        bool hooked;

        void Update()
        {
            if (!hooked && AchievementManager.Instance != null)
            {
                AchievementManager.Instance.Unlocked += Show;
                hooked = true;
            }

            if (group == null)
                return;

            var visible = Time.unscaledTime < hideAt;
            group.alpha = visible ? 1f : 0f;
        }

        void OnDestroy()
        {
            if (hooked && AchievementManager.Instance != null)
                AchievementManager.Instance.Unlocked -= Show;
        }

        void Show(AchievementData data)
        {
            if (title != null)
                title.text = "Открыто";
            if (body != null)
                body.text = data.displayName + "\n" + data.description;
            hideAt = Time.unscaledTime + 3.4f;
        }

        static Text Label(Transform parent, string name, string value, int fontSize, Vector2 position, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(-32f, 36f);
            var text = textObject.GetComponent<Text>();
            text.font = UIFont.DefaultFont;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = color;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }

        static Sprite white;

        static Sprite White()
        {
            if (white != null)
                return white;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            white = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return white;
        }
    }
}
