// Assets/Scripts/UI/TutorialHints.cs
// Namespace: KANYE.UI
// GameObject: TutorialHints
// First-time pilot orientation with safe practice target dummy and skip support.

using System.Collections;
using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
#endif

namespace KANYE.UI
{
    public class TutorialHints : MonoBehaviour
    {
        public const string TutorialKey = "TutorialCompleted";
        public static bool IsTutorialActive { get; private set; }
        public static TutorialHints Instance { get; private set; }

        public static bool IsTutorialCompleted => PlayerPrefs.GetInt(TutorialKey, 0) == 1;
        public static bool PracticeAllowsEX => !IsTutorialActive || (Instance != null && Instance.step == Lesson.Ex);
        public static bool PracticeAllowsSuper => !IsTutorialActive || (Instance != null && Instance.step == Lesson.Super);

        public static void SetCoveredByPause(bool covered)
        {
            if (Instance == null || Instance.lessonCanvas == null)
                return;
            Instance.lessonCanvas.sortingOrder = covered ? 100 : 300;
        }

        private CanvasGroup canvasGroup;
        private Canvas lessonCanvas;
        private Text titleText;
        private Text moveText;
        private Text fireText;
        private Text statusText;
        private Button skipButton;

        private enum Lesson
        {
            Move,
            Shoot,
            Ex,
            Super,
            Done
        }

        private bool moved;
        private bool shot;
        private Lesson step = Lesson.Move;
        private HypeState hype;
        private Asteroid activeTrainingAsteroid;
        private float nextTargetAt;

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

            // Keep tutorial HUD hidden while Barney is speaking via radio comms
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            StartCoroutine(WaitForCommsAndStartTutorialRoutine());
        }

        private IEnumerator WaitForCommsAndStartTutorialRoutine()
        {
            // Wait until Barney's radio transmission finishes
            while (MissionRadioComms.IsCommsActive)
                yield return null;

            // Small settle pause after comms close
            yield return new WaitForSecondsRealtime(0.2f);

            var player = GameObject.Find("Player");
            var input = player != null ? player.GetComponent<ShipInput>() : null;
            if (input != null)
            {
                input.OnFirstMove += HandleMove;
                input.OnFirstShot += HandleShot;
            }

            // Smooth fade-in of tutorial HUD
            var t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Clamp01(t / 0.35f);
                yield return null;
            }
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            hype = FindAnyObjectByType<HypeState>();
            if (hype != null)
            {
                hype.EXFired += HandleEx;
                hype.SuperFired += HandleSuper;
            }

