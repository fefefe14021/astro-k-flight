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

        // Modals
        private GameObject shopModal;
        private GameObject achievementsModal;
        private GameObject settingsModal;
        private GameObject loreModal;
        private GameObject dailyRewardModal;
        private Transform dailyCardsContainer;
        private RectTransform achievementScrollContent;
        private readonly List<Button> skinActionButtons = new List<Button>();
        private readonly List<Text> skinActionLabels = new List<Text>();

        // Localized navigation button labels
        private Text btnCampaignLabel;
        private Text btnRogueLabel;
        private Text btnHangarLabel;
        private Text btnLoreLabel;
        private Text btnAchievementsLabel;
        private Text btnSettingsLabel;
        private Text btnAuthLabel;
        private Text btnDailyHeaderLabel;

        // Localized daily reward elements
        private Text dailyTitleText;
        private Text dailySubText;
        private Button dailyClaimBtn;
        private Text dailyClaimBtnText;
        private Text dailyCloseBtnText;
        private Transform jackpotAuraTransform;

        // Localized settings modal elements
        private Text settingsTitleText;
        private Text langCaptionText;
        private Text langButtonLabel;
        private Text volCaptionText;
        private Text ctrlCaptionText;
        private Text ctrlL1, ctrlL2, ctrlL3, ctrlL4, ctrlL5;
        private Text resetTitleText;
        private Text resetDescText;
        private Text resetBtnLabel;
        private Text closeBtnLabel;

        // Localized lore modal elements
        private Text loreTitleText;
        private Text loreSubText;
        private Text loreCutsceneBtnText;
        private Text loreCloseBtnText;
        private Transform loreScrollContent;

        // Localized achievements modal elements
        private Text achievementsTitleText;
        private Text achievementsCloseBtnLabel;

        // Localized first launch modal elements
        private Text firstLaunchTitleText;
        private Text firstLaunchBodyText;
        private Text firstLaunchGoBtnLabel;
        private Text firstLaunchStayBtnLabel;

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

            Localization.LanguageChanged += RefreshAllTexts;
            RefreshData();
            RefreshAllTexts();
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= RefreshData;
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.Changed -= RefreshData;
            if (YandexGamesService.Instance != null)
                YandexGamesService.Instance.Changed -= RefreshData;

            Localization.LanguageChanged -= RefreshAllTexts;
        }

        private void Update()
        {
            // Floating hover animation for the ship in the hangar bay
            if (shipDisplayTransform != null)
            {
                var hover = Mathf.Sin(Time.unscaledTime * 2.8f) * 10f;
                shipDisplayTransform.localPosition = new Vector3(-350f, -30f + hover, 0f);
            }

            // Engine thruster flame flicker
            if (thrusterFlameTransform != null)
            {
                var flicker = 0.9f + 0.35f * Mathf.Sin(Time.unscaledTime * 24f);
                thrusterFlameTransform.localScale = new Vector3(flicker * 1.3f, flicker * 0.9f, 1f);
            }

            // Jackpot daily reward icon pulse animation
            if (jackpotAuraTransform != null && dailyRewardModal != null && dailyRewardModal.activeInHierarchy)
            {
                var s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f);
                jackpotAuraTransform.localScale = new Vector3(s, s, 1f);
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
            rankText = CreateText(header.transform, "RankBadge", "ПИЛОТ // 0 СБИТО", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(450f, 36f), 20, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            rankText.alignment = TextAnchor.MiddleLeft;

            // Right: Credits / Currency badge
            coinsText = CreateText(header.transform, "CoinsBadge", "0 МОНЕТ", new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(300f, 36f), 20, Color.white, FontStyle.Bold);
            coinsText.alignment = TextAnchor.MiddleRight;

            var dailyBtn = CreateSlicedButton(header.transform, "BtnDaily", Localization.T("🎁 НАГРАДА ДНЯ", "🎁 DAILY REWARD"), PixelUiAtlas.ButtonPrimary, new Vector2(0f, 0f), new Vector2(210f, 38f), 15, new Color(1f, 0.92f, 0.35f));
            var dailyRect = dailyBtn.GetComponent<RectTransform>();
            dailyRect.anchorMin = new Vector2(1f, 0.5f);
            dailyRect.anchorMax = new Vector2(1f, 0.5f);
            dailyRect.pivot = new Vector2(1f, 0.5f);
            dailyRect.anchoredPosition = new Vector2(-360f, 0f);
            btnDailyHeaderLabel = dailyBtn.GetComponentInChildren<Text>();
            dailyBtn.onClick.AddListener(OpenDailyRewardModal);

            playerNameText = CreateText(header.transform, "PlayerName", "ПИЛОТ: ГОСТЬ", new Vector2(0.5f, 0.5f), new Vector2(52f, 0f), new Vector2(360f, 32f), 18, new Color(0.65f, 0.95f, 1f), FontStyle.Bold);
            var nameRect = playerNameText.rectTransform;
            nameRect.pivot = new Vector2(1f, 0.5f);
            nameRect.anchoredPosition = new Vector2(52f, 0f);
            playerNameText.alignment = TextAnchor.MiddleRight;

            authButton = CreateSlicedButton(header.transform, "BtnAuth", Localization.T("ВОЙТИ", "SIGN IN"), PixelUiAtlas.ButtonSecondary, new Vector2(148f, 0f), new Vector2(150f, 36f), 16, Color.white);
            btnAuthLabel = authButton.GetComponentInChildren<Text>();
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

            // Floating Ship Sprite (Original sleek saucer facing right)
            var shipObj = CreateImage("ShipVisual", shipContainer.transform, shipSprite, Color.white);
            shipObj.rectTransform.anchoredPosition = new Vector2(10f, 0f);
            shipObj.rectTransform.sizeDelta = new Vector2(160f, 110f);
            shipObj.rectTransform.localEulerAngles = Vector3.zero;
            shipObj.preserveAspect = true;
            shipDisplayImage = shipObj;

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

            // Primary Hero Buttons - CAMPAIGN & ROGUELITE EXPEDITION (Distinct separation & generous spacing)
            var campaignBtn = CreateSlicedButton(menuRoot.transform, "BtnPlay", Localization.T("КАМПАНИЯ // СЮЖЕТ", "CAMPAIGN // STORY"), PixelUiAtlas.ButtonPrimary, new Vector2(0f, 172f), new Vector2(480f, 58f), 22, new Color(1f, 0.98f, 0.85f));
            btnCampaignLabel = campaignBtn.GetComponentInChildren<Text>();
            campaignBtn.onClick.AddListener(OnPlayClicked);

            var rogueBtn = CreateSlicedButton(menuRoot.transform, "BtnRogue", Localization.T("РОГАЛИК // ЭКСПЕДИЦИЯ", "ROGUELITE // EXPEDITION"), PixelUiAtlas.ButtonPrimary, new Vector2(0f, 104f), new Vector2(480f, 58f), 22, new Color(0.85f, 0.95f, 1f));
            btnRogueLabel = rogueBtn.GetComponentInChildren<Text>();
            rogueBtn.onClick.AddListener(OnRogueClicked);

            // Secondary Navigation Buttons (Clean, crisp, separated arcade buttons)
            var hangarBtn = CreateSlicedButton(menuRoot.transform, "BtnHangar", Localization.T("АНГАР И АРСЕНАЛ", "HANGAR & ARSENAL"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, 36f), new Vector2(480f, 50f), 19, Color.white);
            btnHangarLabel = hangarBtn.GetComponentInChildren<Text>();
            hangarBtn.onClick.AddListener(() => SceneLoader.LoadSceneByName(SceneLoader.ShopScene));

            var loreBtn = CreateSlicedButton(menuRoot.transform, "BtnLore", Localization.T("БОРТОВОЙ ЖУРНАЛ // ЛОР", "LOGBOOK // LORE"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -24f), new Vector2(480f, 48f), 19, new Color(0.9f, 0.95f, 1f));
            btnLoreLabel = loreBtn.GetComponentInChildren<Text>();
            loreBtn.onClick.AddListener(() => OpenModal(loreModal));

            var achBtn = CreateSlicedButton(menuRoot.transform, "BtnAchievements", Localization.T("ДОСТИЖЕНИЯ", "ACHIEVEMENTS"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -82f), new Vector2(480f, 48f), 19, Color.white);
            btnAchievementsLabel = achBtn.GetComponentInChildren<Text>();
            achBtn.onClick.AddListener(() => OpenModal(achievementsModal));

            var settingsBtn = CreateSlicedButton(menuRoot.transform, "BtnSettings", Localization.T("НАСТРОЙКИ", "SETTINGS"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -140f), new Vector2(480f, 48f), 19, Color.white);
            btnSettingsLabel = settingsBtn.GetComponentInChildren<Text>();
            settingsBtn.onClick.AddListener(() => OpenModal(settingsModal));
        }

        #region Modals Implementation

        private void BuildShopModal(Transform parent)
        {
            shopModal = CreateModalContainer(parent, "ShopModal", out var card);
            CreateText(card.transform, "Title", Localization.T("КОСМИЧЕСКИЙ АНГАР // СКИНОТЕКА", "SPACE HANGAR // SKINS"), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(740f, 38f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            CreateText(card.transform, "Sub", Localization.T("Облик меняет цвет и стиль корабля, не влияя на боевые параметры.", "Hulls customize ship visuals and thruster trail effects without affecting combat stats."), new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(740f, 26f), 17, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);

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

                CreateText(row.transform, "Name", skin.LocalizedTitle, new Vector2(0f, 0.5f), new Vector2(125f, 14f), new Vector2(360f, 28f), 20, Color.white, FontStyle.Bold).alignment = TextAnchor.MiddleLeft;
                var costStr = skin.Price == 0 ? Localization.T("Базовый корпус", "Default Hull") : Localization.TFormat("Стоимость: {0} монет", "Price: {0} coins", skin.Price);
                CreateText(row.transform, "Price", costStr, new Vector2(0f, 0.5f), new Vector2(125f, -16f), new Vector2(360f, 24f), 17, new Color(1f, 0.85f, 0.4f), FontStyle.Normal).alignment = TextAnchor.MiddleLeft;

                var btn = CreateSlicedButton(row.transform, "Btn" + i, Localization.T("Выбрать", "Select"), PixelUiAtlas.ButtonSecondary, new Vector2(270f, 0f), new Vector2(190f, 52f), 19, Color.white);
                skinActionButtons.Add(btn);
                skinActionLabels.Add(btn.GetComponentInChildren<Text>());

                var s = skin;
                btn.onClick.AddListener(() => BuyOrEquipSkin(s));
            }

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", Localization.T("ЗАКРЫТЬ", "CLOSE"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildAchievementsModal(Transform parent)
        {
            achievementsModal = CreateModalContainer(parent, "AchievementsModal", out var card);
            achievementsTitleText = CreateText(card.transform, "Title", Localization.T("БОРТОВЫЕ ДОСТИЖЕНИЯ", "SHIP ACHIEVEMENTS"), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(740f, 38f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);

            var scroll = CreateScroll(card.transform, new Vector2(0f, -95f), new Vector2(800f, 500f));
            achievementScrollContent = scroll;

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", Localization.T("ЗАКРЫТЬ", "CLOSE"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
            achievementsCloseBtnLabel = closeBtn.GetComponentInChildren<Text>();
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildSettingsModal(Transform parent)
        {
            settingsModal = CreateModalContainer(parent, "SettingsModal", out var card);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(880f, 760f);

            settingsTitleText = CreateText(card.transform, "Title", Localization.T("НАСТРОЙКИ СИСТЕМЫ", "SYSTEM SETTINGS"), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(800f, 38f), 28, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);

            // 1. Language Section (Clean subpanel)
            var langBox = CreateImage("LangBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            langBox.type = Image.Type.Sliced;
            var lRect = langBox.rectTransform;
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.pivot = new Vector2(0.5f, 1f);
            lRect.anchoredPosition = new Vector2(0f, -76f);
            lRect.sizeDelta = new Vector2(820f, 72f);

            langCaptionText = CreateText(langBox.transform, "LangCaption", Localization.T("ЯЗЫК ИНТЕРФЕЙСА", "INTERFACE LANGUAGE"), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(460f, 32f), 18, Color.white, FontStyle.Bold);
            langCaptionText.alignment = TextAnchor.MiddleLeft;

            var langBtn = CreateSlicedButton(langBox.transform, "BtnLang", Localization.GetLanguageDisplayName(), PixelUiAtlas.ButtonPrimary, new Vector2(-28f, 0f), new Vector2(260f, 46f), 17, Color.white, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            langButtonLabel = langBtn.GetComponentInChildren<Text>();
            langBtn.onClick.AddListener(() =>
            {
                Localization.ToggleLanguage();
                RefreshAllTexts();
            });

            // 2. Volume Section (Clean sliced subpanel)
            var volBox = CreateImage("VolBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            volBox.type = Image.Type.Sliced;
            volBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            volBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            volBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            volBox.rectTransform.anchoredPosition = new Vector2(0f, -158f);
            volBox.rectTransform.sizeDelta = new Vector2(820f, 94f);

            volCaptionText = CreateText(volBox.transform, "VolCaption", Localization.T("ГРОМКОСТЬ ЗВУКА И САУНДТРЕКА", "MASTER & MUSIC VOLUME"), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(760f, 28f), 17, Color.white, FontStyle.Bold);
            CreateSlider(volBox.transform, new Vector2(0f, -54f));

            // 3. Controls Guide Section (Clean sliced subpanel)
            var ctrlBox = CreateImage("CtrlBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            ctrlBox.type = Image.Type.Sliced;
            ctrlBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            ctrlBox.rectTransform.anchoredPosition = new Vector2(0f, -262f);
            ctrlBox.rectTransform.sizeDelta = new Vector2(820f, 186f);

            ctrlCaptionText = CreateText(ctrlBox.transform, "CtrlCaption", Localization.T("БОРТОВОЕ УПРАВЛЕНИЕ", "FLIGHT CONTROLS"), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(760f, 28f), 17, new Color(0.35f, 0.85f, 1f), FontStyle.Bold);
            ctrlL1 = CreateText(ctrlBox.transform, "L1", Localization.T("WASD / Стрелки / Сенсорный стик — Маневрирование корабля", "WASD / Arrows / Touch Stick — Ship Maneuvering"), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(760f, 24f), 15, Color.white, FontStyle.Normal);
            ctrlL2 = CreateText(ctrlBox.transform, "L2", Localization.T("ПРОБЕЛ / Кнопка «Огонь» — Скорострельный залп", "SPACE / Fire Button — Primary Fire"), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(760f, 24f), 15, Color.white, FontStyle.Normal);
            ctrlL3 = CreateText(ctrlBox.transform, "L3", Localization.T("КЛАВИША [Q] — EX-ракета (тратит 1 карту энергии)", "KEY [Q] — EX Rocket (Consumes 1 Energy Card)"), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(760f, 24f), 15, new Color(0.4f, 0.85f, 1f), FontStyle.Normal);
            ctrlL4 = CreateText(ctrlBox.transform, "L4", Localization.T("КЛАВИША [E] — Мега-Лазер (полный супер-залп при полной шкале)", "KEY [E] — Mega-Laser (Full Super Beam on Full Hype)"), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(760f, 24f), 15, new Color(1f, 0.85f, 0.4f), FontStyle.Normal);
            ctrlL5 = CreateText(ctrlBox.transform, "L5", Localization.T("⚡ ПАРИРОВАНИЕ: Сближайся с РОЗОВЫМИ сферами для +30 Ярости!", "PARRY: Touch PINK hazards to absorb +30 Hype!"), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(760f, 24f), 14, new Color(1f, 0.45f, 0.85f), FontStyle.Bold);

            // 4. Reset Section (Spacious layout with clean text and button separation)
            var resetBox = CreateImage("ResetBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            resetBox.type = Image.Type.Sliced;
            resetBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            resetBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchoredPosition = new Vector2(0f, -460f);
            resetBox.rectTransform.sizeDelta = new Vector2(820f, 154f);

            resetTitleText = CreateText(resetBox.transform, "Warn", Localization.T("СБРОС ПРОГРЕССА И ПОВТОР ОБУЧЕНИЯ", "RESET PROGRESS & REPLAY TUTORIAL"), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(760f, 26f), 16, new Color(1f, 0.45f, 0.45f), FontStyle.Bold);
            resetDescText = CreateText(resetBox.transform, "WarnDesc", Localization.T("Очистит сохранения, открытые скины и рекорды,\nи сразу же запустит стартовое обучение пилота в Секторе 1.", "Clears save data, unlocked skins and records,\nand immediately launches flight tutorial in Sector 1."), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(760f, 38f), 14, new Color(0.9f, 0.8f, 0.85f), FontStyle.Normal);

            var resetBtn = CreateSlicedButton(resetBox.transform, "BtnReset", Localization.T("СБРОСИТЬ И НАЧАТЬ ОБУЧЕНИЕ", "RESET & START TUTORIAL"), PixelUiAtlas.ButtonDanger, new Vector2(0f, 16f), new Vector2(420f, 42f), 16, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            resetBtnLabel = resetBtn.GetComponentInChildren<Text>();
            resetBtn.onClick.AddListener(ResetProgressAndStartTutorial);

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", Localization.T("ЗАКРЫТЬ", "CLOSE"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, 22f), new Vector2(280f, 50f), 19, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            closeBtnLabel = closeBtn.GetComponentInChildren<Text>();
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void BuildLoreModal(Transform parent)
        {
            loreModal = CreateModalContainer(parent, "LoreModal", out var card);
            loreTitleText = CreateText(card.transform, "Title", Localization.T("БОРТОВОЙ ЖУРНАЛ", "FLIGHT LOGBOOK"), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(780f, 34f), 28, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            loreSubText = CreateText(card.transform, "Sub", Localization.T("Хроники Капитана Зигги, досье боссов Синдиката и тайны реликвий Hype Core", "Captain Ziggy's chronicles, Syndicate boss dossiers, and secrets of the Hype Core relics"), new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(780f, 36f), 16, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);

            var scroll = CreateScroll(card.transform, new Vector2(0f, -112f), new Vector2(800f, 470f));
            loreScrollContent = scroll;
            PopulateLoreItems();

            var watchCutsceneBtn = CreateSlicedButton(card.transform, "BtnWatchPrologue", Localization.T("СМОТРЕТЬ ПРОЛОГ (КАТСЦЕНА)", "WATCH PROLOGUE (CUTSCENE)"), PixelUiAtlas.ButtonPrimary, new Vector2(-160f, 24f), new Vector2(380f, 50f), 17, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            loreCutsceneBtnText = watchCutsceneBtn.GetComponentInChildren<Text>();
            watchCutsceneBtn.onClick.AddListener(() =>
            {
                CloseAllModals();
                CinematicIntroCutscene.Play(null);
            });

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", Localization.T("ЗАКРЫТЬ", "CLOSE"), PixelUiAtlas.ButtonSecondary, new Vector2(230f, 24f), new Vector2(240f, 50f), 19, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            loreCloseBtnText = closeBtn.GetComponentInChildren<Text>();
            closeBtn.onClick.AddListener(CloseAllModals);
        }

        private void PopulateLoreItems()
        {
            if (loreScrollContent == null) return;
            for (var i = loreScrollContent.childCount - 1; i >= 0; i--)
            {
                Destroy(loreScrollContent.GetChild(i).gameObject);
            }

            var entries = LoreCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var contentText = entry.LocalizedContent;
                var lines = Mathf.Max(2, Mathf.CeilToInt(contentText.Length / 72f));
                var bodyHeight = lines * 26f;
                var itemObj = CreateImage("LoreItem" + i, loreScrollContent, PixelUiAtlas.SubPanel, Color.white);
                itemObj.type = Image.Type.Sliced;
                var le = itemObj.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 92f + bodyHeight;
                le.preferredHeight = 92f + bodyHeight;

                var tag = PlaceLoreLine(itemObj.transform, "Tag", $"[{entry.LocalizedCategory}]", 12f, 22f, 15, new Color(0.55f, 0.9f, 1f), FontStyle.Bold);
                var title = PlaceLoreLine(itemObj.transform, "Title", entry.LocalizedTitle, 38f, 28f, 20, new Color(1f, 0.88f, 0.4f), FontStyle.Bold);
                var content = PlaceLoreLine(itemObj.transform, "Content", contentText, 72f, bodyHeight, 17, new Color(0.9f, 0.94f, 0.98f), FontStyle.Normal);
                tag.alignment = TextAnchor.MiddleLeft;
                title.alignment = TextAnchor.MiddleLeft;
                content.alignment = TextAnchor.UpperLeft;
                content.lineSpacing = 1.12f;
                content.verticalOverflow = VerticalWrapMode.Overflow;
            }
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

        private void BuildDailyRewardModal(Transform parent)
        {
            dailyRewardModal = CreateModalContainer(parent, "DailyRewardModal", out var card);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(920f, 540f);

            dailyTitleText = CreateText(card.transform, "Title", Localization.T("🎁 ЕЖЕДНЕВНЫЙ БОНУС ПИЛОТА 🎁", "🎁 DAILY PILOT BONUS 🎁"), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(860f, 38f), 28, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            dailySubText = CreateText(card.transform, "Sub", Localization.T("Заходи в игру каждый день, чтобы получать монеты и редкие Звёздные Ядра!", "Log in every day to collect coins and rare Star Cores!"), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(860f, 26f), 16, new Color(0.7f, 0.85f, 0.95f), FontStyle.Normal);

            var cardsObj = new GameObject("CardsGrid", typeof(RectTransform));
            cardsObj.transform.SetParent(card.transform, false);
            var gridRect = cardsObj.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 1f);
            gridRect.anchorMax = new Vector2(0.5f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.anchoredPosition = new Vector2(0f, -115f);
            gridRect.sizeDelta = new Vector2(880f, 230f);
            dailyCardsContainer = cardsObj.transform;

            dailyClaimBtn = CreateSlicedButton(card.transform, "BtnClaimDaily", Localization.T("ЗАБРАТЬ НАГРАДУ", "CLAIM REWARD"), PixelUiAtlas.ButtonPrimary, new Vector2(0f, 85f), new Vector2(440f, 52f), 19, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            dailyClaimBtnText = dailyClaimBtn.GetComponentInChildren<Text>();
            dailyClaimBtn.onClick.AddListener(() =>
            {
                if (SaveManager.Instance != null && SaveManager.Instance.ClaimDailyReward(out var c, out var cr))
                {
                    AudioManager.PlayVictory();
                    dailyClaimBtn.interactable = false;
                    if (dailyClaimBtnText != null)
                        dailyClaimBtnText.text = Localization.T("НАГРАДА ПОЛУЧЕНА!", "REWARD CLAIMED!");
                    RefreshData();
                    RebuildDailyCards();
                }
            });

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", Localization.T("ЗАКРЫТЬ", "CLOSE"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, 24f), new Vector2(240f, 44f), 17, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            dailyCloseBtnText = closeBtn.GetComponentInChildren<Text>();
            closeBtn.onClick.AddListener(CloseAllModals);

            RebuildDailyCards();
        }

        private void RebuildDailyCards()
        {
            if (dailyCardsContainer == null) return;
            for (var i = dailyCardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(dailyCardsContainer.GetChild(i).gameObject);
            }

            var save = SaveManager.Instance;
            var currentDay = save != null ? save.GetCurrentDailyDay() : 1;
            var isAvailable = save != null && save.IsDailyRewardAvailable();

            var startX = -375f;
            var gapX = 125f;

            for (var d = 1; d <= 7; d++)
            {
                var dayNum = d;
                var reward = save != null ? save.GetDailyRewardValues(dayNum) : new SaveManager.DailyRewardEntry(50, 0);
                var isCurrent = dayNum == currentDay;
                var isPast = dayNum < currentDay && !isAvailable;
                var isJackpot = dayNum == 7;

                var cardBox = CreateImage($"DayCard_{dayNum}", dailyCardsContainer, PixelUiAtlas.SubPanel, Color.white);
                cardBox.type = Image.Type.Sliced;
                var cRect = cardBox.rectTransform;
                cRect.anchorMin = new Vector2(0.5f, 0.5f);
                cRect.anchorMax = new Vector2(0.5f, 0.5f);
                cRect.pivot = new Vector2(0.5f, 0.5f);
                cRect.anchoredPosition = new Vector2(startX + (dayNum - 1) * gapX, 0f);
                cRect.sizeDelta = new Vector2(110f, 210f);

                if (isCurrent && isAvailable)
                    cardBox.color = isJackpot ? new Color(0.38f, 0.32f, 0.12f, 1f) : new Color(0.18f, 0.35f, 0.55f, 1f);
                else if (isPast)
                    cardBox.color = new Color(0.08f, 0.12f, 0.18f, 0.6f);
                else
                    cardBox.color = isJackpot ? new Color(0.22f, 0.18f, 0.08f, 0.95f) : new Color(0.08f, 0.14f, 0.22f, 0.95f);

                CreateText(cardBox.transform, "DayHeader", Localization.TFormat("ДЕНЬ {0}", "DAY {0}", dayNum), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(104f, 20f), 15, isCurrent ? new Color(1f, 0.85f, 0.3f) : (isJackpot ? new Color(1f, 0.8f, 0.4f) : new Color(0.65f, 0.85f, 1f)), FontStyle.Bold);

                if (isJackpot)
                {
                    CreateText(cardBox.transform, "JackpotLabel", Localization.T("★ ДЖЕКПОТ ★", "★ JACKPOT ★"), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(108f, 14f), 10, new Color(1f, 0.85f, 0.25f), FontStyle.Bold);
                }

                // Reward Icon
                var iconSprite = isJackpot ? PixelUiAtlas.IconChestEpic : (reward.cores > 0 ? PixelUiAtlas.IconStarCore : PixelUiAtlas.IconCredit);
                var iconImg = CreateImage($"DayIcon_{dayNum}", cardBox.transform, iconSprite, Color.white);
                var iconRect = iconImg.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = new Vector2(0f, isJackpot ? 10f : 14f);
                iconRect.sizeDelta = isJackpot ? new Vector2(54f, 54f) : new Vector2(44f, 44f);
                iconImg.preserveAspect = true;

                if (isJackpot)
                {
                    jackpotAuraTransform = iconImg.transform;
                }

                // Reward text
                string rewardStr;
                if (isJackpot)
                    rewardStr = Localization.TFormat("+{0} 💰\n+{1} ЯДРА", "+{0} 💰\n+{1} CORES", reward.credits, reward.cores);
                else if (reward.cores > 0)
                    rewardStr = Localization.TFormat("+{0} 💰\n+{1} ЯДРО", "+{0} 💰\n+{1} CORE", reward.credits, reward.cores);
                else
                    rewardStr = $"+{reward.credits} 💰";

                CreateText(cardBox.transform, "Val", rewardStr, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(106f, 38f), 13, isJackpot ? new Color(1f, 0.88f, 0.35f) : new Color(1f, 0.95f, 0.65f), FontStyle.Bold);

                if (isPast)
                {
                    var overlay = CreateImage("PastOverlay", cardBox.transform, PixelUiAtlas.White, new Color(0f, 0f, 0f, 0.55f));
                    Stretch(overlay.rectTransform);
                    CreateText(cardBox.transform, "Check", "✓", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), 38, new Color(0.3f, 1f, 0.45f), FontStyle.Bold);
                }
            }

            if (dailyClaimBtn != null)
            {
                dailyClaimBtn.interactable = isAvailable;
                if (dailyClaimBtnText != null)
                {
                    dailyClaimBtnText.text = isAvailable
                        ? Localization.T("ЗАБРАТЬ НАГРАДУ", "CLAIM REWARD")
                        : Localization.T("НАГРАДА УЖЕ ПОЛУЧЕНА", "ALREADY CLAIMED");
                }
            }
        }

        private void OpenDailyRewardModal()
        {
            RebuildDailyCards();
            OpenModal(dailyRewardModal);
        }

        private void OpenModal(GameObject modal)
        {
            CloseAllModals();
            if (modal != null)
                modal.SetActive(true);
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
            var rawName = authorized ? sdk.PlayerName : string.Empty;
            string displayName;
            if (!authorized || string.IsNullOrEmpty(rawName) || rawName.Equals("Гость", System.StringComparison.OrdinalIgnoreCase))
                displayName = Localization.T("ГОСТЬ", "GUEST");
            else if (rawName.Equals("Локальный пилот", System.StringComparison.OrdinalIgnoreCase))
                displayName = Localization.T("ЛОКАЛЬНЫЙ ПИЛОТ", "LOCAL PILOT");
            else
                displayName = rawName.ToUpper();

            if (playerNameText != null)
                playerNameText.text = Localization.TFormat("ПИЛОТ: {0}", "PILOT: {0}", displayName);
            if (authButton != null)
                authButton.gameObject.SetActive(!authorized);
            if (firstLaunchNameText != null)
            {
                firstLaunchNameText.text = authorized
                    ? Localization.TFormat("На связи {0}. Это имя пришло после авторизации.", "Welcome {0}. Identity verified via platform.", displayName)
                    : Localization.T("Пока ты гость. Кнопка «Войти» в шапке запрашивает имя у Яндекс Игр.", "Playing as Guest. 'Sign In' authorizes via Yandex Games.");
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

            firstLaunchTitleText = CreateText(card.transform, "Title", Localization.T("ПЕРВЫЙ ВЫЛЕТ", "MAIDEN FLIGHT"), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(680f, 40f), 32, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            firstLaunchNameText = CreateText(card.transform, "Pilot", Localization.T("Пока ты гость.", "Playing as Guest."), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(680f, 28f), 18, new Color(0.65f, 0.95f, 1f), FontStyle.Normal);
            firstLaunchBodyText = CreateText(card.transform, "Body",
                Localization.T(
                    "Сессия начинается с меню. Окно урока закроется только после манёвра, выстрела, ракеты на Q и мега-луча на E.\nДальше задание миссии — пройденный путь, а шкала Ярости копится от сбитых целей.",
                    "Mission flight begins from the hangar. The tutorial briefing completes after maneuvers, primary fire, EX rocket on [Q], and Mega-Laser on [E]. Progress by distance and charge Hype from destroyed targets."),
                new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(660f, 110f), 20, Color.white, FontStyle.Normal);
            firstLaunchBodyText.alignment = TextAnchor.UpperCenter;

            var go = CreateSlicedButton(card.transform, "BtnFirstFlight", Localization.T("В ПЕРВЫЙ ВЫЛЕТ", "LAUNCH FLIGHT"), PixelUiAtlas.ButtonPrimary, new Vector2(0f, -40f), new Vector2(420f, 58f), 22, new Color(1f, 0.98f, 0.85f));
            firstLaunchGoBtnLabel = go.GetComponentInChildren<Text>();
            go.onClick.AddListener(StartCampaign);
            var stay = CreateSlicedButton(card.transform, "BtnStay", Localization.T("ОСТАТЬСЯ В МЕНЮ", "STAY IN MENU"), PixelUiAtlas.ButtonSecondary, new Vector2(0f, -110f), new Vector2(420f, 48f), 18, Color.white);
            firstLaunchStayBtnLabel = stay.GetComponentInChildren<Text>();
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

            // Cold Open: Launch cinematic intro cutscene if first time entering campaign
            if (SaveManager.Instance != null && !SaveManager.Instance.HasSeenIntro)
            {
                CinematicIntroCutscene.Play(() =>
                {
                    SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
                });
                return;
            }

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

        private void RefreshAllTexts()
        {
            if (btnCampaignLabel != null) btnCampaignLabel.text = Localization.T("КАМПАНИЯ // СЮЖЕТ", "CAMPAIGN // STORY");
            if (btnRogueLabel != null) btnRogueLabel.text = Localization.T("РОГАЛИК // ЭКСПЕДИЦИЯ", "ROGUELITE // EXPEDITION");
            if (btnHangarLabel != null) btnHangarLabel.text = Localization.T("АНГАР И АРСЕНАЛ", "HANGAR & ARSENAL");
            if (btnLoreLabel != null) btnLoreLabel.text = Localization.T("БОРТОВОЙ ЖУРНАЛ // ЛОР", "LOGBOOK // LORE");
            if (btnAchievementsLabel != null) btnAchievementsLabel.text = Localization.T("ДОСТИЖЕНИЯ", "ACHIEVEMENTS");
            if (btnSettingsLabel != null) btnSettingsLabel.text = Localization.T("НАСТРОЙКИ", "SETTINGS");
            if (btnAuthLabel != null) btnAuthLabel.text = Localization.T("ВОЙТИ", "SIGN IN");

            if (settingsTitleText != null) settingsTitleText.text = Localization.T("НАСТРОЙКИ СИСТЕМЫ", "SYSTEM SETTINGS");
            if (langCaptionText != null) langCaptionText.text = Localization.T("ЯЗЫК ИНТЕРФЕЙСА", "INTERFACE LANGUAGE");
            if (langButtonLabel != null) langButtonLabel.text = Localization.GetLanguageDisplayName();
            if (volCaptionText != null) volCaptionText.text = Localization.T("ГРОМКОСТЬ ЗВУКА И САУНДТРЕКА", "MASTER & MUSIC VOLUME");
            if (ctrlCaptionText != null) ctrlCaptionText.text = Localization.T("БОРТОВОЕ УПРАВЛЕНИЕ", "FLIGHT CONTROLS");
            if (ctrlL1 != null) ctrlL1.text = Localization.T("WASD / Стрелки / Сенсорный стик — Маневрирование корабля", "WASD / Arrows / Touch Stick — Ship Maneuvering");
            if (ctrlL2 != null) ctrlL2.text = Localization.T("ПРОБЕЛ / Кнопка «Огонь» — Скорострельный залп", "SPACE / Fire Button — Primary Fire");
            if (ctrlL3 != null) ctrlL3.text = Localization.T("КЛАВИША [Q] — EX-ракета (тратит 1 карту энергии)", "KEY [Q] — EX Rocket (Consumes 1 Energy Card)");
            if (ctrlL4 != null) ctrlL4.text = Localization.T("КЛАВИША [E] — Мега-Лазер (полный супер-залп при полной шкале)", "KEY [E] — Mega-Laser (Full Super Beam on Full Hype)");
            if (ctrlL5 != null) ctrlL5.text = Localization.T("⚡ ПАРИРОВАНИЕ: Сближайся с РОЗОВЫМИ сферами для +30 Ярости!", "PARRY: Touch PINK hazards to absorb +30 Hype!");
            if (resetTitleText != null) resetTitleText.text = Localization.T("СБРОС ПРОГРЕССА И ПОВТОР ОБУЧЕНИЯ", "RESET PROGRESS & REPLAY TUTORIAL");
            if (resetDescText != null) resetDescText.text = Localization.T("Очистит сохранения, открытые скины и рекорды,\nи сразу же запустит стартовое обучение пилота в Секторе 1.", "Clears save data, unlocked skins and records,\nand immediately launches flight tutorial in Sector 1.");
            if (resetBtnLabel != null) resetBtnLabel.text = Localization.T("СБРОСИТЬ И НАЧАТЬ ОБУЧЕНИЕ", "RESET & START TUTORIAL");
            if (closeBtnLabel != null) closeBtnLabel.text = Localization.T("ЗАКРЫТЬ", "CLOSE");

            if (loreTitleText != null) loreTitleText.text = Localization.T("БОРТОВОЙ ЖУРНАЛ", "FLIGHT LOGBOOK");
            if (loreSubText != null) loreSubText.text = Localization.T("Хроники Капитана Зигги, досье боссов Синдиката и тайны реликвий Hype Core", "Captain Ziggy's chronicles, Syndicate boss dossiers, and secrets of the Hype Core relics");
            if (loreCutsceneBtnText != null) loreCutsceneBtnText.text = Localization.T("СМОТРЕТЬ ПРОЛОГ (КАТСЦЕНА)", "WATCH PROLOGUE (CUTSCENE)");
            if (loreCloseBtnText != null) loreCloseBtnText.text = Localization.T("ЗАКРЫТЬ", "CLOSE");
            PopulateLoreItems();

            if (achievementsTitleText != null) achievementsTitleText.text = Localization.T("БОРТОВЫЕ ДОСТИЖЕНИЯ", "SHIP ACHIEVEMENTS");
            if (achievementsCloseBtnLabel != null) achievementsCloseBtnLabel.text = Localization.T("ЗАКРЫТЬ", "CLOSE");

            if (firstLaunchTitleText != null) firstLaunchTitleText.text = Localization.T("ПЕРВЫЙ ВЫЛЕТ", "MAIDEN FLIGHT");
            if (firstLaunchBodyText != null)
            {
                firstLaunchBodyText.text = Localization.T(
                    "Сессия начинается с меню. Окно урока закроется только после манёвра, выстрела, ракеты на Q и мега-луча на E.\nДальше задание миссии — пройденный путь, а шкала Ярости копится от сбитых целей.",
                    "Mission flight begins from the hangar. The tutorial briefing completes after maneuvers, primary fire, EX rocket on [Q], and Mega-Laser on [E]. Progress by distance and charge Hype from destroyed targets.");
            }
            if (firstLaunchGoBtnLabel != null) firstLaunchGoBtnLabel.text = Localization.T("В ПЕРВЫЙ ВЫЛЕТ", "LAUNCH FLIGHT");
            if (firstLaunchStayBtnLabel != null) firstLaunchStayBtnLabel.text = Localization.T("ОСТАТЬСЯ В МЕНЮ", "STAY IN MENU");

            if (btnDailyHeaderLabel != null)
            {
                var isAvailable = SaveManager.Instance != null && SaveManager.Instance.IsDailyRewardAvailable();
                btnDailyHeaderLabel.text = isAvailable
                    ? Localization.T("🎁 НАГРАДА ДНЯ !", "🎁 DAILY REWARD !")
                    : Localization.T("🎁 НАГРАДА ДНЯ", "🎁 DAILY REWARD");
            }
            if (dailyTitleText != null) dailyTitleText.text = Localization.T("🎁 ЕЖЕДНЕВНЫЙ БОНУС ПИЛОТА 🎁", "🎁 DAILY PILOT BONUS 🎁");
            if (dailySubText != null) dailySubText.text = Localization.T("Заходи в игру каждый день, чтобы получать монеты и редкие Звёздные Ядра!", "Log in every day to collect coins and rare Star Cores!");
            if (dailyCloseBtnText != null) dailyCloseBtnText.text = Localization.T("ЗАКРЫТЬ", "CLOSE");
            RebuildDailyCards();

            RefreshData();
        }

        private void RefreshData()
        {
            var save = SaveManager.Instance;
            var coins = save != null ? save.Data.coins : 0;
            var missionIndex = save != null ? save.CurrentMission : 0;
            var mission = MissionCatalog.All[Mathf.Clamp(missionIndex, 0, MissionCatalog.All.Length - 1)];
            var kills = save != null ? save.Data.totalKills : 0;
            var equippedSkinId = save != null ? save.Data.equippedSkinId : "default";
            var equippedSkin = ShipSkinCatalog.Find(equippedSkinId);

            if (btnDailyHeaderLabel != null)
            {
                var isAvailable = save != null && save.IsDailyRewardAvailable();
                btnDailyHeaderLabel.text = isAvailable
                    ? Localization.T("🎁 НАГРАДА ДНЯ !", "🎁 DAILY REWARD !")
                    : Localization.T("🎁 НАГРАДА ДНЯ", "🎁 DAILY REWARD");
            }

            if (coinsText != null)
                coinsText.text = Localization.TFormat("{0} МОНЕТ", "{0} COINS", coins);
            if (rankText != null)
                rankText.text = Localization.TFormat("{0} // {1} СБИТО", "{0} // {1} KILLS", PilotRank.Title(kills).ToUpper(), kills);
            if (sectorTitleText != null)
                sectorTitleText.text = mission.LocalizedTitle.ToUpper();
            if (sectorDescText != null)
                sectorDescText.text = mission.LocalizedSummary;
            if (shipSkinTitleText != null)
            {
                var weapon = WeaponCatalog.Find(save != null ? save.Data.equippedWeaponId : SaveManager.DefaultWeaponId);
                var weaponTitle = weapon != null ? weapon.LocalizedTitle : Localization.T("Базовое оружие", "Default Weapon");
                var skinTitle = equippedSkin != null ? equippedSkin.LocalizedTitle : Localization.T("Базовый корпус", "Default Hull");
                shipSkinTitleText.text = Localization.TFormat("КОРПУС: {0}\nОРУЖИЕ: {1}", "HULL: {0}\nWEAPON: {1}", skinTitle.ToUpper(), weaponTitle.ToUpper());
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
                    skinActionLabels[i].text = Localization.T("НАДЕТ", "EQUIPPED");
                    skinActionButtons[i].interactable = false;
                }
                else if (owned || skin.Price == 0)
                {
                    skinActionLabels[i].text = Localization.T("ЭКИПИРОВАТЬ", "EQUIP");
                    skinActionButtons[i].interactable = true;
                }
                else
                {
                    skinActionLabels[i].text = Localization.TFormat("КУПИТЬ ({0})", "BUY ({0})", skin.Price);
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
                var status = unlocked ? Localization.T("ОТКРЫТО", "UNLOCKED") : $"{progress} / {data.targetAmount}";

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
                var achTitle = Localization.GetAchievementTitle(data.id, data.displayName);
                var achDesc = Localization.GetAchievementDesc(data.id, data.description);

                var nameText = CreateText(row.transform, "Title", badge + achTitle + "  —  [" + status + "]", new Vector2(0f, 1f), new Vector2(25f, -22f), new Vector2(720f, 28f), 19, unlocked ? new Color(0.7f, 1f, 0.75f) : Color.white, FontStyle.Bold);
                nameText.alignment = TextAnchor.MiddleLeft;

                var descText = CreateText(row.transform, "Desc", achDesc, new Vector2(0f, 1f), new Vector2(25f, -54f), new Vector2(720f, 26f), 15, new Color(0.75f, 0.85f, 0.95f), FontStyle.Normal);
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
            scaler.dynamicPixelsPerUnit = 1.25f;
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
            label.font = GameFont.Get();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.lineSpacing = 1.08f;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
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

        private static Button CreateSlicedButton(Transform parent, string name, string text, Sprite sprite, Vector2 pos, Vector2 size, int fontSize, Color textColor, Vector2? anchor = null, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            var a = anchor ?? new Vector2(0.5f, 0.5f);
            var p = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchorMin = a;
            rect.anchorMax = a;
            rect.pivot = p;
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
