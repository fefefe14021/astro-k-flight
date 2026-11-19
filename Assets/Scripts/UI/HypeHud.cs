// Assets/Scripts/UI/HypeHud.cs
// Namespace: KANYE.UI
// Cuphead & Broforce style arcade HUD with segmented Hype cards, Boss Health Bar, and Lives hearts.

using System.Collections;
using KANYE.Audio;
using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class HypeHud : MonoBehaviour
    {
        public static HypeHud Instance { get; private set; }

        [SerializeField] private float fillSpeed = 2.2f;

        private HypeState hypeState;
        private ShipHealth shipHealth;

        private Image hypeFill;
        private Image[] cardPips = new Image[4];
        private Text albumLabel;
        private Text rageLabel;
        private GameObject rageMeter;
        private RectTransform centerPanelRect;
        private RectTransform livesPanelRect;
        private Image[] heartImages = new Image[6];
        private Transform heartsContainer;
        private Text parryLabel;

        // EX & Super Action Prompts
        private GameObject exButtonObj;
        private GameObject superButtonObj;

        // Boss Health Bar UI
        private GameObject bossBarObj;
        private Text bossNameText;
        private Image bossFillImage;

        // Big Banner System ("WALLOP!", "WARNING!", "KNOCKOUT!")
        private GameObject bannerObj;
        private Text bannerTitleText;
        private Text bannerSubText;
        private CanvasGroup bannerCanvasGroup;

        private float targetHypeFill;
        private GameObject pauseRoot;
        private bool pausedByPlayer;
        private Text pauseBtnLabel;
        private Text pauseTitleLabel;
        private Text resumeBtnLabel;
        private Text menuBtnLabel;

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

            if (FindAnyObjectByType<HypeHud>() != null)
                return;

            new GameObject("HypeHud").AddComponent<HypeHud>();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            hypeState = FindAnyObjectByType<HypeState>();
            shipHealth = FindAnyObjectByType<ShipHealth>();

            BuildHUD();

            if (hypeState != null)
                hypeState.Changed += Refresh;
            if (shipHealth != null)
                shipHealth.Changed += Refresh;

            Refresh();
            StartCoroutine(ShowMissionStartBanner());
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (hypeState != null)
                hypeState.Changed -= Refresh;
            if (shipHealth != null)
                shipHealth.Changed -= Refresh;
        }

        private IEnumerator ShowMissionStartBanner()
        {
            while (MissionRadioComms.IsCommsActive)
                yield return null;

            yield return new WaitForSecondsRealtime(0.25f);
            ShowBanner("READY? FIGHT!", Localization.T("Сокруши вражеский флот и собери ресурсы!", "Defeat the enemy fleet and extract resources!"), 2.0f, new Color(1f, 0.85f, 0.2f));
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                TryTogglePause();
#else
            if (Input.GetKeyDown(KeyCode.Escape))
                TryTogglePause();
#endif

            if (hypeState == null)
            {
                hypeState = FindAnyObjectByType<HypeState>();
                if (hypeState != null)
                {
                    hypeState.Changed += Refresh;
                    Refresh();
                }
            }

            if (shipHealth == null)
            {
                shipHealth = FindAnyObjectByType<ShipHealth>();
                if (shipHealth != null)
                {
                    shipHealth.Changed += Refresh;
                    Refresh();
                }
            }

            // 1. Smooth Hype Meter Fill
            if (hypeFill != null)
            {
                var step = fillSpeed * Time.unscaledDeltaTime;
                hypeFill.fillAmount = Mathf.MoveTowards(hypeFill.fillAmount, targetHypeFill, step);

                var isFull = hypeFill.fillAmount >= 0.99f;
                if (isFull)
                {
                    var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f);
                    hypeFill.color = Color.Lerp(new Color(1f, 0.72f, 0.15f), new Color(1f, 0.98f, 0.65f), pulse);
                }
                else
                {
                    hypeFill.color = new Color(0.2f, 0.85f, 1f, 1f); // Sleek cyan energy
                }
            }

            // 2. EX & Super Button visibility
            var canEx = hypeState != null && hypeState.CanUseEX;
            var canSuper = hypeState != null && hypeState.CanUseSuper;

            ApplyRageMeter(HypeState.RageCanCharge);

            if (exButtonObj != null && exButtonObj.activeSelf != (canEx && !canSuper))
                exButtonObj.SetActive(canEx && !canSuper);

            if (superButtonObj != null && superButtonObj.activeSelf != canSuper)
                superButtonObj.SetActive(canSuper);

            // 3. Boss Health Bar Binding
            var boss = BossController.ActiveBoss;
            if (boss != null && !boss.IsDead)
            {
                if (bossBarObj != null && !bossBarObj.activeSelf)
                {
                    bossBarObj.SetActive(true);
                    if (bossNameText != null)
                        bossNameText.text = "⚠️ " + boss.BossName;
                    ShowBanner("ВНИМАНИЕ: БОСС!", boss.BossName, 2.0f, new Color(1f, 0.25f, 0.25f));
                }

                if (bossFillImage != null && boss.MaxHealth > 0)
                {
                    var target = (float)boss.CurrentHealth / boss.MaxHealth;
                    bossFillImage.fillAmount = Mathf.MoveTowards(bossFillImage.fillAmount, target, Time.deltaTime * 1.5f);
                    bossFillImage.color = boss.IsEnraged ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.5f, 0.1f);
                }
            }
            else
            {
                if (bossBarObj != null && bossBarObj.activeSelf)
                    bossBarObj.SetActive(false);
            }
        }

        public void ShowBanner(string title, string subtitle, float duration, Color titleColor)
        {
            if (bannerObj == null) return;
            StartCoroutine(BannerRoutine(title, subtitle, duration, titleColor));
        }

        private IEnumerator BannerRoutine(string title, string subtitle, float duration, Color titleColor)
        {
            if (bannerTitleText != null)
            {
                bannerTitleText.text = title;
                bannerTitleText.color = titleColor;
            }
            if (bannerSubText != null)
                bannerSubText.text = subtitle;

            bannerObj.SetActive(true);
            bannerCanvasGroup.alpha = 0f;

            // Fade in
            var t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                bannerCanvasGroup.alpha = t / 0.25f;
                yield return null;
            }

            bannerCanvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(duration);

            // Fade out
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                bannerCanvasGroup.alpha = 1f - (t / 0.35f);
                yield return null;
            }

            bannerObj.SetActive(false);
        }

        private void ApplyRageMeter(bool open)
        {
            if (rageLabel != null && rageLabel.gameObject.activeSelf != open)
                rageLabel.gameObject.SetActive(open);
            if (rageMeter != null && rageMeter.activeSelf != open)
                rageMeter.SetActive(open);
            if (centerPanelRect != null)
                centerPanelRect.sizeDelta = new Vector2(520f, open ? 76f : 44f);
            if (albumLabel != null)
            {
                albumLabel.alignment = open ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                albumLabel.rectTransform.anchoredPosition = open ? new Vector2(78f, 16f) : Vector2.zero;
                albumLabel.rectTransform.sizeDelta = new Vector2(open ? 300f : 480f, 28f);
            }
        }

        private void Refresh()
        {
            if (hypeState != null)
            {
                targetHypeFill = !HypeState.RageCanCharge || hypeState.HypeMax <= 0f
                    ? 0f
                    : hypeState.Hype / hypeState.HypeMax;
                var req = hypeState.RouteRequired > 0f ? hypeState.RouteRequired : 6f;
                if (albumLabel != null)
                    albumLabel.text = Localization.TFormat("ПУТЬ: {0} / {1} км", "ROUTE: {0} / {1} km", Mathf.FloorToInt(hypeState.Route), Mathf.FloorToInt(req));

                // Update EX Card pips
                var cards = hypeState.CardsCount;
                for (var i = 0; i < cardPips.Length; i++)
                {
                    if (cardPips[i] != null)
                    {
                        var filled = i < cards;
                        cardPips[i].color = filled 
                            ? (cards == 4 ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.25f, 0.95f, 1f, 1f)) 
                            : new Color(0.12f, 0.2f, 0.3f, 0.65f);
                    }
                }
            }

            if (shipHealth != null)
            {
                var hp = Mathf.Max(0, shipHealth.Lives);
                var maxHp = Mathf.Clamp(shipHealth.MaxLives, 1, heartImages.Length);

                // Dynamically fit panel width tightly to max lives (34px per heart + 22px padding)
                if (livesPanelRect != null)
                {
                    var targetWidth = Mathf.Max(90f, maxHp * 34f + 22f);
                    livesPanelRect.sizeDelta = new Vector2(targetWidth, 44f);
                }

                for (var i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] == null) continue;

                    if (i < maxHp)
                    {
                        heartImages[i].gameObject.SetActive(true);
                        var isFull = i < hp;
                        heartImages[i].sprite = isFull ? PixelUiAtlas.IconHeartFull : PixelUiAtlas.IconHeartEmpty;
                        heartImages[i].color = isFull ? Color.white : new Color(0.45f, 0.55f, 0.7f, 0.65f);

                        // Center hearts within the container
                        var rt = heartImages[i].rectTransform;
                        rt.anchoredPosition = new Vector2((i - (maxHp - 1) * 0.5f) * 34f, 0f);
                    }
                    else
                    {
                        heartImages[i].gameObject.SetActive(false);
                    }
                }
            }

            if (parryLabel != null)
            {
                if (Systems.GameRules.IsParryUnlocked)
                {
                    parryLabel.gameObject.SetActive(true);
                    parryLabel.text = Localization.TFormat("ПАРИРОВАНИЕ: {0}", "PARRY: {0}", Systems.GameRules.ParriesThisRun);
                }
                else
                {
                    parryLabel.gameObject.SetActive(false);
                }
            }
        }

        private void BuildHUD()
        {
            var canvasObj = new GameObject("HypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 1.25f;

            // 1. Top-Left: Pilot Vitality (Lives hearts) - Compact, clean arcade panel
            var livesPanel = CreateImage("LivesPanel", canvasObj.transform, PixelUiAtlas.SubPanel, Color.white);
            livesPanel.type = Image.Type.Sliced;
            livesPanelRect = livesPanel.rectTransform;
            livesPanelRect.anchorMin = new Vector2(0f, 1f);
            livesPanelRect.anchorMax = new Vector2(0f, 1f);
            livesPanelRect.pivot = new Vector2(0f, 1f);
            livesPanelRect.anchoredPosition = new Vector2(24f, -24f);
            livesPanelRect.sizeDelta = new Vector2(124f, 44f);

            var containerObj = new GameObject("HeartsContainer", typeof(RectTransform));
            containerObj.transform.SetParent(livesPanel.transform, false);
            heartsContainer = containerObj.transform;
            var containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(1f, 1f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            for (var i = 0; i < heartImages.Length; i++)
            {
                var hObj = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                hObj.transform.SetParent(heartsContainer, false);
                var hRect = hObj.GetComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0.5f, 0.5f);
                hRect.anchorMax = new Vector2(0.5f, 0.5f);
                hRect.pivot = new Vector2(0.5f, 0.5f);
                hRect.anchoredPosition = new Vector2((i - 1f) * 34f, 0f);
                hRect.sizeDelta = new Vector2(26f, 26f);

                var hImg = hObj.GetComponent<Image>();
                hImg.sprite = PixelUiAtlas.IconHeartFull;
                hImg.preserveAspect = true;
                heartImages[i] = hImg;
                hObj.SetActive(false);
            }

            parryLabel = CreateText("ParryText", canvasObj.transform, Localization.T("ПАРИРОВАНИЕ: 0", "PARRY: 0"), 16, new Vector2(24f, -76f), new Color(1f, 0.45f, 0.85f, 1f));
            parryLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            parryLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            parryLabel.rectTransform.pivot = new Vector2(0f, 1f);
            parryLabel.alignment = TextAnchor.MiddleLeft;
            parryLabel.rectTransform.sizeDelta = new Vector2(240f, 26f);

            // 2. Top-Center: Segmented Hype / EX Meter & Album Objectives
            var centerPanel = CreateImage("CenterPanel", canvasObj.transform, PixelUiAtlas.SubPanel, Color.white);
            centerPanel.type = Image.Type.Sliced;
            centerPanelRect = centerPanel.rectTransform;
            centerPanelRect.anchorMin = new Vector2(0.5f, 1f);
            centerPanelRect.anchorMax = new Vector2(0.5f, 1f);
            centerPanelRect.pivot = new Vector2(0.5f, 1f);
            centerPanelRect.anchoredPosition = new Vector2(0f, -20f);
            centerPanelRect.sizeDelta = new Vector2(520f, 76f);

            rageLabel = CreateText("RageLabel", centerPanel.transform, Localization.T("ЯРОСТЬ", "HYPE"), 16, new Vector2(-188f, 16f), new Color(0.45f, 0.92f, 1f, 1f));
            rageLabel.fontStyle = FontStyle.Bold;
            rageLabel.alignment = TextAnchor.MiddleLeft;
            rageLabel.rectTransform.sizeDelta = new Vector2(140f, 28f);

            albumLabel = CreateText("AlbumLabel", centerPanel.transform, Localization.T("ПУТЬ: 0 / 0 км", "ROUTE: 0 / 0 km"), 18, new Vector2(78f, 16f), new Color(1f, 0.88f, 0.4f, 1f));
            albumLabel.fontStyle = FontStyle.Bold;
            albumLabel.alignment = TextAnchor.MiddleRight;
            albumLabel.rectTransform.sizeDelta = new Vector2(300f, 28f);

            // Meter Frame
            var meterFrame = CreateImage("MeterFrame", centerPanel.transform, PixelUiAtlas.White, new Color(0.04f, 0.08f, 0.14f, 0.95f));
            rageMeter = meterFrame.gameObject;
            meterFrame.rectTransform.anchoredPosition = new Vector2(0f, -16f);
            meterFrame.rectTransform.sizeDelta = new Vector2(460f, 22f);

            // Continuous fill behind cards
            var fillObj = CreateImage("Fill", meterFrame.transform, PixelUiAtlas.White, new Color(0.2f, 0.85f, 1f, 1f));
            var fillRect = fillObj.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            hypeFill = fillObj;
            hypeFill.type = Image.Type.Filled;
            hypeFill.fillMethod = Image.FillMethod.Horizontal;
            hypeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            hypeFill.fillAmount = 0f;

            // 4 Card segment markers (Cuphead card style!)
            for (var i = 0; i < 4; i++)
            {
                var pip = CreateImage($"CardPip_{i}", meterFrame.transform, PixelUiAtlas.SubPanel, new Color(0.05f, 0.08f, 0.12f, 0.18f));
                pip.type = Image.Type.Sliced;
                var pipRect = pip.rectTransform;
                pipRect.anchorMin = new Vector2((float)i / 4f, 0f);
                pipRect.anchorMax = new Vector2((float)(i + 1) / 4f, 1f);
                pipRect.offsetMin = new Vector2(2f, 2f);
                pipRect.offsetMax = new Vector2(-2f, -2f);
                cardPips[i] = pip;
            }

            hypeFill.transform.SetAsLastSibling();

            // 3. Action Prompts / Buttons for EX [Q] and Super [E]
            exButtonObj = CreatePromptButton(canvasObj.transform, "EXButton", Localization.T("EX-РАКЕТА [Q]", "EX-ROCKET [Q]"), new Vector2(0f, -112f), new Color(0.2f, 0.75f, 1f), () =>
            {
                var shooter = FindAnyObjectByType<ShipShooter>();
                shooter?.TryFireEX();
            });
            exButtonObj.SetActive(false);

            superButtonObj = CreatePromptButton(canvasObj.transform, "SuperButton", Localization.T("МЕГА-ЛАЗЕР [E]", "MEGA-LASER [E]"), new Vector2(0f, -112f), new Color(1f, 0.8f, 0.15f), () =>
            {
                var shooter = FindAnyObjectByType<ShipShooter>();
                shooter?.TryFireSuper();
            });
            superButtonObj.SetActive(false);

            // 4. Boss Health Bar (Top screen, below meters)
            bossBarObj = new GameObject("BossHealthBar", typeof(RectTransform));
            bossBarObj.transform.SetParent(canvasObj.transform, false);
            var bossRect = bossBarObj.GetComponent<RectTransform>();
            bossRect.anchorMin = new Vector2(0.5f, 1f);
            bossRect.anchorMax = new Vector2(0.5f, 1f);
            bossRect.pivot = new Vector2(0.5f, 1f);
            bossRect.anchoredPosition = new Vector2(0f, -105f);
            bossRect.sizeDelta = new Vector2(680f, 52f);

            var bossBg = CreateImage("BossBg", bossBarObj.transform, PixelUiAtlas.SubPanel, Color.white);
            bossBg.type = Image.Type.Sliced;
            Stretch(bossBg.rectTransform);

            bossNameText = CreateText("BossName", bossBarObj.transform, "⚠️ VANGUARD DREAD-DRONE", 17, new Vector2(0f, 12f), new Color(1f, 0.35f, 0.35f, 1f));
            bossNameText.fontStyle = FontStyle.Bold;

            var bossBarInner = CreateImage("BossInner", bossBarObj.transform, PixelUiAtlas.White, new Color(0.06f, 0.08f, 0.12f, 0.95f));
            bossBarInner.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            bossBarInner.rectTransform.sizeDelta = new Vector2(640f, 16f);

            var bFill = CreateImage("BossFill", bossBarInner.transform, PixelUiAtlas.White, new Color(1f, 0.45f, 0.15f, 1f));
            Stretch(bFill.rectTransform);
            bossFillImage = bFill;
            bossFillImage.type = Image.Type.Filled;
            bossFillImage.fillMethod = Image.FillMethod.Horizontal;
            bossFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            bossFillImage.fillAmount = 1f;

            bossBarObj.SetActive(false);

            // 5. Epic Banner Notifications ("READY? FIGHT!", "A KNOCKOUT!")
            bannerObj = new GameObject("BannerContainer", typeof(RectTransform), typeof(CanvasGroup));
            bannerObj.transform.SetParent(canvasObj.transform, false);
            bannerCanvasGroup = bannerObj.GetComponent<CanvasGroup>();
            var bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 80f);
            bannerRect.sizeDelta = new Vector2(720f, 140f);

            var bannerBg = CreateImage("BannerBg", bannerObj.transform, PixelUiAtlas.Panel, Color.white);
            bannerBg.type = Image.Type.Sliced;
            Stretch(bannerBg.rectTransform);

            bannerTitleText = CreateText("BannerTitle", bannerObj.transform, "READY? FIGHT!", 36, new Vector2(0f, 22f), new Color(1f, 0.85f, 0.2f));
            bannerTitleText.fontStyle = FontStyle.Bold;

            bannerSubText = CreateText("BannerSub", bannerObj.transform, "Сокруши вражеский флот!", 20, new Vector2(0f, -24f), new Color(0.85f, 0.92f, 1f));

            bannerObj.SetActive(false);

            if (GameRules.CurrentMode == GameMode.Campaign)
                BuildMissionPause(canvasObj.transform);
        }

        private void TryTogglePause()
        {
            if (GameRules.CurrentMode != GameMode.Campaign)
                return;

            if (!pausedByPlayer && !CanPauseMission())
                return;

            SetPaused(!pausedByPlayer);
        }

        private static bool CanPauseMission()
        {
            if (MissionRadioComms.IsCommsActive || MissionController.IsMissionFinished)
                return false;

            var player = FindAnyObjectByType<PlayerHealth>();
            return player == null || !player.IsDead;
        }

        private void SetPaused(bool paused)
        {
            if (paused && !CanPauseMission())
                return;

            pausedByPlayer = paused;
            if (pauseRoot != null)
                pauseRoot.SetActive(paused);
            TutorialHints.SetCoveredByPause(paused);

            if (paused)
            {
                if (pauseBtnLabel != null) pauseBtnLabel.text = Localization.T("ПАУЗА", "PAUSE");
                if (pauseTitleLabel != null) pauseTitleLabel.text = Localization.T("ПАУЗА", "PAUSE");
                if (resumeBtnLabel != null) resumeBtnLabel.text = Localization.T("ПРОДОЛЖИТЬ", "RESUME");
                if (menuBtnLabel != null) menuBtnLabel.text = Localization.T("В МЕНЮ", "MAIN MENU");
                GamePause.Lock();
            }
            else if (!MissionController.IsMissionFinished)
            {
                GamePause.Release();
            }
        }

        private void BuildMissionPause(Transform parent)
        {
            var buttonObj = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-24f, -24f);
            buttonRect.sizeDelta = new Vector2(150f, 52f);
            var buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.sprite = PixelUiAtlas.ButtonSecondary;
            buttonImage.type = Image.Type.Sliced;
            pauseBtnLabel = CreateText("Label", buttonObj.transform, Localization.T("ПАУЗА", "PAUSE"), 18, Vector2.zero, Color.white);
            pauseBtnLabel.fontStyle = FontStyle.Bold;
            buttonObj.GetComponent<Button>().onClick.AddListener(TryTogglePause);

            pauseRoot = new GameObject("PauseRoot", typeof(RectTransform));
            pauseRoot.transform.SetParent(parent, false);
            Stretch(pauseRoot.GetComponent<RectTransform>());

            var dim = CreateImage("Dim", pauseRoot.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.08f, 0.78f));
            Stretch(dim.rectTransform);

            var panel = CreateImage("PausePanel", pauseRoot.transform, PixelUiAtlas.Panel, Color.white);
            panel.type = Image.Type.Sliced;
            panel.rectTransform.sizeDelta = new Vector2(460f, 280f);

            pauseTitleLabel = CreateText("Title", panel.transform, Localization.T("ПАУЗА", "PAUSE"), 32, new Vector2(0f, 90f), new Color(1f, 0.85f, 0.35f));
            pauseTitleLabel.fontStyle = FontStyle.Bold;

            resumeBtnLabel = CreatePauseChoice(panel.transform, "Resume", Localization.T("ПРОДОЛЖИТЬ", "RESUME"), new Vector2(0f, 10f), PixelUiAtlas.ButtonPrimary, () =>
            {
                AudioManager.PlayClick();
                SetPaused(false);
            });
            menuBtnLabel = CreatePauseChoice(panel.transform, "Menu", Localization.T("В МЕНЮ", "MAIN MENU"), new Vector2(0f, -60f), PixelUiAtlas.ButtonDanger, () =>
            {
                AudioManager.PlayClick();
                GamePause.Release();
                SceneLoader.ReturnToMainMenu();
            });

            pauseRoot.SetActive(false);
        }

        private static Text CreatePauseChoice(Transform parent, string name, string text, Vector2 position, Sprite sprite, UnityEngine.Events.UnityAction onClick)
        {
            var choice = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            choice.transform.SetParent(parent, false);
            var rect = choice.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300f, 52f);
            var image = choice.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            var label = CreateText("Label", choice.transform, text, 18, Vector2.zero, Color.white);
            label.fontStyle = FontStyle.Bold;
            choice.GetComponent<Button>().onClick.AddListener(onClick);
            return label;
        }

        private static GameObject CreatePromptButton(Transform parent, string name, string text, Vector2 pos, Color highlight, System.Action onClick)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(260f, 38f);

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonPrimary;
            img.type = Image.Type.Sliced;
            img.color = highlight;

            var label = CreateText("Label", btnObj.transform, text, 18, Vector2.zero, Color.white);
            label.fontStyle = FontStyle.Bold;

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btnObj;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, Vector2 pos, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(500f, 36f);

            var label = obj.GetComponent<Text>();
            label.font = GameFont.Get();
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
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
