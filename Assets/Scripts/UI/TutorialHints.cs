// Assets/Scripts/UI/TutorialHints.cs
// Namespace: KANYE.UI
// GameObject: TutorialHints
// First-time pilot orientation with safe practice target dummy and skip support.

using System.Collections;
using KANYE.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KANYE.UI
{
    public class TutorialHints : MonoBehaviour
    {
        public const string TutorialKey = "TutorialCompleted";
        public static bool IsTutorialActive { get; private set; }
        public static TutorialHints Instance { get; private set; }

        public static bool IsTutorialCompleted => PlayerPrefs.GetInt(TutorialKey, 0) == 1;

        private CanvasGroup canvasGroup;
        private Text titleText;
        private Text moveText;
        private Text fireText;
        private Text statusText;
        private Button skipButton;

        private bool moved;
        private bool shot;
        private bool targetDestroyed;
        private int collisionCount;
        private int missCount;
        private Asteroid activeTrainingAsteroid;

        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TutorialKey);
            PlayerPrefs.DeleteKey("TutorialShown");
            PlayerPrefs.Save();
            IsTutorialActive = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (SceneManager.GetActiveScene().name != "Gameplay")
                return;

            // Tutorial ONLY runs in Mission 0 (first sector) and only if not completed yet
            if (MissionCatalog.Selected != 0 || IsTutorialCompleted)
            {
                IsTutorialActive = false;
                return;
            }

            IsTutorialActive = true;
            if (FindAnyObjectByType<TutorialHints>() == null)
            {
                new GameObject("TutorialHints").AddComponent<TutorialHints>();
            }
        }

        private void Awake()
        {
            Instance = this;
            BuildUI();
        }

        private void Start()
        {
            if (!IsTutorialActive)
            {
                gameObject.SetActive(false);
                return;
            }

            var player = GameObject.Find("Player");
            var input = player != null ? player.GetComponent<ShipInput>() : null;
            if (input != null)
            {
                input.OnFirstMove += HandleMove;
                input.OnFirstShot += HandleShot;
            }

            SpawnTrainingAsteroidInstance();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!IsTutorialActive || targetDestroyed)
                return;

            var skipPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                    skipPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
                skipPressed = true;
