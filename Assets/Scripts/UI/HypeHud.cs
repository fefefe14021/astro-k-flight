using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KANYE.Gameplay;

namespace KANYE.UI
{
    public class HypeHud : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 0.35f;

        private HypeState state;
        private ShipHealth health;
        private Image fill;
        private Text albumLabel;
        private Text livesLabel;
        private float targetFill;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying || scene.name != "Gameplay")
                return;

            if (Object.FindAnyObjectByType<HypeHud>() != null)
                return;

            new GameObject("HypeHud").AddComponent<HypeHud>();
        }

        private void Start()
        {
            state = Object.FindAnyObjectByType<HypeState>();
            health = Object.FindAnyObjectByType<ShipHealth>();
            Build();
            if (state != null)
                state.Changed += Refresh;
            if (health != null)
                health.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (state != null)
                state.Changed -= Refresh;
            if (health != null)
                health.Changed -= Refresh;
        }

        private void Update()
        {
            if (fill == null)
                return;

            var step = fillSpeed * Time.unscaledDeltaTime;
            fill.fillAmount = targetFill >= 1f
                ? 1f
                : Mathf.MoveTowards(fill.fillAmount, targetFill, step);
        }

        private void Refresh()
        {
            if (state != null && albumLabel != null)
            {
                targetFill = state.HypeMax <= 0f ? 0f : state.Hype / state.HypeMax;
                albumLabel.text = $"Album {state.Albums}";
            }

            if (livesLabel == null || health == null)
                return;

            livesLabel.text = health.IsDefeated ? "Сбит" : $"Lives {health.Lives}";
        }

        private void Build()
        {
            var canvasObject = new GameObject("HypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var bar = CreateImage("HypeBar", canvasObject.transform, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(420f, 28f), new Color(0.08f, 0.08f, 0.1f, 0.85f));
            bar.type = Image.Type.Simple;

            var fillRect = CreateImage("Fill", bar.transform, new Vector2(0f, 0f), Vector2.zero, Vector2.zero, new Color(1f, 0.72f, 0.15f, 1f));
            fillRect.rectTransform.anchorMin = Vector2.zero;
            fillRect.rectTransform.anchorMax = Vector2.one;
            fillRect.rectTransform.offsetMin = new Vector2(4f, 4f);
            fillRect.rectTransform.offsetMax = new Vector2(-4f, -4f);
            fillRect.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            fill = fillRect;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            albumLabel = CreateText("AlbumLabel", canvasObject.transform, new Vector2(0.5f, 1f), new Vector2(0f, -28f), 28);
            albumLabel.text = "Album 0";

            livesLabel = CreateText("LivesLabel", canvasObject.transform, new Vector2(0f, 1f), new Vector2(150f, -36f), 28);
            livesLabel.alignment = TextAnchor.MiddleLeft;
            livesLabel.text = "Lives 3";

            var caption = CreateText("HypeCaption", canvasObject.transform, new Vector2(0.5f, 1f), new Vector2(-250f, -70f), 22);
            caption.alignment = TextAnchor.MiddleRight;
            caption.text = "Hype";
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = imageObject.GetComponent<Image>();
            image.sprite = WhiteSprite();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 position, int fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(240f, 40f);

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite whiteSprite;

        private static Sprite WhiteSprite()
        {
            if (whiteSprite != null)
                return whiteSprite;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return whiteSprite;
        }
    }
}
