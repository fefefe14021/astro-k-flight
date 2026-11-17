// Assets/Scripts/UI/CinematicIntroCutscene.cs
// Namespace: KANYE.UI
// Kinetic comic-style Cold Open cutscene designed for maximum early retention (TikTok-style hook).

using System;
using System.Collections;
using KANYE.Audio;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class CinematicIntroCutscene : MonoBehaviour
    {
        public static CinematicIntroCutscene Instance { get; private set; }

        private GameObject rootObj;
        private Image backdropImage;
        private Image comicArtImage;
        private Image speakerPortrait;
        private Text titleText;
        private Text speakerNameText;
        private Text dialogueText;
        private Text instructionText;
        private Button skipButton;

        private Action onCompleteCallback;
        private int currentFrame = 0;
        private float frameTimer;
        private const float AutoAdvanceTime = 3.6f;
        private bool isTransitioning;

        private struct ComicFrame
        {
            public string Header;
            public string Speaker;
            public string Speech;
            public string SpritePath;
            public Color FrameColor;
            public Action PlaySound;

            public ComicFrame(string header, string speaker, string speech, string sprite, Color color, Action sound)
            {
                Header = header;
                Speaker = speaker;
                Speech = speech;
                SpritePath = sprite;
                FrameColor = color;
                PlaySound = sound;
            }
        }

        private static ComicFrame[] frames;

        private static void InitFrames()
        {
            if (frames != null) return;
            frames = new[]
            {
                new ComicFrame(
                    "🚨 ЭКСТРЕННАЯ ТРЕВОГА // СЕКТОР 01",
                    "СИСТЕМНЫЙ СКАНИРУЮЩИЙ ДРОН",
                    "ВНИМАНИЕ! Синдикат «Авангард» атаковал пять секторов! 50 Великих Золотых Винилов Hype Core похищены! На галактику надвигается Великое Глушение!",
                    "Sprites/boss_drone",
                    new Color(1f, 0.25f, 0.25f),
                    () => AudioManager.PlayBossWarning()),

                new ComicFrame(
                    "📡 РАДИОСТАНЦИЯ «НЕОНОВЫЙ МАЯК»",
                    "ШТУРМАН БАРНИ",
                    "Зигги, дружище, приём! Слышишь этот вой сирен? Они заперли винил внутри своих боевых боссов! Если мы не прорвем блокаду — вся музыка и свобода обратятся в серый монохром!",
                    "Sprites/barney_portrait",
                    new Color(0.35f, 0.9f, 1f),
                    () => AudioManager.PlayRadioOpen()),

                new ComicFrame(
                    "🛸 АНГАР // ASTRO-SAUCER X-1",
                    "КАПИТАН ЗИГГИ",
                    "Спокойно, Барни! Моя тарелка заправлена виниловым огнем, а штурвал откалиброван. Ни один корпоративный дрон не отнимет у нас наш бит!",
                    "Sprites/ship",
                    new Color(1f, 0.85f, 0.35f),
                    () => AudioManager.PlayPowerup()),

                new ComicFrame(
                    "⚡ ГИПЕР-ФОРСАЖ АКТИВИРОВАН",
                    "БОРТОВОЙ КОМПЬЮТЕР",
                    "Кварцевый купол загерметизирован! Ионные приводы на 100%! ВПЕРЁД, В БОЙ ЗА ЗВУК И СВОБОДУ ВСЕЛЕННОЙ!",
                    "Sprites/thruster_ion",
                    new Color(0.4f, 1f, 0.5f),
                    () => AudioManager.PlayEXShoot())
            };
        }

        public static void Play(Action onFinish)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.SetIntroSeen(true);

            onFinish?.Invoke();
        }

        private void Awake()
        {
            Instance = this;
            InitFrames();
            BuildUI();
            if (rootObj != null)
                rootObj.SetActive(false);
        }

        private void StartCutscene(Action onFinish)
        {
            onCompleteCallback = onFinish;
            currentFrame = 0;
            frameTimer = AutoAdvanceTime;

            if (rootObj != null)
                rootObj.SetActive(true);

            Time.timeScale = 0f;
            DisplayCurrentFrame();
        }

        private void Update()
        {
            if (rootObj == null || !rootObj.activeSelf)
                return;

            // Spacebar, Enter, or Click to advance
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextFrame();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FinishCutscene();
                return;
            }

            frameTimer -= Time.unscaledDeltaTime;
            if (frameTimer <= 0f)
            {
                NextFrame();
            }
        }

        private void NextFrame()
        {
            if (isTransitioning) return;
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                FinishCutscene();
            }
            else
            {
                StartCoroutine(FrameTransitionRoutine());
            }
        }

        private IEnumerator FrameTransitionRoutine()
        {
            isTransitioning = true;
            frameTimer = AutoAdvanceTime;

            // Quick micro flash
            if (backdropImage != null)
                backdropImage.color = new Color(0.15f, 0.2f, 0.3f, 0.98f);
            yield return new WaitForSecondsRealtime(0.06f);
            if (backdropImage != null)
                backdropImage.color = new Color(0.03f, 0.05f, 0.09f, 0.98f);

            DisplayCurrentFrame();
            isTransitioning = false;
        }

        private void DisplayCurrentFrame()
        {
            if (currentFrame < 0 || currentFrame >= frames.Length)
                return;

            var f = frames[currentFrame];
            if (titleText != null)
            {
                titleText.text = f.Header;
                titleText.color = f.FrameColor;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = $"▶  {f.Speaker}";
                speakerNameText.color = Color.Lerp(f.FrameColor, Color.white, 0.5f);
            }

            if (dialogueText != null)
            {
                dialogueText.text = f.Speech;
            }

            if (instructionText != null)
            {
                instructionText.text = $"[ Кадр {currentFrame + 1}/{frames.Length} ]  Нажмите [ПРОБЕЛ] или коснитесь экрана для продолжения ▶";
            }

            if (speakerPortrait != null)
            {
                var sprite = Resources.Load<Sprite>(f.SpritePath);
                if (sprite != null)
                {
                    speakerPortrait.sprite = sprite;
                    speakerPortrait.gameObject.SetActive(true);
                }
                else
                {
                    speakerPortrait.gameObject.SetActive(false);
                }
            }

            f.PlaySound?.Invoke();
        }

        private void FinishCutscene()
        {
            if (rootObj != null)
                rootObj.SetActive(false);

            Time.timeScale = 1f;
            SaveManager.Instance?.MarkIntroSeen(true);

            var cb = onCompleteCallback;
            onCompleteCallback = null;
            cb?.Invoke();
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("CutsceneCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 550;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            rootObj = new GameObject("Root", typeof(RectTransform));
            rootObj.transform.SetParent(canvasObj.transform, false);
            Stretch(rootObj.GetComponent<RectTransform>());

            // Background & Click Detector to advance
            var bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            bgObj.transform.SetParent(rootObj.transform, false);
            Stretch(bgObj.GetComponent<RectTransform>());
            backdropImage = bgObj.GetComponent<Image>();
            backdropImage.color = new Color(0.03f, 0.05f, 0.09f, 0.98f);
            var bgBtn = bgObj.GetComponent<Button>();
            bgBtn.onClick.AddListener(NextFrame);

            // Cinema letterbox top & bottom
            var topBar = CreateImage("TopLetterbox", rootObj.transform, null, Color.black);
            var tbRect = topBar.rectTransform;
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0f, 65f);

            var bottomBar = CreateImage("BottomLetterbox", rootObj.transform, null, Color.black);
            var bbRect = bottomBar.rectTransform;
            bbRect.anchorMin = new Vector2(0f, 0f);
            bbRect.anchorMax = new Vector2(1f, 0f);
            bbRect.pivot = new Vector2(0.5f, 0f);
            bbRect.sizeDelta = new Vector2(0f, 65f);

            // Main Comic Frame Panel
            var comicPanel = CreateImage("ComicPanel", rootObj.transform, PixelUiAtlas.Panel, Color.white);
            comicPanel.type = Image.Type.Sliced;
            var cpRect = comicPanel.rectTransform;
            cpRect.anchoredPosition = new Vector2(0f, 15f);
            cpRect.sizeDelta = new Vector2(1100f, 620f);

            // Frame Header / Siren
            titleText = CreateLabel(comicPanel.transform, "Title", "🚨 ТРЕВОГА // СЕКТОР 01", 26, new Vector2(0f, 255f), new Vector2(980f, 44f), new Color(1f, 0.35f, 0.35f), FontStyle.Bold);

            // Visual Frame Container (Square with glowing border)
            var artBox = CreateImage("ArtBox", comicPanel.transform, PixelUiAtlas.SubPanel, new Color(0.08f, 0.12f, 0.2f, 0.95f));
            artBox.type = Image.Type.Sliced;
            var abRect = artBox.rectTransform;
            abRect.anchoredPosition = new Vector2(-280f, 40f);
            abRect.sizeDelta = new Vector2(340f, 320f);

            speakerPortrait = CreateImage("Portrait", artBox.transform, null, Color.white);
            speakerPortrait.preserveAspect = true;
            speakerPortrait.rectTransform.sizeDelta = new Vector2(260f, 240f);

            // Dialogue Box (Right Side)
            var speechBox = CreateImage("SpeechBox", comicPanel.transform, PixelUiAtlas.SubPanel, new Color(0.05f, 0.08f, 0.14f, 0.95f));
            speechBox.type = Image.Type.Sliced;
            var sbRect = speechBox.rectTransform;
            sbRect.anchoredPosition = new Vector2(190f, 40f);
            sbRect.sizeDelta = new Vector2(560f, 320f);

            speakerNameText = CreateLabel(speechBox.transform, "SpeakerName", "ШТУРМАН БАРНИ", 22, new Vector2(0f, 115f), new Vector2(500f, 34f), new Color(0.4f, 0.9f, 1f), FontStyle.Bold);
            speakerNameText.alignment = TextAnchor.MiddleLeft;

            dialogueText = CreateLabel(speechBox.transform, "Dialogue", "Текст сообщения...", 20, new Vector2(0f, -10f), new Vector2(500f, 190f), new Color(0.92f, 0.96f, 1f), FontStyle.Normal);
            dialogueText.alignment = TextAnchor.UpperLeft;

            instructionText = CreateLabel(comicPanel.transform, "Instruction", "[ Нажмите ПРОБЕЛ для продолжения ]", 16, new Vector2(0f, -255f), new Vector2(980f, 32f), new Color(0.6f, 0.8f, 1f, 0.85f), FontStyle.Normal);

            // Prominent SKIP Button (Top Right)
            var skipBtnObj = new GameObject("SkipBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            skipBtnObj.transform.SetParent(rootObj.transform, false);
            var skipRect = skipBtnObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1f, 1f);
            skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-35f, -12f);
            skipRect.sizeDelta = new Vector2(280f, 46f);

            var skipImg = skipBtnObj.GetComponent<Image>();
            skipImg.sprite = PixelUiAtlas.ButtonPrimary;
            skipImg.type = Image.Type.Sliced;
            skipImg.color = new Color(1f, 0.85f, 0.35f);

            skipButton = skipBtnObj.GetComponent<Button>();
            skipButton.onClick.AddListener(FinishCutscene);

            var skipLabel = CreateLabel(skipBtnObj.transform, "Label", "ПРОПУСТИТЬ >> [ПРОБЕЛ]", 16, Vector2.zero, new Vector2(260f, 36f), Color.white, FontStyle.Bold);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static Text CreateLabel(Transform parent, string name, string text, int size, Vector2 pos, Vector2 sizeDelta, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = UIFont.DefaultFont;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            return t;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
