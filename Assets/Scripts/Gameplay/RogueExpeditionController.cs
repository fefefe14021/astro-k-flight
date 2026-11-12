// Assets/Scripts/Gameplay/RogueExpeditionController.cs
// Namespace: KANYE.Gameplay
// Infinite Arcade Rogue-lite Expedition Mode for Yandex Games with Anti-Overfarm Protection.

using System.Collections.Generic;
using KANYE.Audio;
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
        private Text waveHudText;
        private Text bannerText;
        private float bannerTimer;

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

            ShowBanner($"⚡ ВОЛНА {CurrentWave} ⚡", $"Квота винила: {albumsRequiredForWave} Альбомов");
            AudioManager.PlayPowerup();
            UpdateHud();
        }

        private void Update()
        {
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
            if (currentHype != null && currentHype.Albums >= albumsRequiredForWave)
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

            if (CurrentWave % 5 == 0)
            {
                // Boss defeated!
                ShowBanner("👑 БОСС ПОВЕРЖЕН!", $"+{waveReward} монет! Подготовка к выбору перка...");
            }
            else
            {
                ShowBanner($"ВОЛНА {CurrentWave} ПРОЙДЕНА!", $"+{waveReward} монет! Выберите перк...");
            }

            Invoke(nameof(OpenPerkSelection), 1.2f);
        }

        private void OpenPerkSelection()
        {
            Time.timeScale = 0f;
            PresentPerkModal();
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
            ShowBanner("⚠️ ВНИМАНИЕ: БОСС ЭКСПЕДИЦИИ!", "Уничтожьте флагман Синдиката для завершения сектора!");
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
            waveHudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bannerText.fontSize = 32;
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontStyle = FontStyle.Bold;
            bannerText.color = new Color(1f, 0.85f, 0.35f);
            bannerText.gameObject.SetActive(false);

            BuildPerkModalRoot(canvasObj.transform);
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
            var capStr = CoinsEarnedThisRun >= MaxCoinsPerRun ? "<color=#ff5555>[ЛИМИТ]</color>" : "";
            waveHudText.text = $"🌀 ЭКСПЕДИЦИЯ // ВОЛНА: {CurrentWave}   |   🪙 {CoinsEarnedThisRun}/{MaxCoinsPerRun} {capStr}";
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
            tText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tText.fontSize = 28;
            tText.text = "⚡ ВЫБЕРИТЕ УЛУЧШЕНИЕ ЭКСПЕДИЦИИ ⚡";
            tText.fontStyle = FontStyle.Bold;
            tText.alignment = TextAnchor.MiddleCenter;
            tText.color = new Color(1f, 0.85f, 0.35f);
            tText.rectTransform.anchoredPosition = new Vector2(0f, 210f);
            tText.rectTransform.sizeDelta = new Vector2(800f, 44f);

            var subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            subObj.transform.SetParent(panelObj.transform, false);
            var sText = subObj.GetComponent<Text>();
            sText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sText.fontSize = 17;
            sText.text = $"Волна {CurrentWave} зачищена. Установите квантовый апгрейд на тарелку Зигги:";
            sText.alignment = TextAnchor.MiddleCenter;
            sText.color = new Color(0.8f, 0.92f, 1f);
            sText.rectTransform.anchoredPosition = new Vector2(0f, 168f);
            sText.rectTransform.sizeDelta = new Vector2(800f, 32f);

            perkModalObj.SetActive(false);
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

            var btn = cardObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                ApplyPerk(perk);
            });

            // Icon Emoji
            CreateLabel(cardObj.transform, "Icon", perk.IconEmoji, 44, new Vector2(0f, 95f), new Vector2(220f, 60f), perk.ThemeColor, FontStyle.Bold);

            // Title
            CreateLabel(cardObj.transform, "Title", perk.Title, 19, new Vector2(0f, 40f), new Vector2(230f, 44f), perk.ThemeColor, FontStyle.Bold);

            // Subtitle
            CreateLabel(cardObj.transform, "Sub", perk.Subtitle, 13, new Vector2(0f, 10f), new Vector2(230f, 24f), new Color(0.6f, 0.85f, 1f), FontStyle.Normal);

            // Description
            CreateLabel(cardObj.transform, "Desc", perk.Description, 14, new Vector2(0f, -50f), new Vector2(220f, 80f), new Color(0.9f, 0.95f, 1f), FontStyle.Normal);

            // Select Button inside card
            var pickBtnObj = new GameObject("PickBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            pickBtnObj.transform.SetParent(cardObj.transform, false);
            var pickRect = pickBtnObj.GetComponent<RectTransform>();
            pickRect.anchoredPosition = new Vector2(0f, -118f);
            pickRect.sizeDelta = new Vector2(210f, 40f);
            var pickImg = pickBtnObj.GetComponent<Image>();
            pickImg.sprite = PixelUiAtlas.ButtonPrimary;
            pickImg.type = Image.Type.Sliced;
            CreateLabel(pickBtnObj.transform, "Label", "ВЫБРАТЬ", 16, Vector2.zero, new Vector2(200f, 34f), Color.white, FontStyle.Bold);
        }

        private void ApplyPerk(RoguePerk perk)
        {
            var player = FindAnyObjectByType<PlayerHealth>();
            perk.OnApply?.Invoke(player);
            ActivePerkTitles.Add(perk.Title);

            if (perkModalObj != null)
                perkModalObj.SetActive(false);

            Time.timeScale = 1f;
            StartWave(CurrentWave + 1);
        }

        private List<RoguePerk> GetAvailablePerks()
        {
            return new List<RoguePerk>
            {
                new RoguePerk("dmg", "Тяжелый Заряд", "ОРУЖИЕ", "+1 к урону всех выстрелов тарелки.", "💥", new Color(1f, 0.5f, 0.2f), p => BonusDamage += 1),
                new RoguePerk("rate", "Форсаж Плазмы", "СКОРОСТЬ СТРЕЛЬБЫ", "+20% к темпу стрельбы.", "⚡", new Color(0.3f, 0.9f, 1f), p => FireRateMultiplier *= 1.2f),
                new RoguePerk("armor", "Нано-Броня", "ЗАЩИТА", "+1 макс. прочность и полное исцеление тарелки.", "🛡️", new Color(0.3f, 1f, 0.5f), p => p?.IncreaseMaxLives(1)),
                new RoguePerk("speed", "Ионный Форсаж", "МАНЕВРЕННОСТЬ", "+25% к скорости перемещения тарелки.", "🚀", new Color(1f, 0.85f, 0.3f), p => MoveSpeedMultiplier *= 1.25f),
                new RoguePerk("hype", "Hype-Резонатор", "СПЕЦ-АТАКИ", "+40% к скорости накопления карт Hype.", "🌀", new Color(0.9f, 0.4f, 1f), p => HypeMultiplier *= 1.4f),
                new RoguePerk("magnet", "Грави-Магнит", "РЕСУРСЫ", "Притягивает наниты и сферы с удвоенного расстояния.", "🧲", new Color(0.4f, 0.8f, 1f), p => MagnetRangeBonus += 3.5f)
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
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        #endregion
    }
}
