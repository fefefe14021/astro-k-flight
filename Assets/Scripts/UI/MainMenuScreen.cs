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

            RefreshData();
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= RefreshData;
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.Changed -= RefreshData;
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

            // 5. Modals (Shop, Achievements, Settings)
            BuildShopModal(canvas.transform);
            BuildAchievementsModal(canvas.transform);
            BuildSettingsModal(canvas.transform);

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
            rankText = CreateText(header.transform, "RankBadge", "🎖 ПИЛОТ // 0 СБИТО", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(450f, 36f), 20, new Color(1f, 0.85f, 0.4f), FontStyle.Bold);
            rankText.alignment = TextAnchor.MiddleLeft;

            // Right: Credits / Currency badge
            coinsText = CreateText(header.transform, "CoinsBadge", "🪙 0 МОНЕТ", new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(300f, 36f), 20, Color.white, FontStyle.Bold);
            coinsText.alignment = TextAnchor.MiddleRight;
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

            // Floating Ship Sprite (Original sleek ship facing right)
            var shipObj = CreateImage("ShipVisual", shipContainer.transform, shipSprite, Color.white);
            shipObj.rectTransform.anchoredPosition = Vector2.zero;
            shipObj.rectTransform.sizeDelta = new Vector2(128f, 100f);
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
            flameRect.sizeDelta = new Vector2(40f, 30f);
            flameRect.localEulerAngles = Vector3.zero;
            thrusterFlameTransform = flameRect;

            // Skin Name Label
            shipSkinTitleText = CreateText(shipContainer.transform, "SkinTitle", "КОРПУС: БАЗОВЫЙ", new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(360f, 30f), 18, new Color(0.75f, 0.88f, 1f), FontStyle.Bold);

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
            sectorDescText = CreateText(sectorCard.transform, "Desc", "Astro K выходит из доков Neon City. Собери квоту Album для перехода в гиперпространство.", new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(500f, 60f), 16, new Color(0.8f, 0.9f, 1f), FontStyle.Normal);
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

            // Primary Hero Button - PLAY / FLY (Sliced golden arcade button)
            var playBtn = CreateSlicedButton(menuRoot.transform, "BtnPlay", "В ПОЛЁТ [СТАРТ]", PixelUiAtlas.ButtonPrimary, new Vector2(0f, 155f), new Vector2(480f, 92f), 32, new Color(1f, 0.98f, 0.85f));
            playBtn.onClick.AddListener(OnPlayClicked);

            // Secondary Navigation Buttons (Sliced cyber-blue arcade buttons)
            var hangarBtn = CreateSlicedButton(menuRoot.transform, "BtnHangar", "🚀  АНГАР И ОБЛИКИ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, 55f), new Vector2(480f, 72f), 22, Color.white);
            hangarBtn.onClick.AddListener(() => OpenModal(shopModal));

            var achBtn = CreateSlicedButton(menuRoot.transform, "BtnAchievements", "🏆  ДОСТИЖЕНИЯ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -35f), new Vector2(480f, 72f), 22, Color.white);
            achBtn.onClick.AddListener(() => OpenModal(achievementsModal));

            var settingsBtn = CreateSlicedButton(menuRoot.transform, "BtnSettings", "⚙️  НАСТРОЙКИ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -125f), new Vector2(480f, 72f), 22, Color.white);
            settingsBtn.onClick.AddListener(() => OpenModal(settingsModal));

            var quitBtn = CreateSlicedButton(menuRoot.transform, "BtnQuit", "🚪  ВЫХОД ИЗ ИГРЫ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -215f), new Vector2(480f, 64f), 20, new Color(0.95f, 0.75f, 0.8f));
            quitBtn.onClick.AddListener(SceneLoader.Quit);
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
            CreateText(ctrlBox.transform, "L4", "КЛАВИША [E] — Мега-Лазер (полный супер-залп при 100 Hype)", new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(720f, 24f), 16, new Color(1f, 0.85f, 0.4f), FontStyle.Normal);
            CreateText(ctrlBox.transform, "L5", "⚡ ПАРИРОВАНИЕ: Сближайся с РОЗОВЫМИ сферами для поглощения и +30 Hype!", new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(720f, 22f), 15, new Color(1f, 0.45f, 0.85f), FontStyle.Bold);

            // Reset Section - KEPT PROMINENTLY IN SETTINGS AS REQUESTED!
            var resetBox = CreateImage("ResetBox", card.transform, PixelUiAtlas.SubPanel, Color.white);
            resetBox.type = Image.Type.Sliced;
            resetBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            resetBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            resetBox.rectTransform.anchoredPosition = new Vector2(0f, -395f);
            resetBox.rectTransform.sizeDelta = new Vector2(780f, 140f);

            CreateText(resetBox.transform, "Warn", "СБРОС ПРОГРЕССА И ПОВТОР ОБУЧЕНИЯ", new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(700f, 26f), 19, new Color(1f, 0.45f, 0.45f), FontStyle.Bold);
            CreateText(resetBox.transform, "WarnDesc", "Очистит сохранения, открытые скины и рекорды,\nи сразу же запустит стартовое обучение пилота в Секторе 1.", new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(720f, 36f), 15, new Color(0.9f, 0.8f, 0.85f), FontStyle.Normal);

            var resetBtn = CreateSlicedButton(resetBox.transform, "BtnReset", "СБРОСИТЬ И НАЧАТЬ ОБУЧЕНИЕ", PixelUiAtlas.ButtonDanger, new Vector2(0f, -98f), new Vector2(420f, 46f), 18, Color.white);
            resetBtn.onClick.AddListener(ResetProgressAndStartTutorial);

            var closeBtn = CreateSlicedButton(card.transform, "BtnClose", "ЗАКРЫТЬ", PixelUiAtlas.ButtonSecondary, new Vector2(0f, -330f), new Vector2(300f, 54f), 20, Color.white);
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
                modal.SetActive(true);
        }

        private void CloseAllModals()
        {
            if (shopModal != null) shopModal.SetActive(false);
            if (achievementsModal != null) achievementsModal.SetActive(false);
            if (settingsModal != null) settingsModal.SetActive(false);
        }

        #endregion

        #region Actions & Data Binding

        private void OnPlayClicked()
        {
            var index = SaveManager.Instance != null ? SaveManager.Instance.CurrentMission : 0;
            MissionCatalog.Select(index);
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
            var missionIndex = save != null ? save.CurrentMission : 0;
            var mission = MissionCatalog.All[Mathf.Clamp(missionIndex, 0, MissionCatalog.All.Length - 1)];
            var kills = save != null ? save.Data.totalKills : 0;
            var equippedSkinId = save != null ? save.Data.equippedSkinId : "default";
            var equippedSkin = ShipSkinCatalog.Find(equippedSkinId);

            if (coinsText != null)
                coinsText.text = $"🪙 {coins} МОНЕТ";
            if (rankText != null)
                rankText.text = $"🎖 {PilotRank.Title(kills).ToUpper()} // {kills} СБИТО";
            if (sectorTitleText != null)
                sectorTitleText.text = mission.Title.ToUpper();
            if (sectorDescText != null)
                sectorDescText.text = mission.Summary;
            if (shipSkinTitleText != null)
                shipSkinTitleText.text = $"КОРПУС: {equippedSkin.Title.ToUpper()}";

            if (shipDisplayImage != null)
                shipDisplayImage.color = equippedSkin.Color;

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
                layout.minHeight = 82f;
                layout.preferredHeight = 82f;

                var accent = CreateImage("Accent", row.transform, PixelUiAtlas.White, unlocked ? new Color(0.3f, 1f, 0.6f, 1f) : new Color(0.25f, 0.4f, 0.6f, 0.6f));
                accent.rectTransform.anchorMin = new Vector2(0f, 0f);
                accent.rectTransform.anchorMax = new Vector2(0f, 1f);
                accent.rectTransform.pivot = new Vector2(0f, 0.5f);
                accent.rectTransform.sizeDelta = new Vector2(4f, 0f);

                var badge = unlocked ? "★ " : "○ ";
                var nameText = CreateText(row.transform, "Title", badge + data.displayName + "  —  [" + status + "]", new Vector2(0f, 1f), new Vector2(25f, -18f), new Vector2(720f, 26f), 19, unlocked ? new Color(0.7f, 1f, 0.75f) : Color.white, FontStyle.Bold);
                nameText.alignment = TextAnchor.MiddleLeft;

                var descText = CreateText(row.transform, "Desc", data.description, new Vector2(0f, 1f), new Vector2(25f, -48f), new Vector2(720f, 24f), 15, new Color(0.75f, 0.85f, 0.95f), FontStyle.Normal);
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
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
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
