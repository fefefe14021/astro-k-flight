// Assets/Scripts/Gameplay/RogueExpeditionController.cs
// Namespace: KANYE.Gameplay
// Infinite Arcade Rogue-lite Expedition Mode for Yandex Games with Anti-Overfarm Protection.

using System.Collections;
using System.Collections.Generic;
using KANYE.Audio;
using KANYE.Services;
using KANYE.Systems;
using KANYE.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.Gameplay
{
    public class RoguePerk
    {
        public string Id;
        public string Title;
        public string Subtitle;
        public string Description;
        public string IconEmoji;
        public Color ThemeColor;
        public System.Action<PlayerHealth> OnApply;

        public RoguePerk(string id, string title, string subtitle, string desc, string icon, Color color, System.Action<PlayerHealth> apply)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            Description = desc;
            IconEmoji = icon;
            ThemeColor = color;
            OnApply = apply;
        }
    }

    public class RogueExpeditionController : MonoBehaviour
    {
        public const int MaxCoinsPerRun = 150;

        public static RogueExpeditionController Instance { get; private set; }
        public static int CurrentWave { get; private set; } = 1;
        public static int CoinsEarnedThisRun { get; private set; } = 0;

        // Modifiers active for current run
        public static int BonusDamage { get; private set; } = 0;
        public static float FireRateMultiplier { get; private set; } = 1f;
        public static float MoveSpeedMultiplier { get; private set; } = 1f;
        public static float HypeMultiplier { get; private set; } = 1f;
        public static float MagnetRangeBonus { get; private set; } = 0f;
        public static readonly List<string> ActivePerkTitles = new List<string>();

        private int albumsRequiredForWave;
        private float spawnTimer;
        private float parryTimer;
        private bool isWaveTransitioning;
        private BossController activeBoss;

        private GameObject perkModalObj;
        private GameObject pauseRoot;
        private Text waveHudText;
        private Text bannerText;
        private float bannerTimer;
        private bool pausedByPlayer;

        public static void ResetRunModifiers()
        {
            CurrentWave = 1;
            CoinsEarnedThisRun = 0;
            BonusDamage = 0;
            FireRateMultiplier = 1f;
            MoveSpeedMultiplier = 1f;
            HypeMultiplier = 1f;
            MagnetRangeBonus = 0f;
            ActivePerkTitles.Clear();
        }

        public static void AddRogueCoins(int amount)
        {
            if (amount <= 0 || CoinsEarnedThisRun >= MaxCoinsPerRun)
                return;

            var remaining = MaxCoinsPerRun - CoinsEarnedThisRun;
            var toAdd = Mathf.Clamp(amount, 0, remaining);
            CoinsEarnedThisRun += toAdd;
            SaveManager.Instance?.AddCoins(toAdd);
        }

        private void Awake()
        {
            Instance = this;
            EnsureEventSystem();
            ResetRunModifiers();
            StartWave(1);
            BuildHudAndModal();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void StartWave(int wave)
        {
            CurrentWave = wave;
            albumsRequiredForWave = 10 + wave * 2;
            isWaveTransitioning = false;

            var hype = FindAnyObjectByType<HypeState>();
            if (hype != null)
                hype.BeginMission(albumsRequiredForWave);

            ShowBanner(Localization.TFormat("⚡ ВОЛНА {0} ⚡", "⚡ WAVE {0} ⚡", CurrentWave), Localization.TFormat("Маршрут волны: {0} км", "Wave route: {0} km", albumsRequiredForWave));
            AudioManager.PlayPowerup();
            UpdateHud();
        }

        private void ExitToMenu()
        {
            AudioManager.PlayClick();
            GamePause.Release();
            SceneLoader.ReturnToMainMenu();
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

            if (isWaveTransitioning || Time.timeScale <= 0f)
                return;

            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null && player.IsDead)
                return;

            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.deltaTime;
                if (bannerTimer <= 0f && bannerText != null)
                    bannerText.gameObject.SetActive(false);
            }

            // Boss wave handling (every 5th wave)
            if (CurrentWave % 5 == 0 && activeBoss == null)
            {
                SpawnRogueBoss();
                return;
            }

            if (activeBoss != null)
            {
                if (activeBoss.IsDead)
                {
                    activeBoss = null;
                    CompleteWave();
                }
                return;
            }

            // Continuous spawning
            var spawnInterval = Mathf.Max(0.6f, 1.35f - CurrentWave * 0.05f);
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnRogueAsteroid();
                if (Random.value < 0.35f)
                    SpawnRogueAsteroid(Random.Range(-1.2f, 1.2f));
            }

            // Parry spheres
            if (GameRules.IsParryUnlocked)
            {
                parryTimer += Time.deltaTime;
                if (parryTimer >= 3.2f)
                {
                    parryTimer = 0f;
                    SpawnRogueParryOrb();
                }
            }

            // Check if wave albums collected
            var currentHype = FindAnyObjectByType<HypeState>();
            currentHype?.AdvanceRoute(1.05f * Time.deltaTime);
            if (currentHype != null && currentHype.Route + 0.05f >= currentHype.RouteRequired)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            if (isWaveTransitioning) return;
            isWaveTransitioning = true;

            // Award wave coins (respecting 150 cap)
            var waveReward = Mathf.Min(16, 6 + CurrentWave * 2);
            AddRogueCoins(waveReward);

            // Audio & Screen Juice
            AudioManager.PlayVictory();
            EffectsManager.PlayExplosion(Vector3.zero, new Color(0.2f, 0.9f, 1f), isMechanical: false);
            GamePause.Lock();

            if (CurrentWave % 5 == 0)
            {
                // Boss defeated!
                ShowBanner(Localization.T("👑 БОСС ПОВЕРЖЕН!", "👑 BOSS DEFEATED!"), Localization.TFormat("+{0} монет! Подготовка к выбору перка...", "+{0} coins! Preparing perk selection...", waveReward));
            }
            else
            {
                ShowBanner(Localization.TFormat("ВОЛНА {0} ПРОЙДЕНА!", "WAVE {0} CLEARED!", CurrentWave), Localization.TFormat("+{0} монет! Выберите перк...", "+{0} coins! Choose a perk...", waveReward));
            }

            StartCoroutine(OpenPerkAfterDelay());
        }

        private IEnumerator OpenPerkAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            OpenPerkSelection();
        }

        private void OpenPerkSelection()
        {
            pausedByPlayer = false;
            if (pauseRoot != null)
                pauseRoot.SetActive(false);
            if (bannerText != null)
                bannerText.gameObject.SetActive(false);
            GamePause.Lock();
            PresentPerkModal();
        }

        private void TryTogglePause()
        {
            if (perkModalObj != null && perkModalObj.activeSelf)
                return;

            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null && player.IsDead)
                return;

            SetPaused(!pausedByPlayer);
        }

        private void SetPaused(bool paused)
        {
            pausedByPlayer = paused;
            if (pauseRoot != null)
                pauseRoot.SetActive(paused);

            if (paused)
                GamePause.Lock();
            else if (perkModalObj == null || !perkModalObj.activeSelf)
                GamePause.Release();
        }

        private void SpawnRogueAsteroid(float yOffset = 0f)
        {
            var asteroidObj = new GameObject("RogueAsteroid", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            var y = Mathf.Clamp(Random.Range(-3.2f, 3.2f) + yOffset, -3.7f, 3.7f);
            asteroidObj.transform.position = new Vector3(10.5f + Random.Range(0f, 1.2f), y, 0f);

            var large = Random.value < 0.3f;
            asteroidObj.transform.localScale = large ? Vector3.one * 1.5f : Vector3.one * 0.95f;

            var asteroid = asteroidObj.AddComponent<Asteroid>();
            var speed = (2.2f + CurrentWave * 0.08f) * (large ? 0.8f : 1.1f);
            asteroid.Configure(speed, 1, large ? 0.9f : 0.5f, large ? 2 : 1);
        }

        private void SpawnRogueParryOrb()
        {
            var orbObject = new GameObject("RogueParryOrb", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            orbObject.transform.position = new Vector3(10.5f, Random.Range(-3.2f, 3.2f), 0f);
            var hazard = orbObject.AddComponent<ParryHazard>();
            hazard.Launch(Vector2.left, 4.2f + CurrentWave * 0.05f);
        }

        private void SpawnRogueBoss()
        {
            var bossTier = ((CurrentWave / 5) - 1) % 5;
            activeBoss = BossController.SpawnBoss(bossTier);
            ShowBanner(Localization.T("⚠️ ВНИМАНИЕ: БОСС ЭКСПЕДИЦИИ!", "⚠️ WARNING: EXPEDITION BOSS!"), Localization.T("Уничтожьте флагман Синдиката для завершения сектора!", "Destroy the Syndicate flagship to clear the sector!"));
            AudioManager.PlayBossWarning();
        }

        #region UI & Modals

        private void BuildHudAndModal()
        {
            var canvasObj = new GameObject("RogueHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 220;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 1.25f;

            // Top-Center Info Badge
            var badgeObj = new GameObject("WaveBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeObj.transform.SetParent(canvasObj.transform, false);
            var bRect = badgeObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.anchoredPosition = new Vector2(0f, -14f);
            bRect.sizeDelta = new Vector2(480f, 44f);
            var bImg = badgeObj.GetComponent<Image>();
            bImg.sprite = PixelUiAtlas.SubPanel;
            bImg.type = Image.Type.Sliced;
            bImg.color = new Color(0.08f, 0.14f, 0.22f, 0.9f);

            var textObj = new GameObject("WaveText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObj.transform.SetParent(badgeObj.transform, false);
            waveHudText = textObj.GetComponent<Text>();
            waveHudText.font = GameFont.Get();
            waveHudText.fontSize = 18;
            waveHudText.alignment = TextAnchor.MiddleCenter;
            waveHudText.color = new Color(0.4f, 0.95f, 1f);
            waveHudText.fontStyle = FontStyle.Bold;
            Stretch(waveHudText.rectTransform);

            // Center Wave Banner
            var bannerObj = new GameObject("CenterBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            bannerObj.transform.SetParent(canvasObj.transform, false);
            var bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 120f);
            bannerRect.sizeDelta = new Vector2(900f, 120f);
            bannerText = bannerObj.GetComponent<Text>();
            bannerText.font = GameFont.Get();
            bannerText.fontSize = 32;
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontStyle = FontStyle.Bold;
            bannerText.color = new Color(1f, 0.85f, 0.35f);
            bannerText.gameObject.SetActive(false);

            BuildPerkModalRoot(canvasObj.transform);
            BuildPauseOverlay(canvasObj.transform);
            BuildPauseButton(canvasObj.transform);
        }

        private void ShowBanner(string main, string sub = null)
        {
            if (bannerText == null) return;
            bannerText.text = string.IsNullOrEmpty(sub) ? main : $"{main}\n<size=20><color=#90e0ef>{sub}</color></size>";
            bannerText.gameObject.SetActive(true);
            bannerTimer = 2.4f;
        }

        private void UpdateHud()
        {
            if (waveHudText == null) return;
            var capStr = CoinsEarnedThisRun >= MaxCoinsPerRun ? Localization.T("<color=#ff5555>[ЛИМИТ]</color>", "<color=#ff5555>[CAP]</color>") : "";
            waveHudText.text = Localization.TFormat("🌀 ЭКСПЕДИЦИЯ // ВОЛНА: {0}   |   🪙 {1}/{2} {3}", "🌀 EXPEDITION // WAVE: {0}   |   🪙 {1}/{2} {3}", CurrentWave, CoinsEarnedThisRun, MaxCoinsPerRun, capStr);
        }

        private void BuildPerkModalRoot(Transform parent)
        {
            perkModalObj = new GameObject("PerkModalRoot", typeof(RectTransform));
            perkModalObj.transform.SetParent(parent, false);
            Stretch(perkModalObj.GetComponent<RectTransform>());

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dim.transform.SetParent(perkModalObj.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.88f);

            var panelObj = new GameObject("PerkPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObj.transform.SetParent(perkModalObj.transform, false);
            var pRect = panelObj.GetComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(960f, 540f);
            var pImg = panelObj.GetComponent<Image>();
            pImg.sprite = PixelUiAtlas.Panel;
            pImg.type = Image.Type.Sliced;
            pImg.color = Color.white;

            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleObj.transform.SetParent(panelObj.transform, false);
            var tText = titleObj.GetComponent<Text>();
            tText.font = GameFont.Get();
            tText.fontSize = 28;
            tText.text = Localization.T("⚡ ВЫБЕРИТЕ УЛУЧШЕНИЕ ЭКСПЕДИЦИИ ⚡", "⚡ CHOOSE EXPEDITION UPGRADE ⚡");
            tText.fontStyle = FontStyle.Bold;
            tText.alignment = TextAnchor.MiddleCenter;
            tText.color = new Color(1f, 0.85f, 0.35f);
            tText.rectTransform.anchoredPosition = new Vector2(0f, 210f);
            tText.rectTransform.sizeDelta = new Vector2(800f, 44f);

            var subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            subObj.transform.SetParent(panelObj.transform, false);
            var sText = subObj.GetComponent<Text>();
            sText.font = GameFont.Get();
            sText.fontSize = 17;
            sText.text = Localization.TFormat("Волна {0} зачищена. Установите квантовый апгрейд на тарелку Зигги:", "Wave {0} cleared. Install quantum upgrade to Ziggy's saucer:", CurrentWave);
            sText.alignment = TextAnchor.MiddleCenter;
            sText.color = new Color(0.8f, 0.92f, 1f);
            sText.rectTransform.anchoredPosition = new Vector2(0f, 168f);
            sText.rectTransform.sizeDelta = new Vector2(800f, 32f);

            var leaveObj = new GameObject("LeaveButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            leaveObj.transform.SetParent(panelObj.transform, false);
            var leaveRect = leaveObj.GetComponent<RectTransform>();
            leaveRect.anchoredPosition = new Vector2(0f, -230f);
            leaveRect.sizeDelta = new Vector2(280f, 46f);
            var leaveImage = leaveObj.GetComponent<Image>();
            leaveImage.sprite = PixelUiAtlas.ButtonDanger;
            leaveImage.type = Image.Type.Sliced;
            CreateLabel(leaveObj.transform, "Label", Localization.T("В МЕНЮ", "MAIN MENU"), 18, Vector2.zero, new Vector2(260f, 40f), Color.white, FontStyle.Bold);
            leaveObj.GetComponent<Button>().onClick.AddListener(ExitToMenu);

            perkModalObj.SetActive(false);
        }

        private void BuildPauseButton(Transform parent)
        {
            var buttonObj = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            var rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-18f, -14f);
            rect.sizeDelta = new Vector2(150f, 44f);
            var image = buttonObj.GetComponent<Image>();
            image.sprite = PixelUiAtlas.ButtonSecondary;
            image.type = Image.Type.Sliced;
            CreateLabel(buttonObj.transform, "Label", Localization.T("ПАУЗА", "PAUSE"), 18, Vector2.zero, new Vector2(140f, 36f), Color.white, FontStyle.Bold);
            buttonObj.GetComponent<Button>().onClick.AddListener(TryTogglePause);
        }

        private void BuildPauseOverlay(Transform parent)
        {
            pauseRoot = new GameObject("PauseRoot", typeof(RectTransform));
            pauseRoot.transform.SetParent(parent, false);
            Stretch(pauseRoot.GetComponent<RectTransform>());

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dim.transform.SetParent(pauseRoot.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.78f);

            var panel = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(pauseRoot.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(460f, 280f);
            var panelImage = panel.GetComponent<Image>();
            panelImage.sprite = PixelUiAtlas.Panel;
            panelImage.type = Image.Type.Sliced;

            CreateLabel(panel.transform, "Title", Localization.T("ПАУЗА", "PAUSE"), 32, new Vector2(0f, 90f), new Vector2(400f, 48f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);

            var resumeObj = new GameObject("Resume", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            resumeObj.transform.SetParent(panel.transform, false);
            var resumeRect = resumeObj.GetComponent<RectTransform>();
            resumeRect.anchoredPosition = new Vector2(0f, 10f);
            resumeRect.sizeDelta = new Vector2(300f, 52f);
            var resumeImage = resumeObj.GetComponent<Image>();
            resumeImage.sprite = PixelUiAtlas.ButtonPrimary;
            resumeImage.type = Image.Type.Sliced;
            CreateLabel(resumeObj.transform, "Label", Localization.T("ПРОДОЛЖИТЬ", "RESUME"), 18, Vector2.zero, new Vector2(280f, 40f), Color.white, FontStyle.Bold);
            resumeObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                SetPaused(false);
            });

            var menuObj = new GameObject("Menu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            menuObj.transform.SetParent(panel.transform, false);
            var menuRect = menuObj.GetComponent<RectTransform>();
            menuRect.anchoredPosition = new Vector2(0f, -60f);
            menuRect.sizeDelta = new Vector2(300f, 52f);
            var menuImage = menuObj.GetComponent<Image>();
            menuImage.sprite = PixelUiAtlas.ButtonDanger;
            menuImage.type = Image.Type.Sliced;
            CreateLabel(menuObj.transform, "Label", Localization.T("В МЕНЮ", "MAIN MENU"), 18, Vector2.zero, new Vector2(280f, 40f), Color.white, FontStyle.Bold);
            menuObj.GetComponent<Button>().onClick.AddListener(ExitToMenu);

            pauseRoot.SetActive(false);
        }

        private void PresentPerkModal()
        {
            if (perkModalObj == null) return;
            perkModalObj.SetActive(true);

            var panel = perkModalObj.transform.Find("PerkPanel");
            if (panel == null) return;

            // Remove previous cards if any
            for (var i = panel.childCount - 1; i >= 0; i--)
            {
                var child = panel.GetChild(i);
                if (child.name.StartsWith("PerkCard_"))
                    Destroy(child.gameObject);
            }

            var pool = GetAvailablePerks();
            // Shuffle
            for (var i = 0; i < pool.Count; i++)
            {
                var r = Random.Range(i, pool.Count);
                var temp = pool[i];
                pool[i] = pool[r];
                pool[r] = temp;
            }

            var cardsCount = Mathf.Min(3, pool.Count);
            var startX = -280f;
            var gap = 280f;

            for (var i = 0; i < cardsCount; i++)
            {
                var perk = pool[i];
                var posX = startX + i * gap;
                CreatePerkCard(panel, perk, new Vector2(posX, -25f));
            }
        }

        private void CreatePerkCard(Transform parent, RoguePerk perk, Vector2 pos)
        {
            var cardObj = new GameObject($"PerkCard_{perk.Id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            cardObj.transform.SetParent(parent, false);
            var rect = cardObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(260f, 320f);

            var img = cardObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.SubPanel;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.08f, 0.14f, 0.22f, 0.98f);
            img.raycastTarget = true;

            void OnSelectPerk()
            {
                AudioManager.PlayClick();
                ApplyPerk(perk);
            }

            var btn = cardObj.GetComponent<Button>();
            btn.onClick.AddListener(OnSelectPerk);

            // Icon Emoji
            CreateLabel(cardObj.transform, "Icon", perk.IconEmoji, 40, new Vector2(0f, 102f), new Vector2(220f, 48f), perk.ThemeColor, FontStyle.Bold);

            // Title
            CreateLabel(cardObj.transform, "Title", perk.Title, 18, new Vector2(0f, 52f), new Vector2(230f, 32f), perk.ThemeColor, FontStyle.Bold);

            // Subtitle
            CreateLabel(cardObj.transform, "Sub", perk.Subtitle, 12, new Vector2(0f, 22f), new Vector2(230f, 20f), new Color(0.6f, 0.85f, 1f), FontStyle.Normal);

            // Description
            CreateLabel(cardObj.transform, "Desc", perk.Description, 14, new Vector2(0f, -34f), new Vector2(230f, 68f), new Color(0.9f, 0.95f, 1f), FontStyle.Normal);

            // Select Button inside card
            var pickBtnObj = new GameObject("PickBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            pickBtnObj.transform.SetParent(cardObj.transform, false);
            var pickRect = pickBtnObj.GetComponent<RectTransform>();
            pickRect.anchoredPosition = new Vector2(0f, -118f);
            pickRect.sizeDelta = new Vector2(210f, 40f);
            var pickImg = pickBtnObj.GetComponent<Image>();
            pickImg.sprite = PixelUiAtlas.ButtonPrimary;
            pickImg.type = Image.Type.Sliced;
            pickImg.raycastTarget = true;

            var pickBtn = pickBtnObj.GetComponent<Button>();
            pickBtn.onClick.AddListener(OnSelectPerk);

            CreateLabel(pickBtnObj.transform, "Label", Localization.T("ВЫБРАТЬ", "SELECT"), 16, Vector2.zero, new Vector2(200f, 34f), Color.white, FontStyle.Bold);
        }

        private void ApplyPerk(RoguePerk perk)
        {
            var player = FindAnyObjectByType<PlayerHealth>();
            perk.OnApply?.Invoke(player);
            ActivePerkTitles.Add(perk.Title);

            if (perkModalObj != null)
                perkModalObj.SetActive(false);

            GamePause.Release();
            StartWave(CurrentWave + 1);
        }

        private List<RoguePerk> GetAvailablePerks()
        {
            return new List<RoguePerk>
            {
                new RoguePerk("dmg",
                    Localization.T("Тяжелый Заряд", "Heavy Charge"),
                    Localization.T("ОРУЖИЕ", "WEAPONS"),
                    Localization.T("+1 к урону всех выстрелов тарелки.", "+1 damage to all saucer shots."),
                    "💥", new Color(1f, 0.5f, 0.2f), p => BonusDamage += 1),
                new RoguePerk("rate",
                    Localization.T("Форсаж Плазмы", "Plasma Overdrive"),
                    Localization.T("СКОРОСТЬ СТРЕЛЬБЫ", "FIRE RATE"),
                    Localization.T("+20% к темпу стрельбы.", "+20% to saucer fire rate."),
                    "⚡", new Color(0.3f, 0.9f, 1f), p => FireRateMultiplier *= 1.2f),
                new RoguePerk("armor",
                    Localization.T("Нано-Броня", "Nano-Armor"),
                    Localization.T("ЗАЩИТА", "DEFENSE"),
                    Localization.T("+1 макс. прочность и полное исцеление тарелки.", "+1 max vitality and full saucer repair."),
                    "🛡️", new Color(0.3f, 1f, 0.5f), p => p?.IncreaseMaxLives(1)),
                new RoguePerk("speed",
                    Localization.T("Ионный Форсаж", "Ionic Boost"),
                    Localization.T("МАНЕВРЕННОСТЬ", "MOBILITY"),
                    Localization.T("+25% к скорости перемещения тарелки.", "+25% to saucer flight speed."),
                    "🚀", new Color(1f, 0.85f, 0.3f), p => MoveSpeedMultiplier *= 1.25f),
                new RoguePerk("hype",
                    Localization.T("Резонатор ярости", "Hype Resonator"),
                    Localization.T("СПЕЦ-АТАКИ", "SPECIAL ATTACKS"),
                    Localization.T("+40% к скорости накопления шкалы Ярости.", "+40% faster Hype meter gain."),
                    "🌀", new Color(0.9f, 0.4f, 1f), p => HypeMultiplier *= 1.4f),
                new RoguePerk("magnet",
                    Localization.T("Грави-Магнит", "Grav-Magnet"),
                    Localization.T("РЕСУРСЫ", "RESOURCES"),
                    Localization.T("Притягивает наниты и сферы с удвоенного расстояния.", "Attracts nanites and relics from 2x range."),
                    "🧲", new Color(0.4f, 0.8f, 1f), p => MagnetRangeBonus += 3.5f)
            };
        }

        private static Text CreateLabel(Transform parent, string name, string text, int size, Vector2 pos, Vector2 sizeDelta, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = GameFont.Get();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