#endif
            if (skipPressed)
            {
                CompleteTutorial();
            }
        }

        public void OnFirstMove() => HandleMove();
        public void OnFirstShot() => HandleShot();

        private void HandleMove()
        {
            if (moved) return;
            moved = true;
            if (moveText != null)
            {
                moveText.text = "✓ Маневрирование освоено";
                moveText.color = new Color(0.4f, 1f, 0.5f);
            }
            CheckControlsPhase();
        }

        private void HandleShot()
        {
            if (shot) return;
            shot = true;
            if (fireText != null)
            {
                fireText.text = "✓ Огонь освоен";
                fireText.color = new Color(0.4f, 1f, 0.5f);
            }
            CheckControlsPhase();
        }

        private void CheckControlsPhase()
        {
            if (moved && shot && !targetDestroyed)
            {
                if (statusText != null)
                {
                    statusText.text = "Сбей учебный астероид прямо по курсу!";
                    statusText.color = new Color(1f, 0.85f, 0.3f);
                }
            }
        }

        public static void NotifyAsteroidDestroyed()
        {
            if (Instance != null && IsTutorialActive)
            {
                Instance.OnTargetHit();
            }
        }

        public static void NotifyAsteroidCollided()
        {
            if (Instance != null && IsTutorialActive)
            {
                Instance.HandleTargetCollision();
            }
        }

        public static void NotifyAsteroidMissed()
        {
            if (Instance != null && IsTutorialActive)
            {
                Instance.HandleTargetMissed();
            }
        }

        private void HandleTargetCollision()
        {
            if (targetDestroyed || !IsTutorialActive) return;

            collisionCount++;
            if (collisionCount >= 2)
            {
                // After 2 impacts, auto-complete tutorial so player is NEVER blocked
                Debug.Log("[Tutorial] Авто-завершение обучения после серии столкновений.");
                CompleteTutorial();
                return;
            }

            if (statusText != null)
            {
                statusText.text = "Внимание: столкновение! Стреляй на [ПРОБЕЛ] или [ЛКМ]!";
                statusText.color = new Color(1f, 0.45f, 0.45f);
            }

            StartCoroutine(RespawnTrainingAsteroidRoutine(1.2f));
        }

        private void HandleTargetMissed()
        {
            if (targetDestroyed || !IsTutorialActive) return;

            missCount++;
            if (missCount >= 2)
            {
                Debug.Log("[Tutorial] Авто-завершение обучения после пролета цели.");
                CompleteTutorial();
                return;
            }

            if (statusText != null)
            {
                statusText.text = "Астероид ушел! Целься и стреляй на [ПРОБЕЛ] или [ЛКМ]!";
                statusText.color = new Color(1f, 0.75f, 0.3f);
            }

            StartCoroutine(RespawnTrainingAsteroidRoutine(1.0f));
        }

        private IEnumerator RespawnTrainingAsteroidRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!targetDestroyed && IsTutorialActive)
            {
                SpawnTrainingAsteroidInstance();
            }
        }

        private void OnTargetHit()
        {
            if (targetDestroyed) return;
            targetDestroyed = true;

            StartCoroutine(FinishTutorialRoutine());
        }

        private IEnumerator FinishTutorialRoutine()
        {
            if (titleText != null)
                titleText.text = "ОТЛИЧНО! ЦЕЛЬ УНИЧТОЖЕНА";
            if (statusText != null)
            {
                statusText.text = "+1 Album. Hype заряжается!\nВнимание: приближается основной рой!";
                statusText.color = new Color(0.3f, 1f, 0.7f);
            }
            if (moveText != null) moveText.gameObject.SetActive(false);
            if (fireText != null) fireText.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);

            yield return new WaitForSeconds(1.8f);

            // Smooth fade out
            var t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.45f);
                yield return null;
            }

            CompleteTutorial();
        }

        public void CompleteTutorial()
        {
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
            IsTutorialActive = false;

            // Clean up any remaining training asteroids
            var asteroids = FindObjectsByType<Asteroid>();
            for (var i = 0; i < asteroids.Length; i++)
            {
                if (asteroids[i] == activeTrainingAsteroid || asteroids[i].name.Contains("Tutorial"))
                {
                    Destroy(asteroids[i].gameObject);
                }
            }

            gameObject.SetActive(false);
            Debug.Log("[FTUE] Обучение успешно завершено. Полноценная миссия запущена.");
        }

        private void SpawnTrainingAsteroidInstance()
        {
            if (targetDestroyed || !IsTutorialActive) return;

            // Clear any stale existing asteroid
            var existing = FindObjectsByType<Asteroid>();
            for (var i = 0; i < existing.Length; i++)
            {
                Destroy(existing[i].gameObject);
            }

            var asteroidObj = new GameObject("TutorialAsteroid", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            asteroidObj.transform.position = new Vector3(8.5f, 0f, 0f);
            activeTrainingAsteroid = asteroidObj.AddComponent<Asteroid>();
            // Gentle speed (0.85f), 0 contact damage so beginner never loses HP on target dummy!
            activeTrainingAsteroid.Configure(0.85f, 0, 0.65f);
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("TutorialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panelObj = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObj.transform.SetParent(canvasObj.transform, false);
            canvasGroup = panelObj.GetComponent<CanvasGroup>();

            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -50f);
            panelRect.sizeDelta = new Vector2(760f, 160f);

            var bg = panelObj.GetComponent<Image>();
            bg.sprite = PixelUiAtlas.Panel;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.04f, 0.07f, 0.14f, 0.95f);

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accent.transform.SetParent(panelObj.transform, false);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(1f, 0f);
            accentRect.pivot = new Vector2(0.5f, 0f);
            accentRect.sizeDelta = new Vector2(0f, 3f);
            accent.GetComponent<Image>().sprite = PixelUiAtlas.White;
            accent.GetComponent<Image>().color = new Color(1f, 0.75f, 0.2f, 1f);

            titleText = CreateLabel(panelObj.transform, "Title", "БОРТОВОЕ ОБУЧЕНИЕ ПИЛОТА", 22, new Vector2(0f, -14f), new Color(1f, 0.85f, 0.4f));
            moveText = CreateLabel(panelObj.transform, "Move", "WASD / Стрелки / Стик — Маневрирование", 19, new Vector2(0f, -44f), Color.white);
            fireText = CreateLabel(panelObj.transform, "Fire", "ПРОБЕЛ / Кнопка — Огонь по курсу", 19, new Vector2(0f, -70f), Color.white);
            statusText = CreateLabel(panelObj.transform, "Status", "Опробуй управление и сбей первый астероид", 19, new Vector2(0f, -108f), new Color(0.75f, 0.88f, 1f));

            // Skip Tutorial button
            var skipObj = new GameObject("SkipButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            skipObj.transform.SetParent(panelObj.transform, false);
            var skipRect = skipObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1f, 1f);
            skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-12f, -12f);
            skipRect.sizeDelta = new Vector2(160f, 32f);

            var skipImg = skipObj.GetComponent<Image>();
            skipImg.sprite = PixelUiAtlas.ButtonSecondary;
            skipImg.type = Image.Type.Sliced;
            skipImg.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            skipButton = skipObj.GetComponent<Button>();
            skipButton.onClick.AddListener(CompleteTutorial);

            var skipLabel = CreateLabel(skipObj.transform, "SkipText", "Пропустить [TAB]", 14, Vector2.zero, Color.white);
            skipLabel.rectTransform.anchorMin = Vector2.zero;
            skipLabel.rectTransform.anchorMax = Vector2.one;
            skipLabel.rectTransform.sizeDelta = Vector2.zero;
            skipLabel.alignment = TextAnchor.MiddleCenter;
        }

        private static Text CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 pos, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(-20f, 30f);

            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
    }
}
