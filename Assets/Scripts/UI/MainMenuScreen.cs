// Assets/Scripts/UI/MainMenuScreen.cs
// Namespace: KANYE.UI
// GameObject: MainMenuScreen in MainMenu scene

using System.Collections.Generic;
using KANYE.Audio;
using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        private const string VolumeKey = "KANYE.MasterVolume";

        // Dynamic Text Labels
        private Text rankText;
        private Text coinsText;
        private Text playerNameText;
        private Button authButton;
        private GameObject firstLaunchModal;
        private Text firstLaunchNameText;
        private Text sectorTitleText;
        private Text sectorDescText;
        private Text shipSkinTitleText;
        private Image shipDisplayImage;
        private Transform shipDisplayTransform;
        private Transform thrusterFlameTransform;
        private RectTransform holoRingTransform;
        private RectTransform holoScannerTransform;
        private RectTransform jackpotAuraTransform;
        private readonly List<RectTransform> pulsingRewardIcons = new List<RectTransform>();

        // Modals
        private GameObject shopModal;
        private GameObject achievementsModal;
        private GameObject settingsModal;
        private GameObject loreModal;
        private GameObject dailyRewardModal;
        private RectTransform achievementScrollContent;
        private readonly List<Button> skinActionButtons = new List<Button>();
        private readonly List<Text> skinActionLabels = new List<Text>();

        private Sprite shipSprite;
        private Sprite thrusterSprite;

        private void Awake()
        {
            EnsureEventSystem();
            LoadVisualAssets();
            BuildPixelMenu();
        }

        private void OnEnable()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed += RefreshData;
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.Changed += RefreshData;
            if (YandexGamesService.Instance != null)
                YandexGamesService.Instance.Changed += RefreshData;

            RefreshData();
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= RefreshData;
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.Changed -= RefreshData;
            if (YandexGamesService.Instance != null)
                YandexGamesService.Instance.Changed -= RefreshData;
        }

        private void Update()
        {
            var time = Time.unscaledTime;

            // Floating hover + smooth banking pitch tilt in the hangar bay
            if (shipDisplayTransform != null)
            {
                var hover = Mathf.Sin(time * 2.2f) * 8f;
                var tilt = Mathf.Sin(time * 1.8f) * 3.2f;
                shipDisplayTransform.localPosition = new Vector3(-350f, -30f + hover, 0f);
                shipDisplayTransform.localEulerAngles = new Vector3(0f, 0f, tilt);
            }

            // Engine thruster flame flicker & spark particle trail
            if (thrusterFlameTransform != null)
            {
                var flicker = 0.9f + 0.35f * Mathf.Sin(time * 24f);
                thrusterFlameTransform.localScale = new Vector3(flicker * 1.35f, flicker * 0.92f, 1f);

                // Periodic thruster spark emission
                if (MenuParticleSystem.Instance != null && Time.frameCount % 5 == 0)
                {
                    var yOffset = Mathf.Sin(time * 2.2f) * 8f;
                    MenuParticleSystem.Instance.EmitThrusterEmber(new Vector2(-410f, -30f + yOffset), new Color(0.25f, 0.85f, 1f, 0.85f));
                }
            }

            // Holographic platform pulsating
            if (holoRingTransform != null)
            {
                var p = 1f + 0.05f * Mathf.Sin(time * 3f);
                holoRingTransform.localScale = new Vector3(p, p, 1f);
            }

            // Hologram scanner beam moving vertically across the ship
            if (holoScannerTransform != null)
            {
                var scanY = Mathf.PingPong(time * 65f, 70f) - 35f;
                holoScannerTransform.anchoredPosition = new Vector2(0f, scanY);
            }

            // Daily reward modal jackpot rotating aura and pulses
            if (dailyRewardModal != null && dailyRewardModal.activeInHierarchy)
            {
                if (jackpotAuraTransform != null)
                    jackpotAuraTransform.Rotate(0f, 0f, -40f * Time.unscaledDeltaTime);

                var pulse = 1f + 0.07f * Mathf.Sin(time * 4f);
                for (var i = 0; i < pulsingRewardIcons.Count; i++)
                {
                    if (pulsingRewardIcons[i] != null)
                        pulsingRewardIcons[i].localScale = Vector3.one * pulse;
                }
            }
        }

        private void LoadVisualAssets()
        {
            shipSprite = Resources.Load<Sprite>("Sprites/ship");
            if (shipSprite == null || shipSprite.rect.width <= 2)
                shipSprite = PlaceholderShipVisual.CreateFallbackShipSprite();
            thrusterSprite = Resources.Load<Sprite>("Sprites/thruster");
        }

        private void BuildPixelMenu()
        {
            var canvas = CreateCanvas("PixelMenuCanvas", 50);

            // 1. Fullscreen 16-bit Pixel Cyberpunk Background
            var bgSprite = PixelUiAtlas.Background;
            var bgObj = CreateImage("BackgroundArt", canvas.transform, bgSprite, Color.white);
            Stretch(bgObj.rectTransform);
            bgObj.type = Image.Type.Simple;

            // Subtle dark overlay to ensure maximum text readability
            var dimObj = CreateImage("AtmosphereDim", canvas.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.07f, 0.40f));
            Stretch(dimObj.rectTransform);

            // Ambient Space Dust & Interactive UI Particle System
            MenuParticleSystem.Create(canvas);

            // 2. Top Header Navigation Bar (Pilot dossier & Currency)
            BuildTopHeader(canvas.transform);

            // 3. Left Section - Hero Hangar & Ship Showcase
            BuildHangarShowcase(canvas.transform);

            // 4. Right Section - Main Command Buttons
            BuildCommandConsole(canvas.transform);

            // 5. Modals (Shop, Achievements, Settings, Lore, Daily Reward)
            BuildShopModal(canvas.transform);
            BuildAchievementsModal(canvas.transform);
            BuildSettingsModal(canvas.transform);
            BuildLoreModal(canvas.transform);
            BuildDailyRewardModal(canvas.transform);
            BuildFirstLaunch(canvas.transform);

            CloseAllModals();
        }

        private void BuildTopHeader(Transform parent)
        {
            var header = CreateImage("TopHeader", parent, PixelUiAtlas.White, new Color(0.04f, 0.07f, 0.12f, 0.88f));
            var rect = header.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 68f);

            // Cyan bottom border line (1-pixel cyber hairline)
            var border = CreateImage("HeaderLine", header.transform, PixelUiAtlas.White, new Color(0.2f, 0.75f, 1f, 0.85f));
            border.rectTransform.anchorMin = new Vector2(0f, 0f);
            border.rectTransform.anchorMax = new Vector2(1f, 0f);
            border.rectTransform.pivot = new Vector2(0.5f, 0f);
            border.rectTransform.sizeDelta = new Vector2(0f, 2f);

            // Left: Pilot dossier badge
            rankText = CreateText(header.transform, "RankBadge", "ПИЛОТ  •  0 СБИТО", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(450f, 36f), 19, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            rankText.alignment = TextAnchor.MiddleLeft;

            // Right: Credits / Currency badge
            coinsText = CreateText(header.transform, "CoinsBadge", "0 КРЕДИТОВ", new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(300f, 36f), 19, Color.white, FontStyle.Bold);
            coinsText.alignment = TextAnchor.MiddleRight;

            var dailyBtn = CreateSlicedButton(header.transform, "BtnDaily", "🎁 НАГРАДА ДНЯ", PixelUiAtlas.ButtonPrimary, new Vector2(0f, 0f), new Vector2(210f, 38f), 15, new Color(1f, 0.92f, 0.35f));
            var dailyRect = dailyBtn.GetComponent<RectTransform>();
            dailyRect.anchorMin = new Vector2(1f, 0.5f);
            dailyRect.anchorMax = new Vector2(1f, 0.5f);
            dailyRect.pivot = new Vector2(1f, 0.5f);
            dailyRect.anchoredPosition = new Vector2(-360f, 0f);
            dailyBtn.onClick.AddListener(() => OpenModal(dailyRewardModal));

            playerNameText = CreateText(header.transform, "PlayerName", "ПИЛОТ: ГОСТЬ", new Vector2(0.5f, 0.5f), new Vector2(52f, 0f), new Vector2(360f, 32f), 18, new Color(0.65f, 0.95f, 1f), FontStyle.Bold);
            var nameRect = playerNameText.rectTransform;
            nameRect.pivot = new Vector2(1f, 0.5f);
            nameRect.anchoredPosition = new Vector2(52f, 0f);
            playerNameText.alignment = TextAnchor.MiddleRight;

            authButton = CreateSlicedButton(header.transform, "BtnAuth", "ВОЙТИ", PixelUiAtlas.ButtonSecondary, new Vector2(148f, 0f), new Vector2(150f, 36f), 16, Color.white);
            var authRect = authButton.GetComponent<RectTransform>();
            authRect.anchorMin = new Vector2(0.5f, 0.5f);
            authRect.anchorMax = new Vector2(0.5f, 0.5f);
            authButton.onClick.AddListener(() => YandexGamesService.Instance?.RequestAuthorization());
        }

        private void BuildHangarShowcase(Transform parent)
        {
            // Game Logo Artwork (Pixel Art Arcade Logo)
            var logoSprite = PixelUiAtlas.Logo;
            var logoObj = CreateImage("GameLogo", parent, logoSprite, Color.white);
            var logoRect = logoObj.rectTransform;
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.pivot = new Vector2(0.5f, 1f);
            logoRect.anchoredPosition = new Vector2(-350f, -90f);
            logoRect.sizeDelta = new Vector2(520f, 140f);
            logoObj.preserveAspect = true;

            if (logoSprite == null)
            {
                CreateText(logoObj.transform, "FallbackLogo", "ASTRO K\n<size=20><color=#38e8ff>FLIGHT HOME</color></size>", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 120f), 44, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            }

            // Ship Platform / Hangar Bay
            var shipContainer = new GameObject("ShipHangarContainer", typeof(RectTransform));
            shipContainer.transform.SetParent(parent, false);
            shipDisplayTransform = shipContainer.transform;
            shipDisplayTransform.localPosition = new Vector3(-350f, -30f, 0f);

            // Holographic glow ring under the ship
            var holoRing = CreateImage("HoloRing", shipContainer.transform, PixelUiAtlas.SubPanel, new Color(0.2f, 0.8f, 1f, 0.35f));
            holoRing.type = Image.Type.Sliced;
            holoRing.rectTransform.anchoredPosition = new Vector2(0f, -65f);
            holoRing.rectTransform.sizeDelta = new Vector2(280f, 22f);
            holoRingTransform = holoRing.rectTransform;

            // Floating Ship Sprite (Original sleek saucer facing right)
            var shipObj = CreateImage("ShipVisual", shipContainer.transform, shipSprite, Color.white);
            shipObj.rectTransform.anchoredPosition = new Vector2(10f, 0f);
            shipObj.rectTransform.sizeDelta = new Vector2(160f, 110f);
            shipObj.rectTransform.localEulerAngles = Vector3.zero;
            shipObj.preserveAspect = true;
            shipDisplayImage = shipObj;

            // Hologram laser scanline sweeping vertically across the ship
            var scannerLine = CreateImage("HoloScanner", shipObj.transform, PixelUiAtlas.White, new Color(0f, 0.95f, 1f, 0.45f));
            var scRect = scannerLine.rectTransform;
            scRect.anchorMin = new Vector2(0.5f, 0.5f);
            scRect.anchorMax = new Vector2(0.5f, 0.5f);
            scRect.pivot = new Vector2(0.5f, 0.5f);
            scRect.sizeDelta = new Vector2(180f, 2.5f);
            holoScannerTransform = scRect;

            // Dynamic Thruster Flame (Attached to engine exhaust at the rear left)
            var flameObj = CreateImage("ThrusterFlame", shipObj.transform, thrusterSprite, new Color(0.3f, 0.85f, 1f, 0.95f));
            var flameRect = flameObj.rectTransform;
            flameRect.anchorMin = new Vector2(0f, 0.5f);
            flameRect.anchorMax = new Vector2(0f, 0.5f);
            flameRect.pivot = new Vector2(1f, 0.5f);
            flameRect.anchoredPosition = new Vector2(-4f, 0f);
            flameRect.sizeDelta = new Vector2(55f, 32f);
            flameRect.localEulerAngles = Vector3.zero;
            thrusterFlameTransform = flameRect;

            // Skin Name Label
            shipSkinTitleText = CreateText(shipContainer.transform, "SkinTitle", "КОРПУС: БАЗОВЫЙ", new Vector2(0.5f, 0.5f), new Vector2(0f, -108f), new Vector2(420f, 52f), 16, new Color(0.75f, 0.88f, 1f), FontStyle.Bold);

            // Current Mission Sector Card (Sliced pixel panel - NO distorted borders)
            var sectorCard = CreateImage("SectorCard", parent, PixelUiAtlas.Panel, Color.white);
            sectorCard.type = Image.Type.Sliced;
            var secRect = sectorCard.rectTransform;
            secRect.anchorMin = new Vector2(0.5f, 0f);
            secRect.anchorMax = new Vector2(0.5f, 0f);
            secRect.pivot = new Vector2(0.5f, 0f);
            secRect.anchoredPosition = new Vector2(-350f, 40f);
            secRect.sizeDelta = new Vector2(560f, 145f);

            sectorTitleText = CreateText(sectorCard.transform, "Title", "СЕКТОР 01: НЕОНОВЫЙ КОРИДОР", new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(520f, 30f), 20, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            sectorDescText = CreateText(sectorCard.transform, "Desc", "Astro K выходит из доков Neon City. Пройди маршрут сектора, чтобы уйти в гиперпространство.", new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(500f, 60f), 16, new Color(0.8f, 0.9f, 1f), FontStyle.Normal);
        }

        private void BuildCommandConsole(Transform parent)
        {
            var menuRoot = new GameObject("MenuConsole", typeof(RectTransform));
            menuRoot.transform.SetParent(parent, false);
            var rect = menuRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(360f, -30f);
            rect.sizeDelta = new Vector2(520f, 540f);

            // Primary Hero Buttons - CAMPAIGN & ROGUELITE EXPEDITION
            var campaignBtn = CreateSlicedButton(menuRoot.transform, "BtnPlay", "БОЕВАЯ КАМПАНИЯ", PixelUiAtlas.ButtonPrimary, new Vector2(0f, 185f), new Vector2(480f, 62f), 22, new Color(1f, 0.98f, 0.85f));
            campaignBtn.onClick.AddListener(OnPlayClicked);

            var rogueBtn = CreateSlicedButton(menuRoot.transform, "BtnRogue", "ЭКСПЕДИЦИЯ В БЕЗДНУ", PixelUiAtlas.ButtonPrimary, new Vector2(0f, 115f), new Vector2(480f, 62f), 22, new Color(0.85f, 0.95f, 1f));
            rogueBtn.onClick.AddListener(OnRogueClicked);

            // Secondary Navigation Buttons (Sliced cyber-blue arcade buttons)
            var hangarBtn = CreateSlicedButton(menuRoot.transform, "BtnHangar", "АНГАР И АРСЕНАЛ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, 50f), new Vector2(480f, 52f), 18, Color.white);
            hangarBtn.onClick.AddListener(() => SceneLoader.LoadSceneByName(SceneLoader.ShopScene));

            var loreBtn = CreateSlicedButton(menuRoot.transform, "BtnLore", "БОРТОВОЙ ЖУРНАЛ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -8f), new Vector2(480f, 50f), 18, new Color(0.9f, 0.95f, 1f));
            loreBtn.onClick.AddListener(() => OpenModal(loreModal));

            var achBtn = CreateSlicedButton(menuRoot.transform, "BtnAchievements", "ДОСТИЖЕНИЯ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -64f), new Vector2(480f, 50f), 18, Color.white);
            achBtn.onClick.AddListener(() => OpenModal(achievementsModal));

            var settingsBtn = CreateSlicedButton(menuRoot.transform, "BtnSettings", "НАСТРОЙКИ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -120f), new Vector2(480f, 50f), 18, Color.white);
            settingsBtn.onClick.AddListener(() => OpenModal(settingsModal));

#if !UNITY_WEBGL || UNITY_EDITOR
            var quitBtn = CreateSlicedButton(menuRoot.transform, "BtnQuit", "ВЫХОД ИЗ ИГРЫ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -176f), new Vector2(480f, 48f), 17, new Color(0.95f, 0.75f, 0.8f));
            quitBtn.onClick.AddListener(SceneLoader.Quit);
#endif
        }

        #region Modals Implementation

        private void BuildShopModal(Transform parent)
        {
            shopModal = CreateModalContainer(parent, "ShopModal", out var card);
            CreateText(card.transform, "Title", "КОСМИЧЕСКИЙ АНГАР // СКИНОТЕКА", new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(740f, 38f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            CreateText(card.transform, "Sub", "Облик меняет цвет и стиль корабля, не влияя на боевые параметры.", new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(740f, 26f), 17, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);

            for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
            {
                var skin = ShipSkinCatalog.All[i];
                var row = CreateImage("SkinRow" + i, card.transform, PixelUiAtlas.SubPanel, Color.white);
                row.type = Image.Type.Sliced;
                var rowRect = row.rectTransform;
                rowRect.anchorMin = new Vector2(0.5f, 1f);
                rowRect.anchorMax = new Vector2(0.5f, 1f);
                rowRect.pivot = new Vector2(0.5f, 1f);
                rowRect.anchoredPosition = new Vector2(0f, -125f - i * 110f);
                rowRect.sizeDelta = new Vector2(780f, 92f);

                // Color Preview Box
                var iconBox = CreateImage("SkinIcon", row.transform, shipSprite, skin.Color);
                iconBox.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                iconBox.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                iconBox.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                iconBox.rectTransform.anchoredPosition = new Vector2(50f, 0f);
                iconBox.rectTransform.sizeDelta = new Vector2(52f, 40f);
                iconBox.rectTransform.localEulerAngles = Vector3.zero;
                iconBox.preserveAspect = true;

                CreateText(row.transform, "Name", skin.Title, new Vector2(0f, 0.5f), new Vector2(125f, 14f), new Vector2(360f, 28f), 20, Color.white, FontStyle.Bold).alignment = TextAnchor.MiddleLeft;
                var costStr = skin.Price == 0 ? "Базовый корпус" : $"Стоимость: {skin.Price} монет";
                CreateText(row.transform, "Price", costStr, new Vector2(0f, 0.5f), new Vector2(125f, -16f), new Vector2(360f, 24f), 17, new Color(1f, 0.85f, 0.4f), FontStyle.Normal).alignment = TextAnchor.MiddleLeft;

                var btn = CreateSlicedButton(row.transform, "Btn" + i, "Выбрать", PixelUiAtlas.ButtonSecondary, new Vector2(270f, 0f), new Vector2(190f, 52f), 19, Color.white);
                skinActionButtons.Add(btn);
                skinActionLabels.Add(btn.GetComponentInChildren<Text>());

                var s = skin;
                btn.onClick.AddListener(() => BuyOrEquipSkin(s));
            }

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildAchievementsModal(Transform parent)
        {
            achievementsModal = CreateModalContainer(parent, "AchievementsModal", out var card);
            CreateText(card.transform, "Title", "БОРТОВЫЕ ДОСТИЖЕНИЯ", new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(740f, 38f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);

            var scroll = CreateScroll(card.transform, new Vector2(0f, -95f), new Vector2(800f, 500f));
            achievementScrollContent = scroll;

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildSettingsModal(Transform parent)
        {
            settingsModal = CreateModalContainer(parent, "SettingsModal", out var card);
            CreateText(card.transform, "Title", "НАСТРОЙКИ СИСТЕМЫ", new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(740f, 38f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);

            // Volume Section (Clean sliced subpanel)
            var volBox = CreateImage("VolBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            volBox.type = Image.Type.Sliced;
            volBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            volBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            volBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            volBox.rectTransform.anchoredPosition = new Vector2(0f, -95f);
            volBox.rectTransform.sizeDelta = new Vector2(780f, 95f);

            CreateText(volBox.transform, "VolCaption", "ГРОМКОСТЬ ЗВУКА И САУНДТРЕКА", new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(700f, 26f), 19, Color.white, FontStyle.Bold);
            CreateSlider(volBox.transform, new Vector2(0f, -56f));

            // Controls Guide Section (Clean sliced subpanel)
            var ctrlBox = CreateImage("CtrlBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            ctrlBox.type = Image.Type.Sliced;
            ctrlBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.anchoredPosition = new Vector2(0f, -205f);
            ctrlBox.rectTransform.sizeDelta = new Vector2(780f, 175f);

            CreateText(ctrlBox.transform, "CtrlCaption", "БОРТОВОЕ УПРАВЛЕНИЕ (CUPHEAD & BROFORCE EDITION)", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(700f, 26f), 19, new Color(0.35f, 0.85f, 1f), FontStyle.Bold);
            CreateText(ctrlBox.transform, "L1", "WASD / Стрелки / Сенсорный стик — Маневрирование корабля", new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(720f, 24f), 16, Color.white, FontStyle.Normal);
            CreateText(ctrlBox.transform, "L2", "ПРОБЕЛ / Огонь — Основной скорострельный залп", new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(720f, 24f), 16, Color.white, FontStyle.Normal);
            CreateText(ctrlBox.transform, "L3", "КЛАВИША [Q] — EX-ракета (тратит 1 карту энергии)", new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(720f, 24f), 16, new Color(0.4f, 0.85f, 1f), FontStyle.Normal);
            CreateText(ctrlBox.transform, "L4", "КЛАВИША [E] — Мега-Лазер (полный супер-залп при полной шкале Ярости)", new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(720f, 24f), 16, new Color(1f, 0.85f, 0.4f), FontStyle.Normal);
            CreateText(ctrlBox.transform, "L5", "⚡ ПАРИРОВАНИЕ: Сближайся с РОЗОВЫМИ сферами для поглощения и +30 Ярости!", new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(720f, 22f), 15, new Color(1f, 0.45f, 0.85f), FontStyle.Bold);

            // Reset Section - KEPT PROMINENTLY IN SETTINGS AS REQUESTED!
            var resetBox = CreateImage("ResetBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            resetBox.type = Image.Type.Sliced;
            resetBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            resetBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchoredPosition = new Vector2(0f, -495f);
            resetBox.rectTransform.sizeDelta = new Vector2(780f, 115f);

            CreateText(resetBox.transform, "Warn", "СБРОС ПРОГРЕССА И ПОВТОР ОБУЧЕНИЯ", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(700f, 24f), 17, new Color(1f, 0.45f, 0.45f), FontStyle.Bold);
            CreateText(resetBox.transform, "WarnDesc", "Очистит сохранения, открытые скины и рекорды, и запустит стартовое обучение пилота.", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(720f, 26f), 14, new Color(0.9f, 0.8f, 0.85f), FontStyle.Normal);

            var resetBtn = CreateSlicedButton(resetBox.transform, "BtnReset", "СБРОСИТЬ И НАЧАТЬ ОБУЧЕНИЕ", PixelUiAtlas.ButtonDanger, new Vector2(0f, -78f), new Vector2(400f, 40f), 16, Color.white);
            resetBtn.onClick.AddListener(ResetProgressAndStartTutorial);

            // Yandex Ads Testing Section
            var adBox = CreateImage("AdTestBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            adBox.type = Image.Type.Sliced;
            adBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            adBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            adBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            adBox.rectTransform.anchoredPosition = new Vector2(0f, -390f);
            adBox.rectTransform.sizeDelta = new Vector2(780f, 95f);

            CreateText(adBox.transform, "AdCaption", "ПРОВЕРКА РЕКЛАМЫ (SDK ЯНДЕКС ИГРЫ)", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(700f, 24f), 17, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);

            var testInterBtn = CreateSlicedButton(adBox.transform, "BtnTestInter", "МЕЖСТРАНИЧНАЯ РЕКЛАМА", PixelUiAtlas.ButtonPrimary, new Vector2(-190f, -54f), new Vector2(350f, 40f), 15, Color.white);
            testInterBtn.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                YandexGamesService.Instance?.ShowInterstitial(() =>
                {
                    Debug.Log("[AdTest] Interstitial completed successfully.");
                });
            });

            var testRewardBtn = CreateSlicedButton(adBox.transform, "BtnTestReward", "ВИДЕО ЗА НАГРАДУ (+100¢)", PixelUiAtlas.ButtonPrimary, new Vector2(190f, -54f), new Vector2(350f, 40f), 15, Color.white);
            testRewardBtn.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                YandexGamesService.Instance?.ShowRewarded(() =>
                {
                    SaveManager.Instance?.AddCoins(100);
                    RefreshData();
                    Debug.Log("[AdTest] Rewarded ad watched: +100 coins granted.");
                });
            });

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildLoreModal(Transform parent)
        {
            loreModal = CreateModalContainer(parent, "LoreModal", out var card);
            var loreTitle = CreateText(card.transform, "Title", "БОРТОВОЙ ЖУРНАЛ", new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(780f, 34f), 28, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            loreTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            loreTitle.verticalOverflow = VerticalWrapMode.Truncate;
            var loreSub = CreateText(card.transform, "Sub", "Хроники Капитана Зигги, досье боссов Синдиката и тайны реликвий Hype Core", new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(780f, 36f), 16, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);
            loreSub.verticalOverflow = VerticalWrapMode.Truncate;

            var scroll = CreateScroll(card.transform, new Vector2(0f, -112f), new Vector2(800f, 470f));
            var entries = LoreCatalog.Entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var lines = Mathf.Max(2, Mathf.CeilToInt(entry.Content.Length / 72f));
                var bodyHeight = lines * 26f;
                var itemObj = CreateImage("LoreItem" + i, scroll, PixelUiAtlas.SubPanel, Color.white);
                itemObj.type = Image.Type.Sliced;
                var le = itemObj.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 92f + bodyHeight;
                le.preferredHeight = 92f + bodyHeight;

                var tag = PlaceLoreLine(itemObj.transform, "Tag", $"[{entry.Category}]", 12f, 22f, 15, new Color(0.55f, 0.9f, 1f), FontStyle.Bold);
                var title = PlaceLoreLine(itemObj.transform, "Title", entry.Title, 38f, 28f, 20, new Color(1f, 0.88f, 0.4f), FontStyle.Bold);
                var content = PlaceLoreLine(itemObj.transform, "Content", entry.Content, 72f, bodyHeight, 17, new Color(0.9f, 0.94f, 0.98f), FontStyle.Normal);
                tag.alignment = TextAnchor.MiddleLeft;
                title.alignment = TextAnchor.MiddleLeft;
                content.alignment = TextAnchor.UpperLeft;
                content.lineSpacing = 1.12f;
                content.verticalOverflow = VerticalWrapMode.Overflow;
            }

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(280f, 54f), 19, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private GameObject CreateModalContainer(Transform parent, string name, out GameObject cardObj)
        {
            var modal = new GameObject(name, typeof(RectTransform));
            modal.transform.SetParent(parent, false);
            Stretch(modal.GetComponent<RectTransform>());

            // Dim background that closes modal on tap
            var dim = CreateImage("ModalDim", modal.transform, PixelUiAtlas.White, new Color(0f, 0f, 0f, 0.75f));
            Stretch(dim.rectTransform);
            var dimBtn = dim.gameObject.AddComponent<Button>();
            dimBtn.onClick.AddListener(CloseAllModals);

            // Dialog Window Frame using Sliced Pixel Panel (Zero distortion)
            var card = CreateImage("CardFrame", modal.transform, PixelUiAtlas.Panel, Color.white);
            card.type = Image.Type.Sliced;
            var rect = card.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(860f, 740f);

            cardObj = card.gameObject;
            return modal;
        }

        private void OpenModal(GameObject modal)
        {
            CloseAllModals();
            if (modal != null)
            {
                modal.SetActive(true);
                UITween.FadeInModal(this, modal);
            }
        }

        private void BuildDailyRewardModal(Transform parent)
        {
            dailyRewardModal = CreateModalContainer(parent, "DailyRewardModal", out var card);
            CreateText(card.transform, "Title", "🎁 ЕЖЕДНЕВНЫЙ БОНУС ПИЛОТА 🎁", new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(740f, 38f), 30, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            CreateText(card.transform, "Sub", "Заходи в игру каждый день, чтобы получать кредиты и редкие Звёздные Ядра!", new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(740f, 26f), 16, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);

            var save = SaveManager.Instance;
            var currentDay = save != null ? save.GetCurrentDailyDay() : 1;
            var isAvailable = save != null && save.IsDailyRewardAvailable();
            pulsingRewardIcons.Clear();

            var cardsContainer = new GameObject("CardsGrid", typeof(RectTransform));
            cardsContainer.transform.SetParent(card.transform, false);
            var gridRect = cardsContainer.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 1f);
            gridRect.anchorMax = new Vector2(0.5f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.anchoredPosition = new Vector2(0f, -120f);
            gridRect.sizeDelta = new Vector2(800f, 220f);

            var startX = -345f;
            var gapX = 115f;

            for (var d = 1; d <= 7; d++)
            {
                var dayNum = d;
                var reward = save != null ? save.GetDailyRewardValues(dayNum) : new SaveManager.DailyRewardEntry(50, 0);
                var isCurrent = dayNum == currentDay;
                var isPast = dayNum < currentDay && !isAvailable;
                var isJackpot = dayNum == 7;

                var cardBox = CreateImage($"DayCard_{dayNum}", cardsContainer.transform, PixelUiAtlas.SubPanel, Color.white);
                cardBox.type = Image.Type.Sliced;
                var cRect = cardBox.rectTransform;
                cRect.anchorMin = new Vector2(0.5f, 0.5f);
                cRect.anchorMax = new Vector2(0.5f, 0.5f);
                cRect.pivot = new Vector2(0.5f, 0.5f);
                cRect.anchoredPosition = new Vector2(startX + (dayNum - 1) * gapX, 0f);
                cRect.sizeDelta = new Vector2(106f, 190f);

                if (isCurrent && isAvailable)
                    cardBox.color = isJackpot ? new Color(0.35f, 0.30f, 0.12f, 1f) : new Color(0.18f, 0.32f, 0.5f, 1f);
                else if (isPast)
                    cardBox.color = new Color(0.08f, 0.12f, 0.18f, 0.6f);
                else
                    cardBox.color = isJackpot ? new Color(0.20f, 0.16f, 0.08f, 0.95f) : new Color(0.08f, 0.14f, 0.22f, 0.95f);

                CreateText(cardBox.transform, "DayHeader", $"ДЕНЬ {dayNum}", new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(100f, 22f), 15, isCurrent ? new Color(1f, 0.85f, 0.3f) : (isJackpot ? new Color(1f, 0.8f, 0.4f) : new Color(0.65f, 0.85f, 1f)), FontStyle.Bold);

                if (isJackpot)
                {
                    CreateText(cardBox.transform, "JackpotLabel", "★ ДЖЕКПОТ ★", new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(104f, 14f), 10, new Color(1f, 0.85f, 0.25f), FontStyle.Bold);

                    // Rotating golden energy starburst aura behind the Jackpot chest
                    var aura = CreateImage("JackpotAura", cardBox.transform, PixelUiAtlas.IconStarCore, new Color(1f, 0.75f, 0.15f, 0.35f));
                    var aRect = aura.rectTransform;
                    aRect.anchorMin = new Vector2(0.5f, 0.5f);
                    aRect.anchorMax = new Vector2(0.5f, 0.5f);
                    aRect.pivot = new Vector2(0.5f, 0.5f);
                    aRect.anchoredPosition = new Vector2(0f, 6f);
                    aRect.sizeDelta = new Vector2(68f, 68f);
                    jackpotAuraTransform = aRect;
                }

                // Sprite icon for reward
                var iconSprite = isJackpot ? PixelUiAtlas.IconChestEpic : (reward.cores > 0 ? PixelUiAtlas.IconStarCore : PixelUiAtlas.IconCredit);
                var iconImg = CreateImage($"DayIcon_{dayNum}", cardBox.transform, iconSprite, Color.white);
                var iconRect = iconImg.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = new Vector2(0f, isJackpot ? 6f : 10f);
                iconRect.sizeDelta = isJackpot ? new Vector2(50f, 50f) : new Vector2(40f, 40f);
                iconImg.preserveAspect = true;

                if (isJackpot || reward.cores > 0 || (isCurrent && isAvailable))
                {
                    pulsingRewardIcons.Add(iconRect);
                }

                // Value description
                string rewardStr;
                if (isJackpot)
                    rewardStr = $"+{reward.credits} ¢\n+{reward.cores} ЯДРА";
                else if (reward.cores > 0)
                    rewardStr = $"+{reward.credits} ¢\n+{reward.cores} ЯДРО";
                else
                    rewardStr = $"+{reward.credits} ¢";

                CreateText(cardBox.transform, "Val", rewardStr, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(100f, 38f), 13, isJackpot ? new Color(1f, 0.88f, 0.35f) : new Color(1f, 0.95f, 0.65f), FontStyle.Bold);

                if (isPast)
                {
                    var overlay = CreateImage("PastOverlay", cardBox.transform, PixelUiAtlas.White, new Color(0f, 0f, 0f, 0.55f));
                    Stretch(overlay.rectTransform);
                    CreateText(cardBox.transform, "Check", "✓", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), 38, new Color(0.3f, 1f, 0.45f), FontStyle.Bold);
                }
            }

            // Claim Button
            var claimBtn = CreateSlicedButton(card.transform, "BtnClaimDaily", isAvailable ? "ЗАБРАТЬ НАГРАДУ" : "НАГРАДА УЖЕ ПОЛУЧЕНА", isAvailable ? PixelUiAtlas.ButtonPrimary : PixelUiAtlas.ButtonSecondary, new Vector2(0f, -380f), new Vector2(440f, 54f), 19, Color.white);
            claimBtn.interactable = isAvailable;
            claimBtn.onClick.AddListener(() =>
            {
                if (SaveManager.Instance != null && SaveManager.Instance.ClaimDailyReward(out var c, out var cr))
                {
                    AudioManager.PlayVictory();

                    // Explosive victory particle bursts
                    if (MenuParticleSystem.Instance != null)
                    {
                        MenuParticleSystem.Instance.EmitBurst(Vector2.zero, new Color(1f, 0.88f, 0.25f, 1f), 36, PixelUiAtlas.IconCredit);
                        MenuParticleSystem.Instance.EmitBurst(Vector2.zero, new Color(0.2f, 0.95f, 1f, 1f), 28, PixelUiAtlas.IconStarCore);
                    }

                    UITween.PunchScale(this, claimBtn.GetComponent<RectTransform>(), 1.18f, 0.25f);
                    claimBtn.interactable = false;
                    claimBtn.GetComponentInChildren<Text>().text = "НАГРАДА ПОЛУЧЕНА!";
                    RefreshData();
                }
            });

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -445f), new Vector2(240f, 44f), 17, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void CloseAllModals()
        {
            if (shopModal != null) shopModal.SetActive(false);
            if (achievementsModal != null) achievementsModal.SetActive(false);
            if (settingsModal != null) settingsModal.SetActive(false);
            if (loreModal != null) loreModal.SetActive(false);
            if (dailyRewardModal != null) dailyRewardModal.SetActive(false);
            if (firstLaunchModal != null) firstLaunchModal.SetActive(false);
        }

        #endregion

        #region Actions & Data Binding

        private void RefreshPlayerLine()
        {
            var sdk = YandexGamesService.Instance;
            var authorized = sdk != null && sdk.IsAuthorized;
            var name = authorized ? sdk.PlayerName : "Гость";
            if (playerNameText != null)
                playerNameText.text = authorized ? $"ПИЛОТ: {name.ToUpper()}" : "ПИЛОТ: ГОСТЬ";
            if (authButton != null)
                authButton.gameObject.SetActive(!authorized);
            if (firstLaunchNameText != null)
            {
                firstLaunchNameText.text = authorized
                    ? $"На связи {name}. Это имя пришло после авторизации."
                    : "Пока ты гость. Кнопка «Войти» в шапке запрашивает имя у Яндекс Игр.";
            }
        }

        private void BuildFirstLaunch(Transform parent)
        {
            firstLaunchModal = new GameObject("FirstLaunch", typeof(RectTransform));
            firstLaunchModal.transform.SetParent(parent, false);
            Stretch(firstLaunchModal.GetComponent<RectTransform>());

            var dim = CreateImage("Dim", firstLaunchModal.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.08f, 0.82f));
            Stretch(dim.rectTransform);

            var card = CreateImage("Card", firstLaunchModal.transform, PixelUiAtlas.Panel, Color.white);
            card.type = Image.Type.Sliced;
            var cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 420f);

            CreateText(card.transform, "Title", "ПЕРВЫЙ ВЫЛЕТ", new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(680f, 40f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            firstLaunchNameText = CreateText(card.transform, "Pilot", "Пока ты гость.", new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(680f, 28f), 18, new Color(0.65f, 0.95f, 1f), FontStyle.Normal);
            var body = CreateText(card.transform, "Body",
                "Сессия начинается с меню. Окно урока закроется только после манёвра, выстрела, ракеты на Q и мега-луча на E.\nДальше задание миссии — пройденный путь, а шкала Ярости копится от сбитых целей.",
                new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(660f, 110f), 20, Color.white, FontStyle.Normal);
            body.alignment = TextAnchor.UpperCenter;

            var go = CreateSlicedButton(card.transform, "BtnFirstFlight", "В ПЕРВЫЙ ВЫЛЕТ", PixelUiAtlas.ButtonPrimary, new Vector2(0f, -40f), new Vector2(420f, 58f), 22, new Color(1f, 0.98f, 0.85f));
            go.onClick.AddListener(StartCampaign);
            var stay = CreateSlicedButton(card.transform, "BtnStay", "ОСТАТЬСЯ В МЕНЮ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -110f), new Vector2(420f, 48f), 18, Color.white);
            stay.onClick.AddListener(() => firstLaunchModal.SetActive(false));

            firstLaunchModal.SetActive(false);
        }

        private void OnPlayClicked()
        {
            if (firstLaunchModal != null && !TutorialHints.IsTutorialCompleted)
            {
                AudioManager.PlayClick();
                firstLaunchModal.SetActive(true);
                return;
            }

            StartCampaign();
        }

        private void StartCampaign()
        {
            AudioManager.PlayClick();
            GameRules.CurrentMode = GameMode.Campaign;
            var index = SaveManager.Instance != null ? SaveManager.Instance.CurrentMission : 0;
            MissionCatalog.Select(index);

            SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
        }

        private void OnRogueClicked()
        {
            AudioManager.PlayClick();
            GameRules.CurrentMode = GameMode.RogueExpedition;
            SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
        }

        private void ResetProgressAndStartTutorial()
        {
            SaveManager.Instance?.ResetAllProgress();
            TutorialHints.ResetTutorial();
            MissionCatalog.Select(0);
            Debug.Log("[Dev] Прогресс сброшен. Немедленный старт обучения...");
            SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
        }

        private void BuyOrEquipSkin(ShipSkin skin)
        {
            var save = SaveManager.Instance;
            if (save == null) return;

            var owned = save.Data.unlockedSkinIds.Contains(skin.Id);
            if (!owned && skin.Price > 0 && !save.TrySpendCoins(skin.Price))
                return;

            if (!owned)
                save.UnlockSkin(skin.Id);

            save.EquipSkin(skin.Id);
            RefreshData();
        }

        private void RefreshData()
        {
            var save = SaveManager.Instance;
            var coins = save != null ? save.Data.coins : 0;
            var cores = save != null && save.Data != null ? save.Data.starCores : 0;
            var missionIndex = save != null ? save.CurrentMission : 0;
            var mission = MissionCatalog.All[Mathf.Clamp(missionIndex, 0, MissionCatalog.All.Length - 1)];
            var kills = save != null ? save.Data.totalKills : 0;
            var equippedSkinId = save != null ? save.Data.equippedSkinId : "default";
            var equippedSkin = ShipSkinCatalog.Find(equippedSkinId);

            if (coinsText != null)
                coinsText.text = $"{coins} ¢   •   {cores} ЯДЕР";
            if (rankText != null)
                rankText.text = $"{PilotRank.Title(kills).ToUpper()}  •  {kills} СБИТО";
            if (sectorTitleText != null)
                sectorTitleText.text = mission.Title.ToUpper();
            if (sectorDescText != null)
                sectorDescText.text = mission.Summary;
            if (shipSkinTitleText != null)
            {
                var weapon = WeaponCatalog.Find(save != null ? save.Data.equippedWeaponId : SaveManager.DefaultWeaponId);
                var weaponTitle = weapon != null ? weapon.Title : "Базовое оружие";
                shipSkinTitleText.text = $"КОРПУС: {equippedSkin.Title.ToUpper()}\nОРУЖИЕ: {weaponTitle.ToUpper()}";
            }

            RefreshPlayerLine();

            if (shipDisplayImage != null)
            {
                var sprite = Resources.Load<Sprite>(equippedSkin.SpriteName);
                if (sprite != null)
                    shipDisplayImage.sprite = sprite;
                shipDisplayImage.color = Color.white;
            }

            if (thrusterFlameTransform != null)
            {
                var flameImg = thrusterFlameTransform.GetComponent<Image>();
                if (flameImg != null)
                {
                    var tSpr = Resources.Load<Sprite>(equippedSkin.ThrusterSprite);
                    if (tSpr != null)
                        flameImg.sprite = tSpr;
                    flameImg.color = equippedSkin.ThrusterColor;
                }
            }

            // Update shop button states
            for (var i = 0; i < skinActionButtons.Count && i < ShipSkinCatalog.All.Length; i++)
            {
                var skin = ShipSkinCatalog.All[i];
                var owned = save != null && save.Data.unlockedSkinIds.Contains(skin.Id);
                var isEquipped = skin.Id == equippedSkinId;

                if (isEquipped)
                {
                    skinActionLabels[i].text = "НАДЕТ";
                    skinActionButtons[i].interactable = false;
                }
                else if (owned || skin.Price == 0)
                {
                    skinActionLabels[i].text = "ЭКИПИРОВАТЬ";
                    skinActionButtons[i].interactable = true;
                }
                else
                {
                    skinActionLabels[i].text = $"КУПИТЬ ({skin.Price})";
                    skinActionButtons[i].interactable = coins >= skin.Price;
                }
            }

            RebuildAchievementsList();
        }

        private void RebuildAchievementsList()
        {
            if (achievementScrollContent == null) return;

            for (var i = achievementScrollContent.childCount - 1; i >= 0; i--)
            {
                var child = achievementScrollContent.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            var manager = AchievementManager.Instance;
            if (manager == null || manager.Definitions.Count == 0)
                return;

            var list = new List<AchievementData>(manager.Definitions);
            list.Sort((a, b) => string.Compare(a.displayName, b.displayName, System.StringComparison.Ordinal));

            for (var i = 0; i < list.Count; i++)
            {
                var data = list[i];
                if (data == null) continue;

                var unlocked = manager.IsUnlocked(data.id);
                var progress = manager.GetProgress(data.id);
                var status = unlocked ? "ОТКРЫТО" : $"{progress} / {data.targetAmount}";

                var row = CreateImage("AchRow" + i, achievementScrollContent, PixelUiAtlas.SubPanel, Color.white);
                row.type = Image.Type.Sliced;
                var layout = row.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 88f;
                layout.preferredHeight = 88f;

                var accent = CreateImage("Accent", row.transform, PixelUiAtlas.White, unlocked ? new Color(0.3f, 1f, 0.6f, 1f) : new Color(0.25f, 0.4f, 0.6f, 0.6f));
                accent.rectTransform.anchorMin = new Vector2(0f, 0f);
                accent.rectTransform.anchorMax = new Vector2(0f, 1f);
                accent.rectTransform.pivot = new Vector2(0f, 0.5f);
                accent.rectTransform.sizeDelta = new Vector2(4f, 0f);

                var badge = unlocked ? "★ " : "○ ";
                var nameText = CreateText(row.transform, "Title", badge + data.displayName + "  —  [" + status + "]", new Vector2(0f, 1f), new Vector2(25f, -22f), new Vector2(720f, 28f), 19, unlocked ? new Color(0.7f, 1f, 0.75f) : Color.white, FontStyle.Bold);
                nameText.alignment = TextAnchor.MiddleLeft;

                var descText = CreateText(row.transform, "Desc", data.description, new Vector2(0f, 1f), new Vector2(25f, -54f), new Vector2(720f, 26f), 15, new Color(0.75f, 0.85f, 0.95f), FontStyle.Normal);
                descText.alignment = TextAnchor.MiddleLeft;
            }
        }

        #endregion

        #region UI Helpers

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static Canvas CreateCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return c;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite != null ? sprite : PixelUiAtlas.White;
            img.color = color;
            return img;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(anchor.x == 0f ? 0f : (anchor.x == 1f ? 1f : 0.5f), 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var label = go.GetComponent<Text>();
            label.font = UIFont.DefaultFont;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.lineSpacing = 1.08f;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            if (label.font != null && label.font.material != null && label.font.material.mainTexture != null)
                label.font.material.mainTexture.filterMode = FilterMode.Bilinear;
            return label;
        }

        private static Text PlaceLoreLine(Transform parent, string name, string value, float top, float height, int fontSize, Color color, FontStyle style)
        {
            var label = CreateText(parent, name, value, new Vector2(0f, 1f), Vector2.zero, new Vector2(760f, height), fontSize, color, style);
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-44f, height);
            return label;
        }

        private static Button CreateSlicedButton(Transform parent, string name, string text, Sprite sprite, Vector2 pos, Vector2 size, int fontSize, Color textColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = sprite != null ? sprite : PixelUiAtlas.ButtonSecondary;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;

            var label = CreateText(go.transform, "Label", text, new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize, textColor, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;

            UITween.AttachButtonFeedback(btn);
            return btn;
        }

        private static RectTransform CreateScroll(Transform parent, Vector2 pos, Vector2 size)
        {
            var scrollObj = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollObj.transform.SetParent(parent, false);
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.5f, 1f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 1f);
            scrollRect.anchoredPosition = pos;
            scrollRect.sizeDelta = size;
            scrollObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObj.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewport.transform, false);
            var content = contentObj.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = contentObj.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentObj.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollObj.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            return content;
        }

        private void CreateSlider(Transform parent, Vector2 pos)
        {
            var sliderObj = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            var rect = sliderObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(560f, 22f);

            var bg = CreateImage("Bg", sliderObj.transform, PixelUiAtlas.White, new Color(0.04f, 0.06f, 0.1f, 1f));
            Stretch(bg.rectTransform);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>());

            var fill = CreateImage("Fill", fillArea.transform, PixelUiAtlas.White, new Color(0.2f, 0.85f, 1f, 1f));
            Stretch(fill.rectTransform);

            var handle = CreateImage("Handle", sliderObj.transform, PixelUiAtlas.White, new Color(1f, 0.85f, 0.4f, 1f));
            handle.rectTransform.sizeDelta = new Vector2(18f, 30f);

            var slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = PlayerPrefs.GetFloat(VolumeKey, 1f);
            AudioListener.volume = slider.value;
            slider.onValueChanged.AddListener(val =>
            {
                AudioListener.volume = val;
                PlayerPrefs.SetFloat(VolumeKey, val);
                AudioManager.SetVolume(val);
            });
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
