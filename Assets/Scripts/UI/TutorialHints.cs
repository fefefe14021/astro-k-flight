// Assets/Scripts/UI/TutorialHints.cs
// Namespace: KANYE.UI
// GameObject: TutorialHints

using System.Collections;
using KANYE.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        private bool moved = false;
        private bool shot = false;
        private bool targetSpawned = false;
        private bool targetDestroyed = false;

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

            // Check if player already completed tutorial
            if (IsTutorialCompleted)
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

            // Spawn the single training asteroid
            SpawnTrainingAsteroid();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
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

            yield return new WaitForSeconds(2.2f);

            // Smooth fade out
            var t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                yield return null;
            }

            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
            IsTutorialActive = false;
            gameObject.SetActive(false);
            Debug.Log("[FTUE] Обучение завершено. Обычный бой активирован.");
        }

        private void SpawnTrainingAsteroid()
        {
            if (targetSpawned) return;
            targetSpawned = true;

            var existing = FindObjectsByType<Asteroid>();
            if (existing != null && existing.Length > 0)
            {
                existing[0].Configure(1.2f, 1, 0.65f);
                existing[0].transform.position = new Vector3(8.5f, 0f, 0f);
                return;
            }

            var asteroidObj = new GameObject("TutorialAsteroid");
            asteroidObj.transform.position = new Vector3(8.5f, 0f, 0f);
            asteroidObj.AddComponent<SpriteRenderer>();
            var asteroid = asteroidObj.AddComponent<Asteroid>();
            asteroid.Configure(1.2f, 1, 0.65f);
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
            panelRect.anchoredPosition = new Vector2(0f, -60f);
            panelRect.sizeDelta = new Vector2(720f, 155f);

            var bg = panelObj.GetComponent<Image>();
            bg.sprite = WhiteSprite();
            bg.color = new Color(0.04f, 0.06f, 0.12f, 0.94f);

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accent.transform.SetParent(panelObj.transform, false);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(1f, 0f);
            accentRect.pivot = new Vector2(0.5f, 0f);
            accentRect.sizeDelta = new Vector2(0f, 3f);
            accent.GetComponent<Image>().sprite = WhiteSprite();
            accent.GetComponent<Image>().color = new Color(1f, 0.75f, 0.2f, 1f);

            titleText = CreateLabel(panelObj.transform, "Title", "БОРТОВОЕ ОБУЧЕНИЕ ПИЛОТА", 22, new Vector2(0f, -14f), new Color(1f, 0.85f, 0.4f));
            moveText = CreateLabel(panelObj.transform, "Move", "WASD / Стрелки / Стик — Маневрирование", 19, new Vector2(0f, -44f), Color.white);
            fireText = CreateLabel(panelObj.transform, "Fire", "ПРОБЕЛ / Кнопка — Огонь по курсу", 19, new Vector2(0f, -70f), Color.white);
            statusText = CreateLabel(panelObj.transform, "Status", "Опробуй управление и сбей первый астероид", 19, new Vector2(0f, -108f), new Color(0.75f, 0.88f, 1f));
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
            return label;
        }

        private static Sprite whiteSprite;
        private static Sprite WhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return whiteSprite;
        }
    }
}
