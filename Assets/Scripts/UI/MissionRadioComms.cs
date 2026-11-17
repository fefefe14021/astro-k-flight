// Assets/Scripts/UI/MissionRadioComms.cs
// Namespace: KANYE.UI
// In-mission radio comms with Navigator Barney.
// Reveals rich story, sector lore, boss tactics, and comic relief through dynamic typewriter transmissions.

using System;
using System.Collections;
using KANYE.Audio;
using KANYE.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KANYE.UI
{
    public class MissionRadioComms : MonoBehaviour
    {
        public static MissionRadioComms Instance { get; private set; }
        public static bool IsCommsActive { get; private set; }

        private GameObject canvasObj;
        private GameObject dimOverlayObj;
        private GameObject consoleRoot;
        private Image portraitImage;
        private Text channelTagText;
        private Text messageText;
        private Text onAirText;
        private Text promptText;
        private Button advanceButton;

        private Coroutine activeTransmission;
        private string[] queuedLines;
        private int currentLineIndex;
        private bool isTyping;
        private string fullCurrentLine = string.Empty;
        private Action onCompleteCallback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying || scene.name != "Gameplay")
                return;

            if (FindAnyObjectByType<MissionRadioComms>() != null)
                return;

            new GameObject("MissionRadioComms").AddComponent<MissionRadioComms>();
        }

        private void Awake()
        {
            Instance = this;
            IsCommsActive = true;
            Time.timeScale = 0f;
            BuildUI();
            HideConsole();
        }

        private void Start()
        {
            // Trigger sector intro dialogue after a brief scenic moment
            StartCoroutine(StartMissionIntroRoutine());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                IsCommsActive = false;
                Time.timeScale = 1f;
            }
        }

        private IEnumerator StartMissionIntroRoutine()
        {
            yield return null;

            var sector = MissionCatalog.Selected;
            var dialogue = LoreCatalog.GetDialogue(sector);

            if (TutorialHints.IsTutorialActive && !string.IsNullOrEmpty(dialogue.TutorialIntro))
            {
                // Play tutorial introduction from Barney
                StartTransmission(dialogue.Header, new[] { dialogue.TutorialIntro }, null);
            }
            else
            {
                // Play standard sector narrative introduction
                StartTransmission(dialogue.Header, dialogue.IntroLines, null);
            }
        }

        public static void ShowBossAlert(int sectorIndex, Action onDismiss)
        {
            if (Instance == null)
            {
                onDismiss?.Invoke();
                return;
            }

            var dialogue = LoreCatalog.GetDialogue(MissionCatalog.Selected);
            if (string.IsNullOrEmpty(dialogue?.BossWarning))
                dialogue = LoreCatalog.GetDialogue(sectorIndex);

            AudioManager.PlayBossWarning();
            var warning = !string.IsNullOrEmpty(dialogue?.BossWarning)
                ? dialogue.BossWarning
                : "⚠️ Внимание! На радаре обнаружен флагман Синдиката! Приготовься к бою!";

            Instance.StartTransmission("🚨 ПЕРЕХВАТ: БАРНИ НА СВЯЗИ!", new[] { warning }, onDismiss);
        }

        public void StartTransmission(string header, string[] lines, Action onComplete)
        {
            if (lines == null || lines.Length == 0)
            {
                IsCommsActive = false;
                Time.timeScale = 1f;
                onComplete?.Invoke();
                return;
            }

            if (activeTransmission != null)
                StopCoroutine(activeTransmission);

            queuedLines = lines;
            currentLineIndex = 0;
            onCompleteCallback = onComplete;
            IsCommsActive = true;
            Time.timeScale = 0f;

            if (channelTagText != null)
                channelTagText.text = header;

            AudioManager.PlayRadioOpen();
            ShowConsole();

            activeTransmission = StartCoroutine(TypeLineRoutine(queuedLines[0]));
        }

        private IEnumerator TypeLineRoutine(string line)
        {
            isTyping = true;
            fullCurrentLine = line;
            messageText.text = "";

            if (promptText != null)
                promptText.text = "[ ПРОДОЛЖИТЬ (ПРОБЕЛ) >> ]";

            var charDelay = 0.024f;
            var beepCounter = 0;

            for (var i = 0; i < line.Length; i++)
            {
                messageText.text += line[i];

                beepCounter++;
                if (beepCounter >= 3 && char.IsLetterOrDigit(line[i]))
                {
                    beepCounter = 0;
                    AudioManager.PlayRadioBeep();
                }

                // Slight talking bounce on portrait
                if (portraitImage != null)
                {
                    var bounce = Mathf.Sin(Time.unscaledTime * 22f) * 2f;
                    portraitImage.rectTransform.anchoredPosition = new Vector2(0f, bounce);
                }

                yield return new WaitForSecondsRealtime(charDelay);
            }

            isTyping = false;
            if (portraitImage != null)
                portraitImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void CompleteCurrentLineImmediately()
        {
            if (activeTransmission != null)
                StopCoroutine(activeTransmission);

            isTyping = false;
            messageText.text = fullCurrentLine;
            if (portraitImage != null)
                portraitImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void AdvanceDialogue()
        {
            if (!IsCommsActive)
                return;

            if (isTyping)
            {
                CompleteCurrentLineImmediately();
                return;
            }

            currentLineIndex++;
            if (queuedLines != null && currentLineIndex < queuedLines.Length)
            {
                activeTransmission = StartCoroutine(TypeLineRoutine(queuedLines[currentLineIndex]));
            }
            else
            {
                CloseTransmission();
            }
        }

        public void CloseTransmission()
        {
            if (activeTransmission != null)
                StopCoroutine(activeTransmission);

            IsCommsActive = false;
            Time.timeScale = 1f;
            HideConsole();

            var cb = onCompleteCallback;
            onCompleteCallback = null;
            cb?.Invoke();
        }

        public void SkipAll()
        {
            if (!IsCommsActive)
                return;

            CloseTransmission();
        }

        private void Update()
        {
            if (!IsCommsActive)
                return;

            // Animate on-air indicator pulse
            if (onAirText != null)
            {
                var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
                onAirText.color = Color.Lerp(new Color(0.2f, 1f, 0.4f), new Color(1f, 0.3f, 0.3f), pulse);
            }

            // Input listener to advance or skip dialogue
            var advancePressed = false;
            var skipAllPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.eKey.wasPressedThisFrame)
                {
                    advancePressed = true;
                }

                if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                    Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    skipAllPressed = true;
                }
            }
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                    Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    advancePressed = true;
                }
                if (Gamepad.current.startButton.wasPressedThisFrame ||
                    Gamepad.current.selectButton.wasPressedThisFrame)
                {
                    skipAllPressed = true;
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                advancePressed = true;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
                skipAllPressed = true;
#endif

            if (skipAllPressed)
                SkipAll();
            else if (advancePressed)
                AdvanceDialogue();
        }

        private void ShowConsole()
        {
            if (dimOverlayObj != null)
                dimOverlayObj.SetActive(true);
            if (consoleRoot != null)
                consoleRoot.SetActive(true);
        }

        private void HideConsole()
        {
            if (dimOverlayObj != null)
                dimOverlayObj.SetActive(false);
            if (consoleRoot != null)
                consoleRoot.SetActive(false);
        }

        private void BuildUI()
        {
            canvasObj = new GameObject("RadioCommsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 220; // Above HUD, below pause/defeat modals

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Fullscreen Dim Overlay
            dimOverlayObj = new GameObject("RadioDimOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            dimOverlayObj.transform.SetParent(canvasObj.transform, false);
            var dimRect = dimOverlayObj.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            var dimImg = dimOverlayObj.GetComponent<Image>();
            dimImg.color = new Color(0.015f, 0.025f, 0.06f, 0.65f); // Deep dark cinematic tint
            var dimBtn = dimOverlayObj.GetComponent<Button>();
            dimBtn.onClick.AddListener(AdvanceDialogue);

            consoleRoot = new GameObject("RadioConsole", typeof(RectTransform));
            consoleRoot.transform.SetParent(canvasObj.transform, false);
            var rootRect = consoleRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 40f);
            rootRect.sizeDelta = new Vector2(1040f, 190f);

            // Background Frame (Sliced panel)
            var bgObj = new GameObject("BgPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            bgObj.transform.SetParent(consoleRoot.transform, false);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = PixelUiAtlas.Panel;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.04f, 0.08f, 0.16f, 0.96f);

            // Tap/click anywhere to advance
            var bgBtn = bgObj.GetComponent<Button>();
            bgBtn.onClick.AddListener(AdvanceDialogue);

            // Holographic Top Accent Line
            var lineObj = new GameObject("TopGlowLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lineObj.transform.SetParent(consoleRoot.transform, false);
            var lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0f, 1f);
            lineRect.anchorMax = new Vector2(1f, 1f);
            lineRect.pivot = new Vector2(0.5f, 1f);
            lineRect.anchoredPosition = new Vector2(0f, -4f);
            lineRect.sizeDelta = new Vector2(-16f, 3f);
            var lineImg = lineObj.GetComponent<Image>();
            lineImg.sprite = PixelUiAtlas.White;
            lineImg.color = new Color(0.3f, 0.85f, 1f, 0.85f);

            // Portrait Frame (Left)
            var frameObj = new GameObject("PortraitFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameObj.transform.SetParent(consoleRoot.transform, false);
            var frameRect = frameObj.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0f, 0.5f);
            frameRect.anchorMax = new Vector2(0f, 0.5f);
            frameRect.pivot = new Vector2(0f, 0.5f);
            frameRect.anchoredPosition = new Vector2(25f, 0f);
            frameRect.sizeDelta = new Vector2(140f, 140f);
            var frameImg = frameObj.GetComponent<Image>();
            frameImg.sprite = PixelUiAtlas.SubPanel;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = new Color(0.08f, 0.16f, 0.28f, 0.95f);

            // Barney Portrait Sprite
            var portObj = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portObj.transform.SetParent(frameObj.transform, false);
            var portRect = portObj.GetComponent<RectTransform>();
            portRect.anchorMin = new Vector2(0.5f, 0.5f);
            portRect.anchorMax = new Vector2(0.5f, 0.5f);
            portRect.pivot = new Vector2(0.5f, 0.5f);
            portRect.anchoredPosition = Vector2.zero;
            portRect.sizeDelta = new Vector2(120f, 120f);
            portraitImage = portObj.GetComponent<Image>();
            portraitImage.preserveAspect = true;
            portraitImage.sprite = Resources.Load<Sprite>("Sprites/barney_portrait");

            // Header Channel Tag
            channelTagText = CreateLabel(consoleRoot.transform, "ChannelTag", "📡 СВЯЗЬ: ШТУРМАН БАРНИ", 18, new Vector2(180f, 64f), new Vector2(490f, 28f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            channelTagText.alignment = TextAnchor.MiddleLeft;
            channelTagText.horizontalOverflow = HorizontalWrapMode.Overflow;
            channelTagText.verticalOverflow = VerticalWrapMode.Truncate;

            // Live On-Air Indicator. Stops before the skip button, which sits at the right edge.
            onAirText = CreateLabel(consoleRoot.transform, "OnAir", "● В ЭФИРЕ", 15, new Vector2(684f, 64f), new Vector2(130f, 28f), new Color(0.2f, 1f, 0.4f), FontStyle.Bold);
            onAirText.alignment = TextAnchor.MiddleLeft;
            onAirText.horizontalOverflow = HorizontalWrapMode.Overflow;
            onAirText.verticalOverflow = VerticalWrapMode.Truncate;

            // Dialogue Message Text (Generous size, auto wrap, overflow)
            messageText = CreateLabel(consoleRoot.transform, "Message", "...", 17, new Vector2(190f, -8f), new Vector2(810f, 96f), new Color(0.92f, 0.96f, 1f), FontStyle.Normal);
            messageText.alignment = TextAnchor.UpperLeft;

            // Advance / Skip Prompt Button
            var advanceObj = new GameObject("AdvanceButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            advanceObj.transform.SetParent(consoleRoot.transform, false);
            var advRect = advanceObj.GetComponent<RectTransform>();
            advRect.anchorMin = new Vector2(1f, 0f);
            advRect.anchorMax = new Vector2(1f, 0f);
            advRect.pivot = new Vector2(1f, 0f);
            advRect.anchoredPosition = new Vector2(-22f, 16f);
            advRect.sizeDelta = new Vector2(240f, 36f);

            var advImg = advanceObj.GetComponent<Image>();
            advImg.sprite = PixelUiAtlas.ButtonPrimary;
            advImg.type = Image.Type.Sliced;
            advImg.color = new Color(0.25f, 0.5f, 0.8f, 0.95f);

            advanceButton = advanceObj.GetComponent<Button>();
            advanceButton.onClick.AddListener(AdvanceDialogue);

            promptText = CreateLabel(advanceObj.transform, "Prompt", "[ ПРОДОЛЖИТЬ (ПРОБЕЛ) >> ]", 14, Vector2.zero, new Vector2(230f, 30f), Color.white, FontStyle.Bold);
            promptText.alignment = TextAnchor.MiddleCenter;

            // Skip All Button [ESC]
            var skipObj = new GameObject("SkipAllButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            skipObj.transform.SetParent(consoleRoot.transform, false);
            var skipRect = skipObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1f, 1f);
            skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-16f, -12f);
            skipRect.sizeDelta = new Vector2(196f, 28f);

            var skipImg = skipObj.GetComponent<Image>();
            skipImg.sprite = PixelUiAtlas.ButtonSecondary;
            skipImg.type = Image.Type.Sliced;
            skipImg.color = new Color(0.2f, 0.28f, 0.42f, 0.9f);

            var skipBtn = skipObj.GetComponent<Button>();
            skipBtn.onClick.AddListener(SkipAll);

            var skipLabel = CreateLabel(skipObj.transform, "SkipText", "ПРОПУСТИТЬ [ESC]", 13, Vector2.zero, new Vector2(180f, 24f), new Color(0.85f, 0.92f, 1f), FontStyle.Bold);
            skipLabel.rectTransform.anchorMin = Vector2.zero;
            skipLabel.rectTransform.anchorMax = Vector2.one;
            skipLabel.rectTransform.offsetMin = new Vector2(6f, 2f);
            skipLabel.rectTransform.offsetMax = new Vector2(-6f, -2f);
            skipLabel.alignment = TextAnchor.MiddleCenter;
            skipLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            skipLabel.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static Text CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 pos, Vector2 size, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var label = go.GetComponent<Text>();
            label.font = UIFont.DefaultFont;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
    }
}
