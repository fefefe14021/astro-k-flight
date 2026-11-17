// Assets/Scripts/UI/ShopScreen.cs
// Namespace: KANYE.UI
// Complete Armory & Hangar Shop: Flying Saucer Hulls and Diverse Weapon Arsenal.
// Features scrollable item list, responsive text wrapping, live thruster preview, and crystal-clear UI.

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
    public class ShopScreen : MonoBehaviour
    {
        private enum ShopTab
        {
            Weapons,
            Skins,
            Tech
        }

        private ShopTab currentTab = ShopTab.Weapons;
        private int selectedWeaponIndex = 0;
        private int selectedSkinIndex = 0;
        private int selectedTechIndex = 0;

        private Text coinsLabel;
        private Text hintLabel;
        private bool selectionReady;
        private Transform listContainer;
        private readonly List<GameObject> activeListButtons = new List<GameObject>();

        // Detail View UI Elements
        private Image previewImage;
        private Image previewThruster;
        private Image previewGlow;
        private Text previewTitle;
        private Text previewSubtitle;
        private Text previewDescription;
        private Text previewStats;
        private Button actionButton;
        private Text actionButtonText;
        private Button upgradeButton;
        private Text upgradeButtonText;
        private Button tabWeaponsBtn;
        private Button tabSkinsBtn;
        private Button tabTechBtn;

        private void Start()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            BuildUI();
            Refresh();

            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= Refresh;
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("ShopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Background
            var bgObj = new GameObject("Bg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = PixelUiAtlas.Background;
            bgImg.color = new Color(0.35f, 0.4f, 0.5f, 1f);

            // Main Window Panel
            var mainPanel = new GameObject("MainPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            mainPanel.transform.SetParent(canvasObj.transform, false);
            var mainRect = mainPanel.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.pivot = new Vector2(0.5f, 0.5f);
            mainRect.sizeDelta = new Vector2(1140f, 760f);
            var panelImg = mainPanel.GetComponent<Image>();
            panelImg.sprite = PixelUiAtlas.Panel;
            panelImg.type = Image.Type.Sliced;

            // Header Title
            CreateText(mainPanel.transform, "HeaderTitle", "АНГАР И АРСЕНАЛ КАПИТАНА ЗИГГИ", 30, new Vector2(0f, 335f), new Vector2(800f, 40f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            coinsLabel = CreateText(mainPanel.transform, "CoinsLabel", CoinsText(), 22, new Vector2(0f, 298f), new Vector2(900f, 28f), new Color(1f, 0.95f, 0.7f), FontStyle.Bold);
            hintLabel = CreateText(mainPanel.transform, "HintLabel", "Выбери корпус или оружие справа от списка.", 16, new Vector2(0f, 272f), new Vector2(980f, 24f), new Color(0.75f, 0.9f, 1f), FontStyle.Normal);

            // Tab Buttons
            tabWeaponsBtn = CreateTabButton(mainPanel.transform, "TabWeapons", "🚀 АРСЕНАЛ", new Vector2(-330f, 240f), () => SetTab(ShopTab.Weapons));
            tabSkinsBtn = CreateTabButton(mainPanel.transform, "TabSkins", "🛸 КОРПУСА ТАРЕЛКИ", new Vector2(0f, 240f), () => SetTab(ShopTab.Skins));
            tabTechBtn = CreateTabButton(mainPanel.transform, "TabTech", "⚡ МОДЕРНИЗАЦИЯ", new Vector2(330f, 240f), () => SetTab(ShopTab.Tech));

            // Left Side: Scrollable List Viewport
            var listPanel = new GameObject("ListPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            listPanel.transform.SetParent(mainPanel.transform, false);
            var listRect = listPanel.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0.5f);
            listRect.anchorMax = new Vector2(0f, 0.5f);
            listRect.pivot = new Vector2(0f, 0.5f);
            listRect.anchoredPosition = new Vector2(40f, -25f);
            listRect.sizeDelta = new Vector2(500f, 440f);

            var listBg = listPanel.GetComponent<Image>();
            listBg.sprite = PixelUiAtlas.SubPanel;
            listBg.type = Image.Type.Sliced;
            listBg.color = new Color(0.08f, 0.12f, 0.2f, 0.88f);

            var scroll = listPanel.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;

            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(listPanel.transform, false);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(-16f, 0f);

            var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            scroll.viewport = listRect;
            listContainer = contentObj.transform;

            // Right Side: Card Preview & Specs
            var previewPanel = new GameObject("PreviewPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewPanel.transform.SetParent(mainPanel.transform, false);
            var prevRect = previewPanel.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(1f, 0.5f);
            prevRect.anchorMax = new Vector2(1f, 0.5f);
            prevRect.pivot = new Vector2(1f, 0.5f);
            prevRect.anchoredPosition = new Vector2(-40f, -25f);
            prevRect.sizeDelta = new Vector2(520f, 440f);
            var prevBg = previewPanel.GetComponent<Image>();
            prevBg.sprite = PixelUiAtlas.SubPanel;
            prevBg.type = Image.Type.Sliced;
            prevBg.color = new Color(0.08f, 0.12f, 0.2f, 0.92f);

            // Preview Frame for Sprite & Thruster
            var frameObj = new GameObject("SpriteFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameObj.transform.SetParent(previewPanel.transform, false);
            var frameRect = frameObj.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 1f);
            frameRect.anchorMax = new Vector2(0.5f, 1f);
            frameRect.pivot = new Vector2(0.5f, 1f);
            frameRect.anchoredPosition = new Vector2(0f, -16f);
            frameRect.sizeDelta = new Vector2(280f, 150f);
            var frameImg = frameObj.GetComponent<Image>();
            frameImg.sprite = PixelUiAtlas.SubPanel;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = new Color(0.03f, 0.05f, 0.1f, 0.98f);

            // Ventral Glow preview under the saucer
            var glowObj = new GameObject("GlowPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowObj.transform.SetParent(frameObj.transform, false);
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = new Vector2(10f, -36f);
            glowRect.sizeDelta = new Vector2(130f, 26f);
            previewGlow = glowObj.GetComponent<Image>();
            previewGlow.sprite = Resources.Load<Sprite>("Sprites/spark");
            previewGlow.color = new Color(0.2f, 0.85f, 1f, 0.65f);

            // Thruster Flame preview behind the saucer
            var thrusterObj = new GameObject("ThrusterPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            thrusterObj.transform.SetParent(frameObj.transform, false);
            var thrustRect = thrusterObj.GetComponent<RectTransform>();
            thrustRect.anchorMin = new Vector2(0.5f, 0.5f);
            thrustRect.anchorMax = new Vector2(0.5f, 0.5f);
            thrustRect.pivot = new Vector2(1f, 0.5f);
            thrustRect.anchoredPosition = new Vector2(-60f, 2f);
            thrustRect.sizeDelta = new Vector2(75f, 44f);
            previewThruster = thrusterObj.GetComponent<Image>();
            previewThruster.preserveAspect = true;

            // Main Sprite Preview
            var spriteObj = new GameObject("SpritePreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spriteObj.transform.SetParent(frameObj.transform, false);
            var sprRect = spriteObj.GetComponent<RectTransform>();
            sprRect.anchorMin = new Vector2(0.5f, 0.5f);
            sprRect.anchorMax = new Vector2(0.5f, 0.5f);
            sprRect.pivot = new Vector2(0.5f, 0.5f);
            sprRect.anchoredPosition = new Vector2(10f, 2f);
            sprRect.sizeDelta = new Vector2(170f, 115f);
            previewImage = spriteObj.GetComponent<Image>();
            previewImage.preserveAspect = true;

            // Info Labels
            previewTitle = CreateText(previewPanel.transform, "PrevTitle", "ОРУЖИЕ", 23, new Vector2(0f, 32f), new Vector2(480f, 32f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            previewSubtitle = CreateText(previewPanel.transform, "PrevSub", "Категория", 15, new Vector2(0f, 4f), new Vector2(480f, 24f), new Color(0.45f, 0.85f, 1f), FontStyle.Normal);
            previewStats = CreateText(previewPanel.transform, "PrevStats", "УРОН: 1 | ТЕМП: 7.1 выстр/сек", 16, new Vector2(0f, -26f), new Vector2(480f, 26f), new Color(0.9f, 1f, 0.8f), FontStyle.Bold);
            previewDescription = CreateText(previewPanel.transform, "PrevDesc", "Описание технологии...", 14, new Vector2(0f, -88f), new Vector2(480f, 96f), new Color(0.88f, 0.92f, 0.98f), FontStyle.Normal);

            // Upgrade Button (Level 1 -> 2 -> 3)
            var upgObj = new GameObject("UpgradeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            upgObj.transform.SetParent(previewPanel.transform, false);
            var upgRect = upgObj.GetComponent<RectTransform>();
            upgRect.anchorMin = new Vector2(0.5f, 0f);
            upgRect.anchorMax = new Vector2(0.5f, 0f);
            upgRect.pivot = new Vector2(0.5f, 0f);
            upgRect.anchoredPosition = new Vector2(0f, 68f);
            upgRect.sizeDelta = new Vector2(380f, 44f);
            var upgImg = upgObj.GetComponent<Image>();
            upgImg.sprite = PixelUiAtlas.ButtonPrimary;
            upgImg.type = Image.Type.Sliced;
            upgradeButton = upgObj.GetComponent<Button>();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            upgradeButtonText = CreateText(upgObj.transform, "UpgText", "ПРОКАЧАТЬ ОРУЖИЕ", 16, Vector2.zero, new Vector2(360f, 36f), Color.white, FontStyle.Bold);
            UITween.AttachButtonFeedback(upgradeButton);

            // Action Button (Buy / Equip)
            var btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(previewPanel.transform, false);
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 18f);
            btnRect.sizeDelta = new Vector2(380f, 44f);
            var btnImg = btnObj.GetComponent<Image>();
            btnImg.sprite = PixelUiAtlas.ButtonPrimary;
            btnImg.type = Image.Type.Sliced;
            actionButton = btnObj.GetComponent<Button>();
            actionButton.onClick.AddListener(OnActionButtonClicked);
            actionButtonText = CreateText(btnObj.transform, "BtnText", "ВЫБРАТЬ", 18, Vector2.zero, new Vector2(360f, 36f), Color.white, FontStyle.Bold);
            UITween.AttachButtonFeedback(actionButton);

            // Return to Main Menu
            var backBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            backBtn.transform.SetParent(mainPanel.transform, false);
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 18f);
            backRect.sizeDelta = new Vector2(280f, 44f);
            var backImg = backBtn.GetComponent<Image>();
            backImg.sprite = PixelUiAtlas.ButtonSecondary;
            backImg.type = Image.Type.Sliced;
            var backButton = backBtn.GetComponent<Button>();
            backButton.onClick.AddListener(SceneLoader.ReturnToMainMenu);
            CreateText(backBtn.transform, "BackText", "← В ГЛАВНОЕ МЕНЮ", 17, Vector2.zero, new Vector2(260f, 36f), Color.white, FontStyle.Bold);
            UITween.AttachButtonFeedback(backButton);
        }

        private void SetTab(ShopTab tab)
        {
            currentTab = tab;
            Refresh();
        }

        private void Refresh()
        {
            if (coinsLabel != null)
                coinsLabel.text = CoinsText();

            // Tab Buttons coloring
            if (tabWeaponsBtn != null)
                tabWeaponsBtn.GetComponent<Image>().color = currentTab == ShopTab.Weapons ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 0.4f, 0.55f);
            if (tabSkinsBtn != null)
                tabSkinsBtn.GetComponent<Image>().color = currentTab == ShopTab.Skins ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 0.4f, 0.55f);
            if (tabTechBtn != null)
                tabTechBtn.GetComponent<Image>().color = currentTab == ShopTab.Tech ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 0.4f, 0.55f);

            // Clear old buttons
            for (var i = 0; i < activeListButtons.Count; i++)
                Destroy(activeListButtons[i]);
            activeListButtons.Clear();

            var save = SaveManager.Instance;
            if (!selectionReady && save != null)
            {
                selectionReady = true;
                selectedWeaponIndex = IndexOfId(WeaponCatalog.All, save.Data.equippedWeaponId, w => w.Id);
                selectedSkinIndex = IndexOfId(ShipSkinCatalog.All, save.Data.equippedSkinId, s => s.Id);
            }

            if (currentTab == ShopTab.Weapons)
            {
                var weapons = WeaponCatalog.All;
                selectedWeaponIndex = Mathf.Clamp(selectedWeaponIndex, 0, weapons.Length - 1);
                var equippedId = save != null ? save.Data.equippedWeaponId : "twin_blaster";

                for (var i = 0; i < weapons.Length; i++)
                {
                    var index = i;
                    var w = weapons[i];
                    var isOwned = save != null && save.Data.unlockedWeaponIds.Contains(w.Id);
                    var isEquipped = w.Id == equippedId;
                    var isSelected = index == selectedWeaponIndex;
                    var lvl = save != null ? save.GetWeaponLevel(w.Id) : 1;
                    var isLocked = !isOwned && save != null && !save.IsWeaponAccessible(w.Id);
                    var lockBadge = isLocked ? $"🔒 СЕК.{(w.RequiredMissionIndex / 3) + 1}" : null;
                    var itemTitle = isOwned ? $"{w.Title} [Ур.{lvl}]" : w.Title;

                    var btn = CreateListItem(listContainer, itemTitle, w.Subtitle, w.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedWeaponIndex = index;
                        Refresh();
                    }, isLocked, lockBadge);
                    activeListButtons.Add(btn);
                }

                DisplayWeaponDetails(weapons[selectedWeaponIndex]);
            }
            else if (currentTab == ShopTab.Skins)
            {
                var skins = ShipSkinCatalog.All;
                selectedSkinIndex = Mathf.Clamp(selectedSkinIndex, 0, skins.Length - 1);
                var equippedId = save != null ? save.Data.equippedSkinId : SaveManager.DefaultSkinId;

                for (var i = 0; i < skins.Length; i++)
                {
                    var index = i;
                    var s = skins[i];
                    var isOwned = save != null && save.Data.unlockedSkinIds.Contains(s.Id);
                    var isEquipped = s.Id == equippedId;
                    var isSelected = index == selectedSkinIndex;
                    var isLocked = !isOwned && save != null && !save.IsSkinAccessible(s.Id);
                    var lockBadge = isLocked ? $"🔒 СЕК.{(s.RequiredMissionIndex / 3) + 1}" : null;

                    string priceBadge = null;
                    if (!isOwned && !isLocked)
                    {
                        priceBadge = s.CoresPrice > 0 ? $"{s.Price}¢+{s.CoresPrice}🌟" : $"{s.Price}¢";
                    }

                    var btn = CreateListItem(listContainer, s.Title, s.Subtitle, s.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedSkinIndex = index;
                        Refresh();
                    }, isLocked, lockBadge, priceBadge);
                    activeListButtons.Add(btn);
                }

                DisplaySkinDetails(skins[selectedSkinIndex]);
            }
            else
            {
                var techs = MetaTechCatalog.All;
                selectedTechIndex = Mathf.Clamp(selectedTechIndex, 0, techs.Length - 1);

                for (var i = 0; i < techs.Length; i++)
                {
                    var index = i;
                    var t = techs[i];
                    var lvl = save != null ? save.GetMetaLevel(t.Id) : 1;
                    var isMax = lvl >= t.MaxLevel;
                    var isSelected = index == selectedTechIndex;
                    var (costCredits, costCores) = t.GetCost(lvl);

                    string badgeStr;
                    if (isMax)
                        badgeStr = "★ МАКС";
                    else if (costCores > 0)
                        badgeStr = $"{costCredits}¢+{costCores}🌟";
                    else
                        badgeStr = $"{costCredits}¢";

                    var itemTitle = $"{t.Title} [Ур.{lvl}/{t.MaxLevel}]";

                    var btn = CreateListItem(listContainer, itemTitle, t.Subtitle, costCredits, isMax, false, isSelected, () =>
                    {
                        selectedTechIndex = index;
                        Refresh();
                    }, false, null, badgeStr);
                    activeListButtons.Add(btn);
                }

                DisplayTechDetails(techs[selectedTechIndex]);
            }
        }

        private void DisplayWeaponDetails(WeaponDefinition weapon)
        {
            if (weapon == null) return;

            var save = SaveManager.Instance;
            var isOwned = save != null && save.Data.unlockedWeaponIds.Contains(weapon.Id);
            var isEquipped = save != null && save.Data.equippedWeaponId == weapon.Id;
            var level = save != null ? save.GetWeaponLevel(weapon.Id) : 1;

            if (previewThruster != null) previewThruster.gameObject.SetActive(false);
            if (previewGlow != null) previewGlow.gameObject.SetActive(false);

            if (previewImage != null)
            {
                previewImage.rectTransform.anchoredPosition = Vector2.zero;
                previewImage.rectTransform.sizeDelta = new Vector2(160f, 100f);
                var sprite = Resources.Load<Sprite>(weapon.ProjectileSprite);
                previewImage.sprite = sprite;
                previewImage.color = Color.white;
            }

            var currentDamage = WeaponCatalog.GetDamage(weapon.Id, level);
            var currentInterval = WeaponCatalog.GetFireInterval(weapon.Id, level);
            var rps = 1f / currentInterval;
            var stars = level == 3 ? "★★★" : (level == 2 ? "★★☆" : "★☆☆");

            if (previewTitle != null)
                previewTitle.text = isOwned ? $"{weapon.Title}  {stars} ({level}/3)" : weapon.Title;

            if (previewSubtitle != null)
            {
                if (!isOwned)
                    previewSubtitle.text = $"{weapon.Subtitle}  |  Базовый уровень (1/3)";
                else if (level < 3)
                    previewSubtitle.text = $"{weapon.Subtitle}  |  {WeaponCatalog.GetUpgradeSummary(weapon.Id, level)}";
                else
                    previewSubtitle.text = $"{weapon.Subtitle}  |  ★ МАКСИМАЛЬНАЯ МОЩНОСТЬ ★";
            }

            if (previewDescription != null)
                previewDescription.text = weapon.Description;

            if (previewStats != null)
            {
                previewStats.text = $"УРОН: {currentDamage}  |  ТЕМП: {rps:0.0} выстр/сек";
            }

            // Upgrade Button state
            if (upgradeButton != null)
            {
                if (isOwned)
                {
                    upgradeButton.gameObject.SetActive(true);
                    if (level < 3)
                    {
                        var cost = save != null ? save.GetUpgradeCost(weapon.Id) : 50;
                        var canAfford = save != null && save.Data.coins >= cost;
                        upgradeButtonText.text = canAfford
                            ? $"ПРОКАЧАТЬ ДО УР. {level + 1} ({cost} КРЕДИТОВ)"
                            : $"ДО УР. {level + 1} НЕ ХВАТАЕТ {Mathf.Max(0, cost - (save != null ? save.Data.coins : 0))} КРЕДИТОВ";
                        upgradeButton.interactable = true;
                        upgradeButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.72f, 0.15f) : new Color(0.45f, 0.35f, 0.25f);
                    }
                    else
                    {
                        upgradeButtonText.text = "★ МАКСИМАЛЬНЫЙ УРОВЕНЬ (3/3) ★";
                        upgradeButton.interactable = false;
                        upgradeButton.GetComponent<Image>().color = new Color(0.22f, 0.42f, 0.32f);
                    }
                }
                else
                {
                    upgradeButton.gameObject.SetActive(false);
                }
            }

            // Action Button (Equip / Buy)
            if (isEquipped)
            {
                actionButtonText.text = "УЖЕ В БОЮ";
                actionButton.interactable = false;
                actionButton.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
                ShowHint("Это оружие уже стоит на тарелке.");
            }
            else if (isOwned)
            {
                actionButtonText.text = "ЭКИПИРОВАТЬ";
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
                ShowHint("Можно надеть перед вылетом. Урон зависит от уровня.");
            }
            else
            {
                var isAccessible = save == null || save.IsWeaponAccessible(weapon.Id);
                if (!isAccessible)
                {
                    var reqMission = weapon.RequiredMissionIndex + 1;
                    var reqSector = (weapon.RequiredMissionIndex / 3) + 1;
                    actionButtonText.text = $"🔒 МИССИЯ {reqMission}";
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = new Color(0.35f, 0.25f, 0.25f);
                    ShowHint($"Откроется в секторе {reqSector}, когда дойдёшь до миссии {reqMission}.");
                }
                else
                {
                    var coins = save != null ? save.Data.coins : 0;
                    var canAfford = coins >= weapon.Price;
                    actionButtonText.text = canAfford ? $"КУПИТЬ ЗА {weapon.Price} КРЕДИТОВ" : $"НЕ ХВАТАЕТ {weapon.Price - coins} КРЕДИТОВ";
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.55f, 0.28f, 0.28f);
                    ShowHint(canAfford
                        ? "Покупка сразу установит оружие на корабль."
                        : $"На счету {coins}. До этой пушки ещё {weapon.Price - coins} кредитов.");
                }
            }
        }

        private void DisplaySkinDetails(ShipSkin skin)
        {
            if (skin == null) return;

            var save = SaveManager.Instance;
            var isOwned = save != null && save.Data.unlockedSkinIds.Contains(skin.Id);
            var isEquipped = save != null && save.Data.equippedSkinId == skin.Id;

            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);

            if (previewImage != null)
            {
                previewImage.rectTransform.anchoredPosition = new Vector2(12f, 2f);
                previewImage.rectTransform.sizeDelta = new Vector2(170f, 115f);
                var sprite = Resources.Load<Sprite>(skin.SpriteName);
                previewImage.sprite = sprite;
                previewImage.color = Color.white;
            }

            if (previewThruster != null)
            {
                previewThruster.gameObject.SetActive(true);
                var thrusterSprite = Resources.Load<Sprite>(skin.ThrusterSprite);
                if (thrusterSprite == null)
                    thrusterSprite = Resources.Load<Sprite>("Sprites/thruster");
                previewThruster.sprite = thrusterSprite;
                previewThruster.color = skin.ThrusterColor;
            }

            if (previewGlow != null)
            {
                previewGlow.gameObject.SetActive(true);
                previewGlow.color = skin.AntiGravColor;
            }

            if (previewTitle != null) previewTitle.text = skin.Title;
            if (previewSubtitle != null) previewSubtitle.text = skin.Subtitle;
            if (previewDescription != null) previewDescription.text = skin.Description;
            if (previewStats != null) previewStats.text = "ТИП: Летающая тарелка (UFO) с пилотом в кабине";

            if (isEquipped)
            {
                actionButtonText.text = "УЖЕ В БОЮ";
                actionButton.interactable = false;
                actionButton.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
                ShowHint("Этот корпус уже надет. Цвет двигателя виден в рамке сверху.");
            }
            else if (isOwned)
            {
                actionButtonText.text = "ЭКИПИРОВАТЬ";
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
                ShowHint("Корпус не меняет урон, только вид тарелки и пламя.");
            }
            else
            {
                var isAccessible = save == null || save.IsSkinAccessible(skin.Id);
                if (!isAccessible)
                {
                    var reqSector = (skin.RequiredMissionIndex / 3) + 1;
                    var reqMission = skin.RequiredMissionIndex + 1;
                    actionButtonText.text = $"🔒 МИССИЯ {reqMission}";
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = new Color(0.35f, 0.25f, 0.25f);
                    ShowHint($"Корпус откроется с миссии {reqMission}, сектор {reqSector}.");
                }
                else
                {
                    var coins = save != null ? save.Data.coins : 0;
                    var cores = save != null && save.Data != null ? save.Data.starCores : 0;
                    var canAfford = coins >= skin.Price && cores >= skin.CoresPrice;
                    string priceText = skin.CoresPrice > 0 ? $"{skin.Price} КРЕДИТОВ + {skin.CoresPrice} ЯДЕР" : $"{skin.Price} КРЕДИТОВ";
                    actionButtonText.text = canAfford ? $"КУПИТЬ ЗА {priceText}" : $"НЕ ХВАТАЕТ РЕСУРСОВ ({priceText})";
                    actionButton.interactable = canAfford;
                    actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.55f, 0.28f, 0.28f);
                    ShowHint(canAfford
                        ? "После покупки корпус сразу активируется."
                        : $"Требуется {priceText}. Баланс: {coins} кредитов, {cores} ядер.");
                }
            }
        }

        private void DisplayTechDetails(MetaTechDefinition tech)
        {
            if (tech == null) return;

            var save = SaveManager.Instance;
            var lvl = save != null ? save.GetMetaLevel(tech.Id) : 1;
            var isMax = lvl >= tech.MaxLevel;

            if (previewThruster != null) previewThruster.gameObject.SetActive(false);
            if (previewGlow != null) previewGlow.gameObject.SetActive(false);

            if (previewImage != null)
            {
                previewImage.rectTransform.anchoredPosition = Vector2.zero;
                previewImage.rectTransform.sizeDelta = new Vector2(120f, 120f);
                var sprite = Resources.Load<Sprite>(tech.SpriteName);
                if (sprite == null) sprite = Resources.Load<Sprite>("Sprites/spark");
                previewImage.sprite = sprite;
                previewImage.color = tech.ThemeColor;
            }

            if (previewTitle != null)
                previewTitle.text = $"{tech.Title}  [Ур. {lvl}/{tech.MaxLevel}]";

            if (previewSubtitle != null)
                previewSubtitle.text = tech.Subtitle;

            if (previewStats != null)
            {
                if (isMax)
                    previewStats.text = $"ТЕКУЩИЙ ЭФФЕКТ: {tech.GetStatDescription(lvl)} (МАКСИМУМ)";
                else
                    previewStats.text = $"ТЕКУЩИЙ: {tech.GetStatDescription(lvl)}\nСЛЕДУЮЩИЙ: {tech.GetStatDescription(lvl + 1)}";
            }

            if (previewDescription != null)
                previewDescription.text = tech.Description;

            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);

            if (actionButton != null)
            {
                if (isMax)
                {
                    actionButtonText.text = "★ МАКСИМАЛЬНЫЙ УРОВЕНЬ ★";
                    actionButton.interactable = false;
                    actionButton.GetComponent<Image>().color = new Color(0.22f, 0.42f, 0.32f);
                    ShowHint("Эта модернизация исследована на максимум мощности.");
                }
                else
                {
                    var (costCredits, costCores) = tech.GetCost(lvl);
                    var canAfford = save != null && save.Data.coins >= costCredits && save.Data.starCores >= costCores;
                    string costStr = costCores > 0 ? $"{costCredits} КРЕДИТОВ + {costCores} ЯДЕР" : $"{costCredits} КРЕДИТОВ";

                    actionButtonText.text = canAfford ? $"МОДЕРНИЗИРОВАТЬ ДО УР. {lvl + 1} ({costStr})" : $"НЕ ХВАТАЕТ РЕСУРСОВ ({costStr})";
                    actionButton.interactable = canAfford;
                    actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.65f, 0.15f) : new Color(0.55f, 0.3f, 0.25f);
                    ShowHint(canAfford
                        ? "Улучшение действует перманентно во всех режимах игры."
                        : $"Для модернизации требуется {costStr}.");
                }
            }
        }

        private void OnUpgradeButtonClicked()
        {
            var save = SaveManager.Instance;
            if (save == null || currentTab != ShopTab.Weapons) return;

            var w = WeaponCatalog.All[selectedWeaponIndex];
            if (save.UpgradeWeapon(w.Id))
            {
                AudioManager.PlayShoot();
                ShowHint($"Оружие улучшено до уровня {save.GetWeaponLevel(w.Id)}.");
                Refresh();
                return;
            }

            var cost = save.GetUpgradeCost(w.Id);
            ShowHint(cost < 0
                ? "Дальше улучшать некуда."
                : $"На прокачку нужно {cost} монет, не хватает {Mathf.Max(0, cost - save.Data.coins)}.");
            AudioManager.PlayHit();
        }

        private void OnActionButtonClicked()
        {
            var save = SaveManager.Instance;
            if (save == null) return;

            if (currentTab == ShopTab.Tech)
            {
                var tech = MetaTechCatalog.All[selectedTechIndex];
                if (save.TryUpgradeMeta(tech.Id))
                {
                    AudioManager.PlayPowerup();
                    ShowHint($"{tech.Title} улучшен до уровня {save.GetMetaLevel(tech.Id)}!");
                    Refresh();
                }
                else
                {
                    AudioManager.PlayHit();
                    ShowHint("Недостаточно кредитов или Звёздных Ядер для модернизации.");
                }
                return;
            }

            if (currentTab == ShopTab.Weapons)
            {
                var w = WeaponCatalog.All[selectedWeaponIndex];
                var isOwned = save.Data.unlockedWeaponIds.Contains(w.Id);
                if (!isOwned)
                {
                    if (!save.IsWeaponAccessible(w.Id))
                    {
                        ShowHint($"Сначала дойди до миссии {w.RequiredMissionIndex + 1}.");
                        AudioManager.PlayHit();
                        return;
                    }

                    if (save.TrySpendCoins(w.Price))
                    {
                        save.UnlockWeapon(w.Id);
                        save.EquipWeapon(w.Id);
                        ShowHint($"{w.Title} куплено и надето. Осталось {save.Data.coins} монет.");
                        AudioManager.PlayPowerup();
                    }
                    else
                    {
                        ShowHint($"Не хватает {w.Price - save.Data.coins} монет.");
                        AudioManager.PlayHit();
                        return;
                    }
                }
                else
                {
                    save.EquipWeapon(w.Id);
                    ShowHint($"{w.Title} теперь в бою.");
                    AudioManager.PlayClick();
                }
            }
            else
            {
                var s = ShipSkinCatalog.All[selectedSkinIndex];
                var isOwned = save.Data.unlockedSkinIds.Contains(s.Id);
                if (!isOwned)
                {
                    if (!save.IsSkinAccessible(s.Id))
                    {
                        ShowHint($"Корпус откроется с миссии {s.RequiredMissionIndex + 1}.");
                        AudioManager.PlayHit();
                        return;
                    }

                    if (s.CoresPrice > 0 && save.Data.starCores < s.CoresPrice)
                    {
                        ShowHint($"Не хватает {s.CoresPrice - save.Data.starCores} Звёздных Ядер.");
                        AudioManager.PlayHit();
                        return;
                    }

                    if (save.Data.coins >= s.Price && save.Data.starCores >= s.CoresPrice)
                    {
                        save.Data.coins -= s.Price;
                        save.Data.starCores -= s.CoresPrice;
                        save.UnlockSkin(s.Id);
                        save.EquipSkin(s.Id);
                        save.Save();
                        ShowHint($"{s.Title} куплен и надет. Баланс: {save.Data.coins} кредитов, {save.Data.starCores} ядер.");
                        AudioManager.PlayPowerup();
                    }
                    else
                    {
                        ShowHint($"Не хватает {s.Price - save.Data.coins} кредитов.");
                        AudioManager.PlayHit();
                        return;
                    }
                }
                else
                {
                    save.EquipSkin(s.Id);
                    ShowHint($"{s.Title} теперь на тарелке.");
                    AudioManager.PlayClick();
                }
            }

            Refresh();
        }

        private GameObject CreateListItem(Transform parent, string title, string sub, int price, bool isOwned, bool isEquipped, bool isSelected, System.Action onClick, bool isLocked = false, string lockBadge = null, string customBadge = null)
        {
            var btnObj = new GameObject("ItemBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnObj.transform.SetParent(parent, false);

            var le = btnObj.GetComponent<LayoutElement>();
            le.minHeight = 64f;
            le.preferredHeight = 64f;

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonSecondary;
            img.type = Image.Type.Sliced;
            img.color = isSelected 
                ? new Color(0.3f, 0.65f, 0.95f, 1f) 
                : new Color(0.12f, 0.18f, 0.28f, 0.9f);

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Left Title & Subtitle container
            var titleText = CreateText(btnObj.transform, "T", title, 16, Vector2.zero, new Vector2(300f, 26f), Color.white, FontStyle.Bold);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            titleText.rectTransform.anchorMax = new Vector2(0.68f, 0.5f);
            titleText.rectTransform.pivot = new Vector2(0f, 0.5f);
            titleText.rectTransform.anchoredPosition = new Vector2(16f, 12f);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            var subText = CreateText(btnObj.transform, "S", sub, 13, Vector2.zero, new Vector2(300f, 22f), new Color(0.65f, 0.85f, 1f), FontStyle.Normal);
            subText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            subText.rectTransform.anchorMax = new Vector2(0.68f, 0.5f);
            subText.rectTransform.pivot = new Vector2(0f, 0.5f);
            subText.rectTransform.anchoredPosition = new Vector2(16f, -14f);
            subText.alignment = TextAnchor.MiddleLeft;
            subText.horizontalOverflow = HorizontalWrapMode.Overflow;

            // Right Status Badge
            string statusStr;
            Color statusCol;
            if (isEquipped)
            {
                statusStr = "★ В БОЮ";
                statusCol = new Color(0.4f, 1f, 0.5f);
            }
            else if (isOwned)
            {
                statusStr = "В АНГАРЕ";
                statusCol = new Color(0.85f, 0.95f, 1f);
            }
            else if (isLocked)
            {
                statusStr = lockBadge ?? "🔒 БЛОК";
                statusCol = new Color(0.95f, 0.45f, 0.45f);
            }
            else if (!string.IsNullOrEmpty(customBadge))
            {
                statusStr = customBadge;
                statusCol = new Color(1f, 0.85f, 0.3f);
            }
            else
            {
                statusStr = $"{price} 💰";
                statusCol = new Color(1f, 0.85f, 0.3f);
            }

            var badge = CreateText(btnObj.transform, "B", statusStr, 15, Vector2.zero, new Vector2(160f, 28f), statusCol, FontStyle.Bold);
            badge.rectTransform.anchorMin = new Vector2(0.68f, 0.5f);
            badge.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            badge.rectTransform.pivot = new Vector2(1f, 0.5f);
            badge.rectTransform.anchoredPosition = new Vector2(-16f, 0f);
            badge.alignment = TextAnchor.MiddleRight;
            badge.horizontalOverflow = HorizontalWrapMode.Overflow;

            return btnObj;
        }

        private static Button CreateTabButton(Transform parent, string name, string label, Vector2 pos, System.Action onClick)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(380f, 44f);

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonPrimary;
            img.type = Image.Type.Sliced;

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            CreateText(btnObj.transform, "T", label, 18, Vector2.zero, new Vector2(360f, 36f), Color.white, FontStyle.Bold);
            return btn;
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 pos, Vector2 size, Color color, FontStyle fontStyle)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var label = go.GetComponent<Text>();
            label.font = UIFont.DefaultFont;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static void ShowHint(string message)
        {
            if (FindAnyObjectByType<ShopScreen>() is ShopScreen shop && shop.hintLabel != null)
                shop.hintLabel.text = message;
        }

        private static int IndexOfId<T>(T[] items, string id, System.Func<T, string> readId)
        {
            if (items == null || items.Length == 0)
                return 0;

            for (var i = 0; i < items.Length; i++)
            {
                if (readId(items[i]) == id)
                    return i;
            }

            return 0;
        }

        private static string CoinsText()
        {
            var save = SaveManager.Instance;
            var coins = save != null ? save.Data.coins : 0;
            return $"БАЛАНС: {coins} КРЕДИТОВ    {NextDeal(save, coins)}";
        }

        private static string NextDeal(SaveManager save, int coins)
        {
            if (save == null)
                return "Войдите в сектор, чтобы сохранить прогресс.";

            ShipSkin cheapest = null;
            for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
            {
                var skin = ShipSkinCatalog.All[i];
                if (save.Data.unlockedSkinIds.Contains(skin.Id) || skin.Price <= 0)
                    continue;
                if (!save.IsSkinAccessible(skin.Id))
                    continue;
                if (cheapest == null || skin.Price < cheapest.Price)
                    cheapest = skin;
            }

            if (cheapest == null)
                return "Все доступные корпуса разблокированы.";

            if (coins >= cheapest.Price)
                return $"Доступно для покупки: «{cheapest.Title}» за {cheapest.Price} кредитов.";

            return $"До «{cheapest.Title}» ещё {cheapest.Price - coins} кредитов.";
        }
    }
}