            GameRules.TutorialArsenal = true;
            ShowMoveStep();
        }

        private void OnDestroy()
        {
            var player = GameObject.Find("Player");
            var input = player != null ? player.GetComponent<ShipInput>() : null;
            if (input != null)
            {
                input.OnFirstMove -= HandleMove;
                input.OnFirstShot -= HandleShot;
            }

            if (hype != null)
            {
                hype.EXFired -= HandleEx;
                hype.SuperFired -= HandleSuper;
            }

            GameRules.TutorialArsenal = false;

            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!IsTutorialActive || step == Lesson.Done || step == Lesson.Move)
                return;

            if (step == Lesson.Ex)
                EnsureHype(HypeState.EXCost);
            else if (step == Lesson.Super)
                EnsureHype(100f);

            if (activeTrainingAsteroid == null && Time.unscaledTime >= nextTargetAt)
                SpawnTrainingAsteroidInstance();
        }

        public void OnFirstMove() => HandleMove();
        public void OnFirstShot() => HandleShot();

        private void ShowMoveStep()
        {
            step = Lesson.Move;
            if (titleText != null)
                titleText.text = Localization.T("ШАГ 1 ИЗ 4  ·  МАНЁВР", "STEP 1 OF 4  ·  MANEUVER");
            if (moveText != null)
                moveText.text = Localization.T("Сдвинь тарелку клавишами WASD или стрелками.", "Move your saucer using WASD or arrow keys.");
            if (fireText != null)
                fireText.text = "";
            if (statusText != null)
            {
                statusText.text = Localization.T("Пока корабль не сдвинется, учебная цель не появится.", "Training target will not appear until the ship moves.");
                statusText.color = new Color(0.75f, 0.88f, 1f);
            }
        }

        private void ShowShootStep()
        {
            step = Lesson.Shoot;
            if (titleText != null)
                titleText.text = Localization.T("ШАГ 2 ИЗ 4  ·  ОГОНЬ", "STEP 2 OF 4  ·  FIRE");
            if (moveText != null)
            {
                moveText.text = Localization.T("✓ Манёвр засчитан", "✓ Maneuver complete");
                moveText.color = new Color(0.45f, 1f, 0.55f);
            }
            if (fireText != null)
                fireText.text = Localization.T("Зажми ПРОБЕЛ и сбей астероид справа.", "Hold SPACE to shoot down the asteroid on the right.");
            if (statusText != null)
            {
                statusText.text = Localization.T("Сбитые цели копят шкалу Ярости. Задание миссии — пройденный путь, не альбомы.", "Destroying targets builds Hype. Mission goal is distance travelled, not albums.");
                statusText.color = new Color(1f, 0.88f, 0.45f);
            }
            SpawnTrainingAsteroidInstance();
        }

        private void HandleMove()
        {
            if (moved) return;
            moved = true;
            ShowShootStep();
        }

        private void HandleShot()
        {
            shot = true;
        }

        private void HandleEx()
        {
            if (step != Lesson.Ex)
                return;
            ShowSuperStep();
        }

        private void HandleSuper()
        {
            if (step != Lesson.Super)
                return;
            step = Lesson.Done;
            StartCoroutine(FinishTutorialRoutine());
        }

        private void ShowExStep()
        {
            step = Lesson.Ex;
            GameRules.TutorialArsenal = true;
            EnsureHype(HypeState.EXCost);
            if (titleText != null)
                titleText.text = Localization.T("ШАГ 3 ИЗ 4  ·  РАКЕТА", "STEP 3 OF 4  ·  EX ROCKET");
            if (moveText != null)
                moveText.text = Localization.T("✓ Манёвр    ✓ Выстрел", "✓ Maneuver    ✓ Fire");
            if (fireText != null)
                fireText.text = Localization.T("Нажми Q — EX-ракета.", "Press Q — EX-Rocket.");
            if (statusText != null)
            {
                statusText.text = Localization.T("Карта Ярости уже есть. Если ракета уйдёт мимо, карта вернётся.", "Energy card ready. If rocket misses, card will be refunded.");
                statusText.color = new Color(0.45f, 0.9f, 1f);
            }
            SpawnTrainingAsteroidInstance();
        }

        private void ShowSuperStep()
        {
            step = Lesson.Super;
            EnsureHype(100f);
            if (titleText != null)
                titleText.text = Localization.T("ШАГ 4 ИЗ 4  ·  МЕГА-ЛУЧ", "STEP 4 OF 4  ·  MEGA-LASER");
            if (moveText != null)
                moveText.text = Localization.T("✓ Манёвр    ✓ Выстрел    ✓ Ракета Q", "✓ Maneuver    ✓ Fire    ✓ Rocket Q");
            if (fireText != null)
                fireText.text = Localization.T("Нажми E — мега-луч.", "Press E — Mega-Laser.");
            if (statusText != null)
            {
                statusText.text = Localization.T("Шкала Ярости полная. Если луч уйдёт мимо, шкала вернётся.", "Hype meter is full. If laser misses, meter will be refunded.");
                statusText.color = new Color(1f, 0.88f, 0.45f);
            }
            SpawnTrainingAsteroidInstance();
        }

        private void EnsureHype(float target)
        {
            if (hype == null)
                hype = FindAnyObjectByType<HypeState>();
            if (hype == null || hype.Hype >= target)
                return;
            hype.AddHype(target - hype.Hype);
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
            if (!IsTutorialActive || step == Lesson.Done || step == Lesson.Move) return;

            if (statusText != null)
            {
                statusText.text = Localization.T("Это учебная цель без урона. Отойди и сбей её пробелом.", "Training dummy inflicts no damage. Move back and blast it with space.");
                statusText.color = new Color(1f, 0.45f, 0.45f);
            }

            StartCoroutine(RespawnTrainingAsteroidRoutine(1.2f));
        }

        private void HandleTargetMissed()
        {
            if (!IsTutorialActive || step == Lesson.Done || step == Lesson.Move) return;

            if (statusText != null)
            {
                statusText.text = Localization.T("Астероид ушёл мимо. Поднимись на его высоту и снова стреляй.", "Asteroid passed by. Realign your altitude and fire again.");
                statusText.color = new Color(1f, 0.75f, 0.3f);
            }

            StartCoroutine(RespawnTrainingAsteroidRoutine(1.0f));
        }

        private IEnumerator RespawnTrainingAsteroidRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (IsTutorialActive && step != Lesson.Done && step != Lesson.Move)
                SpawnTrainingAsteroidInstance();
        }

        private void OnTargetHit()
        {
            if (step == Lesson.Shoot)
            {
                ShowExStep();
                return;
            }

            if (step == Lesson.Ex || step == Lesson.Super)
                StartCoroutine(RespawnTrainingAsteroidRoutine(0.6f));
        }

        private IEnumerator FinishTutorialRoutine()
        {
            if (titleText != null)
                titleText.text = Localization.T("УРОК ЗАКРЫТ", "TUTORIAL COMPLETE");
            if (moveText != null)
                moveText.text = Localization.T("✓ Манёвр    ✓ Выстрел    ✓ Q    ✓ E", "✓ Maneuver    ✓ Fire    ✓ Q    ✓ E");
            if (fireText != null)
                fireText.text = Localization.T("Обе кнопки проверены.", "All systems verified.");
            if (statusText != null)
            {
                statusText.text = Localization.T("Дальше идёт обычная волна первой миссии.", "Entering standard mission wave.");
                statusText.color = new Color(0.3f, 1f, 0.7f);
            }
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
            GameRules.TutorialArsenal = false;

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
            if (!IsTutorialActive || step == Lesson.Done || step == Lesson.Move) return;

            if (activeTrainingAsteroid != null)
                Destroy(activeTrainingAsteroid.gameObject);

            var asteroidObj = new GameObject("TutorialAsteroid", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            asteroidObj.transform.position = new Vector3(8.5f, 0f, 0f);
            activeTrainingAsteroid = asteroidObj.AddComponent<Asteroid>();
            // Gentle speed (0.85f), 0 contact damage so beginner never loses HP on target dummy!
            activeTrainingAsteroid.Configure(0.85f, 0, 0.65f);
            nextTargetAt = Time.unscaledTime + 0.45f;
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("TutorialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            lessonCanvas = canvasObj.GetComponent<Canvas>();
            lessonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            lessonCanvas.sortingOrder = 300;

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
            panelRect.anchoredPosition = new Vector2(0f, -118f);
            panelRect.sizeDelta = new Vector2(860f, 196f);

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

            titleText = CreateLabel(panelObj.transform, "Title", "ШАГ 1 ИЗ 4  ·  МАНЁВР", 22, new Vector2(16f, -16f), 34f, new Color(1f, 0.85f, 0.4f));
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.rectTransform.offsetMax = new Vector2(-200f, titleText.rectTransform.offsetMax.y);
            moveText = CreateLabel(panelObj.transform, "Move", "Сдвинь тарелку клавишами WASD или стрелками.", 20, new Vector2(0f, -62f), 32f, Color.white);
            fireText = CreateLabel(panelObj.transform, "Fire", "", 20, new Vector2(0f, -102f), 32f, Color.white);
            statusText = CreateLabel(panelObj.transform, "Status", "Пока корабль не сдвинется, учебная цель не появится.", 16, new Vector2(0f, -142f), 32f, new Color(0.75f, 0.88f, 1f));

            // Skip Tutorial button
            var skipObj = new GameObject("SkipButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            skipObj.transform.SetParent(panelObj.transform, false);
            var skipRect = skipObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1f, 1f);
            skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-14f, -16f);
            skipRect.sizeDelta = new Vector2(176f, 32f);

            var skipImg = skipObj.GetComponent<Image>();
            skipImg.sprite = PixelUiAtlas.ButtonSecondary;
            skipImg.type = Image.Type.Sliced;
            skipImg.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            skipButton = skipObj.GetComponent<Button>();
            skipObj.SetActive(false);

            var skipLabel = CreateLabel(skipObj.transform, "SkipText", Localization.T("Пропустить [TAB]", "Skip [TAB]"), 14, Vector2.zero, 24f, Color.white);
            skipLabel.rectTransform.anchorMin = Vector2.zero;
            skipLabel.rectTransform.anchorMax = Vector2.one;
            skipLabel.rectTransform.sizeDelta = Vector2.zero;
            skipLabel.alignment = TextAnchor.MiddleCenter;
        }

        private static Text CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 pos, float height, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(-32f, height);

            var label = go.GetComponent<Text>();
            label.font = GameFont.Get();
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
