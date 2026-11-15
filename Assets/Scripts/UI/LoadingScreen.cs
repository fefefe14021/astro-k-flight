// Assets/Scripts/UI/LoadingScreen.cs
// Namespace: KANYE.UI
// GameObject: LoadingScreen in Bootstrap scene

using System.Collections;
using KANYE.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Image progressBarFill;
        private Text percentText;
        private Text statusText;
        private Text tipText;
        private Transform spinnerTransform;

        private static readonly string[] BootLogs =
        {
            "Инициализация бортового квантового ядра...",
            "Проверка целостности обшивки и щитов...",
            "Синхронизация навигационных карт секторов Neon City...",
            "Калибровка плазменных ускорителей и Hype-модуля...",
            "Загрузка конфигураций оружейных систем...",
            "Связь с орбитальным маяком установлена. Готовность 100%!"
        };

        private static readonly string[] Tips =
        {
            "Совет: Заполняй шкалу Hype сбитыми целями и активируй мощный Супервыстрел клавишей [E].",
            "Совет: Вражеские дроны телеграфируют атаку вспышкой за доли секунды — уклоняйся от лазеров!",
            "Совет: Падающие ракеты можно перехватывать встречным огнем орудий.",
            "Совет: В Ангаре доступны новые корпуса корабля — собирай монеты в каждом секторе!"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoHookBootstrap()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "Bootstrap")
                return;

            if (FindAnyObjectByType<LoadingScreen>() != null)
                return;

            new GameObject("LoadingScreen").AddComponent<LoadingScreen>();
        }

        private void Awake()
        {
            BuildUI();
        }

        private void Start()
        {
            StartCoroutine(LoadSequenceRoutine());
        }

        private void Update()
        {
            if (spinnerTransform != null)
            {
                spinnerTransform.Rotate(0f, 0f, -140f * Time.unscaledDeltaTime);
            }
        }

        private IEnumerator LoadSequenceRoutine()
        {
            var duration = 2.4f;
            var elapsed = 0f;

            var chosenTip = Tips[Random.Range(0, Tips.Length)];
            if (tipText != null)
                tipText.text = chosenTip;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);

                if (progressBarFill != null)
                    progressBarFill.fillAmount = progress;

                if (percentText != null)
                    percentText.text = $"{Mathf.RoundToInt(progress * 100f)}%";

                var logIndex = Mathf.Clamp(Mathf.FloorToInt(progress * BootLogs.Length), 0, BootLogs.Length - 1);
                if (statusText != null)
                    statusText.text = BootLogs[logIndex];

                yield return null;
            }

            if (progressBarFill != null)
                progressBarFill.fillAmount = 1f;
            if (percentText != null)
                percentText.text = "100%";
            if (statusText != null)
                statusText.text = BootLogs[BootLogs.Length - 1];

            yield return new WaitForSecondsRealtime(0.4f);

            // Smooth fade out
            var fadeTime = 0.45f;
            var t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }

            SceneLoader.LoadSceneByName(SceneLoader.MainMenuScene);
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var rootObj = new GameObject("Root", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            rootObj.transform.SetParent(canvasObj.transform, false);
            canvasGroup = rootObj.GetComponent<CanvasGroup>();
            Stretch(rootObj.GetComponent<RectTransform>());

            // Background pixel art
            var bg = rootObj.GetComponent<Image>();
            bg.sprite = PixelUiAtlas.Background;
            bg.color = Color.white;

            // Dark cyber overlay
            var overlay = CreateImage(rootObj.transform, "DimOverlay", Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.03f, 0.05f, 0.09f, 0.65f));
            Stretch(overlay.rectTransform);

            // Cyber star particles
            ScatterStars(rootObj.transform);

            // Clean 1px cyber boundary lines (no raw yellow boxes)
            CreateCyberBorder(rootObj.transform);

            // Arcade Pixel Art Logo
            var logoSprite = PixelUiAtlas.Logo;
            var logoObj = new GameObject("Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logoObj.transform.SetParent(rootObj.transform, false);
            var logoRect = logoObj.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = new Vector2(0f, 150f);
            logoRect.sizeDelta = new Vector2(560f, 150f);

            var logoImg = logoObj.GetComponent<Image>();
            logoImg.sprite = logoSprite != null ? logoSprite : PixelUiAtlas.White;
            logoImg.preserveAspect = true;

            if (logoSprite == null)
            {
                CreateText(logoObj.transform, "Title", "ASTRO K", new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(700f, 60f), 56, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            }

            CreateText(rootObj.transform, "Subtitle", "FLIGHT HOME // НОЧНОЙ РЕЙС", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(800f, 32f), 20, new Color(0.35f, 0.85f, 1f), FontStyle.Normal);

            // Radar spinner
            var spinnerObj = new GameObject("Spinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spinnerObj.transform.SetParent(rootObj.transform, false);
            var spinRect = spinnerObj.GetComponent<RectTransform>();
            spinRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinRect.pivot = new Vector2(0.5f, 0.5f);
            spinRect.anchoredPosition = new Vector2(0f, 0f);
            spinRect.sizeDelta = new Vector2(54f, 54f);
            spinnerTransform = spinRect;

            var spinImg = spinnerObj.GetComponent<Image>();
            spinImg.sprite = CreateRadarSprite();
            spinImg.color = new Color(0.2f, 0.85f, 1f, 0.9f);

            // Progress bar container (Sliced cyber subpanel - zero distortion)
            var barBg = CreateImage(rootObj.transform, "ProgressBarBg", new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(640f, 22f), Color.white);
            barBg.sprite = PixelUiAtlas.SubPanel;
            barBg.type = Image.Type.Sliced;

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(barBg.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>());

            var fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            progressBarFill = fillObj.GetComponent<Image>();
            progressBarFill.sprite = PixelUiAtlas.White;
            progressBarFill.color = new Color(0.18f, 0.85f, 1f, 1f);
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressBarFill.fillAmount = 0f;

            percentText = CreateText(rootObj.transform, "Percent", "0%", new Vector2(0.5f, 0.5f), new Vector2(0f, -116f), new Vector2(200f, 30f), 20, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);

            statusText = CreateText(rootObj.transform, "Status", "Инициализация...", new Vector2(0.5f, 0.5f), new Vector2(0f, -152f), new Vector2(800f, 30f), 18, new Color(0.75f, 0.85f, 0.95f), FontStyle.Normal);

            tipText = CreateText(rootObj.transform, "Tip", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(900f, 44f), 17, new Color(0.6f, 0.72f, 0.85f), FontStyle.Italic);
        }

        private static void CreateCyberBorder(Transform parent)
        {
            var borderColor = new Color(0.2f, 0.75f, 1f, 0.35f);
            // Top hairline
            CreateImage(parent, "BorderTop", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1840f, 1f), borderColor);
            // Bottom hairline
            CreateImage(parent, "BorderBottom", new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1840f, 1f), borderColor);
        }

        private static void ScatterStars(Transform parent)
        {
            var random = new System.Random(42);
            for (var i = 0; i < 40; i++)
            {
                var star = CreateImage(parent, "Star" + i, new Vector2((float)random.NextDouble(), (float)random.NextDouble()), Vector2.zero, Vector2.one * random.Next(2, 4), new Color(1f, 1f, 1f, random.Next(25, 80) / 100f));
                star.rectTransform.anchorMax = star.rectTransform.anchorMin;
            }
        }

        private static Image CreateImage(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var img = obj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.White;
            img.color = color;
            return img;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, Color color, FontStyle fontStyle)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var label = obj.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Sprite CreateRadarSprite()
        {
            var size = 32;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            var center = size * 0.5f;
            var radius = size * 0.45f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (Mathf.Abs(d - radius) < 1.4f || (d < radius && (Mathf.Abs(dx) < 1f || Mathf.Abs(dy) < 1f)))
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
