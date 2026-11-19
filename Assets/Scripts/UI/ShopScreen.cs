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
            Skins
        }

        private ShopTab currentTab = ShopTab.Weapons;
        private int selectedWeaponIndex = 0;
        private int selectedSkinIndex = 0;

        private Text headerTitleLabel;
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
        private Text tabWeaponsText;
        private Button tabSkinsBtn;
        private Text tabSkinsText;
        private Text backButtonText;

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
            Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= Refresh;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            if (headerTitleLabel != null)
                headerTitleLabel.text = Localization.T("АНГАР И АРСЕНАЛ КАПИТАНА ЗИГГИ", "CAPTAIN ZIGGY'S HANGAR & ARSENAL");
            if (tabWeaponsText != null)
                tabWeaponsText.text = Localization.T("🚀 ОРУЖЕЙНЫЙ АРСЕНАЛ", "🚀 WEAPON ARSENAL");
            if (tabSkinsText != null)
                tabSkinsText.text = Localization.T("🛸 КОРПУСА ТАРЕЛКИ", "🛸 SAUCER HULLS");
            if (backButtonText != null)
                backButtonText.text = Localization.T("В ГЛАВНОЕ МЕНЮ", "MAIN MENU");
            Refresh();
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
            scaler.dynamicPixelsPerUnit = 1.25f;

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
            mainRect.sizeDelta = new Vector2(1160f, 780f);
            var panelImg = mainPanel.GetComponent<Image>();
            panelImg.sprite = PixelUiAtlas.Panel;
            panelImg.type = Image.Type.Sliced;

            // Header Title & Status line with clear, generous spacing
            headerTitleLabel = CreateText(mainPanel.transform, "HeaderTitle", Localization.T("АНГАР И АРСЕНАЛ КАПИТАНА ЗИГГИ", "CAPTAIN ZIGGY'S HANGAR & ARSENAL"), 28, new Vector2(0f, 345f), new Vector2(900f, 36f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            coinsLabel = CreateText(mainPanel.transform, "CoinsLabel", CoinsText(), 20, new Vector2(0f, 312f), new Vector2(940f, 26f), new Color(1f, 0.95f, 0.7f), FontStyle.Bold);
            hintLabel = CreateText(mainPanel.transform, "HintLabel", Localization.T("Выбери корпус или оружие справа от списка.", "Select a hull or weapon from the list on the left."), 15, new Vector2(0f, 282f), new Vector2(980f, 24f), new Color(0.75f, 0.9f, 1f), FontStyle.Normal);

            // Tab Buttons (distinct 14px gap above the panels)
            tabWeaponsBtn = CreateTabButton(mainPanel.transform, "TabWeapons", Localization.T("🚀 ОРУЖЕЙНЫЙ АРСЕНАЛ", "🚀 WEAPON ARSENAL"), new Vector2(-220f, 238f), () => SetTab(ShopTab.Weapons), out tabWeaponsText);
            tabSkinsBtn = CreateTabButton(mainPanel.transform, "TabSkins", Localization.T("🛸 КОРПУСА ТАРЕЛКИ", "🛸 SAUCER HULLS"), new Vector2(220f, 238f), () => SetTab(ShopTab.Skins), out tabSkinsText);

            // Left Side: Scrollable List Viewport
            var listPanel = new GameObject("ListPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            listPanel.transform.SetParent(mainPanel.transform, false);
            var listRect = listPanel.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0.5f);
            listRect.anchorMax = new Vector2(0f, 0.5f);
            listRect.pivot = new Vector2(0f, 0.5f);
            listRect.anchoredPosition = new Vector2(40f, -36f);
            listRect.sizeDelta = new Vector2(510f, 484f);

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
            vlg.spacing = 10f;
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

            // Right Side: Card Preview & Specs (Spacious 484px panel with clean button margins)
            var previewPanel = new GameObject("PreviewPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewPanel.transform.SetParent(mainPanel.transform, false);
            var prevRect = previewPanel.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(1f, 0.5f);
            prevRect.anchorMax = new Vector2(1f, 0.5f);
            prevRect.pivot = new Vector2(1f, 0.5f);
            prevRect.anchoredPosition = new Vector2(-40f, -36f);
            prevRect.sizeDelta = new Vector2(530f, 484f);
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
            frameRect.anchoredPosition = new Vector2(0f, -14f);
            frameRect.sizeDelta = new Vector2(490f, 136f);
            var frameBg = frameObj.GetComponent<Image>();
            frameBg.sprite = PixelUiAtlas.Panel;
            frameBg.type = Image.Type.Sliced;
            frameBg.color = new Color(0.04f, 0.07f, 0.14f, 0.98f);

            // Thruster Glow underlay
            var glowObj = new GameObject("ThrusterGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowObj.transform.SetParent(frameObj.transform, false);
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = new Vector2(12f, -10f);
            glowRect.sizeDelta = new Vector2(120f, 50f);
            previewGlow = glowObj.GetComponent<Image>();
            previewGlow.sprite = PixelUiAtlas.White;
            previewGlow.color = new Color(0.3f, 0.8f, 1f, 0.25f);

            // Thruster Flame Image
            var thrustObj = new GameObject("ThrusterFlame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            thrustObj.transform.SetParent(frameObj.transform, false);
            var thrustRect = thrustObj.GetComponent<RectTransform>();
            thrustRect.anchorMin = new Vector2(0.5f, 0.5f);
            thrustRect.anchorMax = new Vector2(0.5f, 0.5f);
            thrustRect.pivot = new Vector2(0.5f, 0.5f);
            thrustRect.anchoredPosition = new Vector2(12f, -22f);
            thrustRect.sizeDelta = new Vector2(75f, 40f);
            previewThruster = thrustObj.GetComponent<Image>();
            previewThruster.preserveAspect = true;

            // Centered Main Sprite
            var spriteObj = new GameObject("PreviewSprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spriteObj.transform.SetParent(frameObj.transform, false);
            var sprRect = spriteObj.GetComponent<RectTransform>();
            sprRect.anchorMin = new Vector2(0.5f, 0.5f);
            sprRect.anchorMax = new Vector2(0.5f, 0.5f);
            sprRect.pivot = new Vector2(0.5f, 0.5f);
            sprRect.anchoredPosition = new Vector2(10f, 2f);
            sprRect.sizeDelta = new Vector2(170f, 115f);
            previewImage = spriteObj.GetComponent<Image>();
            previewImage.preserveAspect = true;

            // Info Labels (Well-proportioned vertical hierarchy with clear breathing margins)
            previewTitle = CreateText(previewPanel.transform, "PrevTitle", Localization.T("ОРУЖИЕ", "WEAPON"), 22, new Vector2(0f, 62f), new Vector2(490f, 30f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
            previewSubtitle = CreateText(previewPanel.transform, "PrevSub", Localization.T("Категория", "Category"), 14, new Vector2(0f, 28f), new Vector2(490f, 28f), new Color(0.45f, 0.85f, 1f), FontStyle.Normal);
            previewStats = CreateText(previewPanel.transform, "PrevStats", Localization.T("УРОН: 1 | ТЕМП: 7.1 выстр/сек", "DAMAGE: 1 | FIRE RATE: 7.1 shots/sec"), 15, new Vector2(0f, -4f), new Vector2(490f, 26f), new Color(0.9f, 1f, 0.8f), FontStyle.Bold);
            previewDescription = CreateText(previewPanel.transform, "PrevDesc", Localization.T("Описание технологии...", "Technology specifications..."), 13, new Vector2(0f, -50f), new Vector2(490f, 54f), new Color(0.88f, 0.92f, 0.98f), FontStyle.Normal);

            // Upgrade Button (Level 1 -> 2 -> 3) with a solid 14px gap above ActionButton
            var upgObj = new GameObject("UpgradeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            upgObj.transform.SetParent(previewPanel.transform, false);
            var upgRect = upgObj.GetComponent<RectTransform>();
            upgRect.anchorMin = new Vector2(0.5f, 0f);
            upgRect.anchorMax = new Vector2(0.5f, 0f);
            upgRect.pivot = new Vector2(0.5f, 0f);
            upgRect.anchoredPosition = new Vector2(0f, 74f);
            upgRect.sizeDelta = new Vector2(400f, 44f);
            var upgImg = upgObj.GetComponent<Image>();
            upgImg.sprite = PixelUiAtlas.ButtonPrimary;
            upgImg.type = Image.Type.Sliced;
            upgradeButton = upgObj.GetComponent<Button>();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            upgradeButtonText = CreateText(upgObj.transform, "UpgText", Localization.T("ПРОКАЧАТЬ ОРУЖИЕ", "UPGRADE WEAPON"), 16, Vector2.zero, new Vector2(380f, 36f), Color.white, FontStyle.Bold);

            // Action Button (Buy / Equip) positioned cleanly at the bottom
            var btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(previewPanel.transform, false);
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 18f);
            btnRect.sizeDelta = new Vector2(400f, 44f);
            var btnImg = btnObj.GetComponent<Image>();
            btnImg.sprite = PixelUiAtlas.ButtonPrimary;
            btnImg.type = Image.Type.Sliced;
            actionButton = btnObj.GetComponent<Button>();
            actionButton.onClick.AddListener(OnActionButtonClicked);
            actionButtonText = CreateText(btnObj.transform, "BtnText", Localization.T("ВЫБРАТЬ", "SELECT"), 18, Vector2.zero, new Vector2(380f, 36f), Color.white, FontStyle.Bold);

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
            backBtn.GetComponent<Button>().onClick.AddListener(SceneLoader.ReturnToMainMenu);
            backButtonText = CreateText(backBtn.transform, "BackText", Localization.T("В ГЛАВНОЕ МЕНЮ", "MAIN MENU"), 18, Vector2.zero, new Vector2(260f, 36f), Color.white, FontStyle.Bold);
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
                    var lockBadge = isLocked ? Localization.TFormat("🔒 СЕК.{0}", "🔒 SEC.{0}", (w.RequiredMissionIndex / 3) + 1) : null;
                    var itemTitle = isOwned ? $"{w.LocalizedTitle} [{Localization.T("Ур.", "Lv.")}{lvl}]" : w.LocalizedTitle;

                    var btn = CreateListItem(listContainer, itemTitle, w.LocalizedSubtitle, w.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedWeaponIndex = index;
                        Refresh();
                    }, isLocked, lockBadge);
                    activeListButtons.Add(btn);
                }

                DisplayWeaponDetails(weapons[selectedWeaponIndex]);
            }
            else
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
                    var lockBadge = isLocked ? Localization.TFormat("🔒 СЕК.{0}", "🔒 SEC.{0}", (s.RequiredMissionIndex / 3) + 1) : null;

                    var btn = CreateListItem(listContainer, s.LocalizedTitle, s.LocalizedSubtitle, s.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedSkinIndex = index;
                        Refresh();
                    }, isLocked, lockBadge);
                    activeListButtons.Add(btn);
                }

                DisplaySkinDetails(skins[selectedSkinIndex]);
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
                previewTitle.text = isOwned ? $"{weapon.LocalizedTitle}  {stars} ({level}/3)" : weapon.LocalizedTitle;

            if (previewSubtitle != null)
            {
                if (!isOwned)
                    previewSubtitle.text = Localization.TFormat("{0}  |  Базовый уровень (1/3)", "{0}  |  Base Level (1/3)", weapon.LocalizedSubtitle);
                else if (level < 3)
                    previewSubtitle.text = $"{weapon.LocalizedSubtitle}  |  {WeaponCatalog.GetUpgradeSummary(weapon.Id, level)}";
                else
                    previewSubtitle.text = Localization.TFormat("{0}  |  ★ МАКСИМАЛЬНАЯ МОЩНОСТЬ ★", "{0}  |  ★ MAXIMUM POWER ★", weapon.LocalizedSubtitle);
            }

            if (previewDescription != null)
                previewDescription.text = weapon.LocalizedDescription;

            if (previewStats != null)
            {
                previewStats.text = Localization.TFormat("УРОН: {0}  |  ТЕМП: {1:0.0} выстр/сек", "DAMAGE: {0}  |  FIRE RATE: {1:0.0} shots/sec", currentDamage, rps);
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
                            ? Localization.TFormat("ПРОКАЧАТЬ ДО УР. {0} ({1} 💰)", "UPGRADE TO LV. {0} ({1} 💰)", level + 1, cost)
                            : Localization.TFormat("ДО УР. {0} НЕ ХВАТАЕТ {1} 💰", "NEED {1} MORE 💰 FOR LV. {0}", level + 1, Mathf.Max(0, cost - (save != null ? save.Data.coins : 0)));
                        upgradeButton.interactable = true;
                        upgradeButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.72f, 0.15f) : new Color(0.45f, 0.35f, 0.25f);
                    }
                    else
                    {
                        upgradeButtonText.text = Localization.T("★ МАКСИМАЛЬНЫЙ УРОВЕНЬ (3/3) ★", "★ MAXIMUM LEVEL (3/3) ★");
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
                actionButtonText.text = Localization.T("УЖЕ В БОЮ", "EQUIPPED");
                actionButton.interactable = false;
                actionButton.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
                ShowHint(Localization.T("Это оружие уже стоит на тарелке.", "This weapon is currently equipped on the saucer."));
            }
            else if (isOwned)
            {
                actionButtonText.text = Localization.T("ЭКИПИРОВАТЬ", "EQUIP");
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
                ShowHint(Localization.T("Можно надеть перед вылетом. Урон зависит от уровня.", "Ready to equip before flight. Damage scales with upgrade level."));
            }
            else
            {
                var isAccessible = save == null || save.IsWeaponAccessible(weapon.Id);
                if (!isAccessible)
                {
                    var reqMission = weapon.RequiredMissionIndex + 1;
                    var reqSector = (weapon.RequiredMissionIndex / 3) + 1;
                    actionButtonText.text = Localization.TFormat("🔒 МИССИЯ {0}", "🔒 MISSION {0}", reqMission);
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = new Color(0.35f, 0.25f, 0.25f);
                    ShowHint(Localization.TFormat("Откроется в секторе {0}, когда дойдёшь до миссии {1}.", "Unlocks in Sector {0} upon reaching Mission {1}.", reqSector, reqMission));
                }
                else
                {
                    var coins = save != null ? save.Data.coins : 0;
                    var canAfford = coins >= weapon.Price;
                    actionButtonText.text = canAfford
                        ? Localization.TFormat("КУПИТЬ ЗА {0} 💰", "BUY FOR {0} 💰", weapon.Price)
                        : Localization.TFormat("НЕ ХВАТАЕТ {0} 💰", "NEED {0} MORE 💰", weapon.Price - coins);
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.55f, 0.28f, 0.28f);
                    ShowHint(canAfford
                        ? Localization.T("Покупка сразу поставит оружие в бой.", "Purchasing will immediately equip this weapon.")
                        : Localization.TFormat("На счету {0}. До этой пушки ещё {1} монет.", "You have {0} coins. Need {1} more to acquire this weapon.", coins, weapon.Price - coins));
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

            if (previewTitle != null) previewTitle.text = isOwned ? $"{skin.LocalizedTitle} ★" : skin.LocalizedTitle;
            if (previewSubtitle != null)
            {
                if (isOwned)
                    previewSubtitle.text = Localization.T("В ангаре Зигги  |  Готов к полету", "In Ziggy's Hangar  |  Ready for flight");
                else if (save != null && !save.IsSkinAccessible(skin.Id))
                    previewSubtitle.text = Localization.TFormat("Требуется: Пройти миссию {0}", "Requires: Complete Mission {0}", skin.RequiredMissionIndex);
                else
                    previewSubtitle.text = Localization.T("Доступен к покупке за монеты", "Available for purchase");
            }
            if (previewDescription != null) previewDescription.text = skin.LocalizedDescription;
            if (previewStats != null) previewStats.text = Localization.T("ТИП: Летающая тарелка (UFO) с пилотом в кабине", "TYPE: Flying Saucer (UFO) with pilot in cockpit");

            if (isEquipped)
            {
                actionButtonText.text = Localization.T("УЖЕ В БОЮ", "EQUIPPED");
                actionButton.interactable = false;
                actionButton.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
                ShowHint(Localization.T("Этот корпус уже надет. Цвет двигателя виден в рамке сверху.", "This hull is already active. Thruster trail is previewed above."));
            }
            else if (isOwned)
            {
                actionButtonText.text = Localization.T("ЭКИПИРОВАТЬ", "EQUIP");
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
                ShowHint(Localization.T("Корпус не меняет урон, только вид тарелки и пламя.", "Hulls customize saucer visuals and thruster trail effects."));
            }
            else
            {
                var isAccessible = save == null || save.IsSkinAccessible(skin.Id);
                if (!isAccessible)
                {
                    var reqSector = (skin.RequiredMissionIndex / 3) + 1;
                    var reqMission = skin.RequiredMissionIndex + 1;
                    actionButtonText.text = Localization.TFormat("🔒 МИССИЯ {0}", "🔒 MISSION {0}", reqMission);
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = new Color(0.35f, 0.25f, 0.25f);
                    ShowHint(Localization.TFormat("Корпус откроется с миссии {0}, сектор {1}.", "Hull unlocks at Mission {0}, Sector {1}.", reqMission, reqSector));
                }
                else
                {
                    var coins = save != null ? save.Data.coins : 0;
                    var canAfford = coins >= skin.Price;
                    actionButtonText.text = canAfford
                        ? Localization.TFormat("КУПИТЬ ЗА {0} 💰", "BUY FOR {0} 💰", skin.Price)
                        : Localization.TFormat("НЕ ХВАТАЕТ {0} 💰", "NEED {0} MORE 💰", skin.Price - coins);
                    actionButton.interactable = true;
                    actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.55f, 0.28f, 0.28f);
                    ShowHint(canAfford
                        ? Localization.T("После покупки корпус сразу надевается.", "Purchased hull will be equipped immediately.")
                        : Localization.TFormat("На счету {0}. До корпуса ещё {1} монет.", "You have {0} coins. Need {1} more to unlock this hull.", coins, skin.Price - coins));
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
                ShowHint(Localization.TFormat("Оружие улучшено до уровня {0}.", "Weapon upgraded to Level {0}.", save.GetWeaponLevel(w.Id)));
                Refresh();
                return;
            }

            var cost = save.GetUpgradeCost(w.Id);
            if (cost < 0)
                ShowHint(Localization.T("Дальше улучшать некуда.", "Maximum upgrade level reached."));
            else
                ShowHint(Localization.TFormat("На прокачку нужно {0} монет, не хватает {1}.", "Upgrade costs {0} coins, need {1} more.", cost, Mathf.Max(0, cost - save.Data.coins)));
        }

        private void OnActionButtonClicked()
        {
            var save = SaveManager.Instance;
            if (save == null) return;

            if (currentTab == ShopTab.Weapons)
            {
                var w = WeaponCatalog.All[selectedWeaponIndex];
                if (!save.IsWeaponAccessible(w.Id))
                {
                    ShowHint(Localization.TFormat("Сначала дойди до миссии {0}.", "Reach Mission {0} first.", w.RequiredMissionIndex + 1));
                    return;
                }

                if (!save.Data.unlockedWeaponIds.Contains(w.Id))
                {
                    if (save.UnlockWeapon(w.Id))
                    {
                        save.EquipWeapon(w.Id);
                        AudioManager.PlayPowerup();
                        ShowHint(Localization.TFormat("{0} куплено и надето. Осталось {1} монет.", "\"{0}\" purchased and equipped. Remaining: {1} coins.", w.LocalizedTitle, save.Data.coins));
                        Refresh();
                    }
                    else
                    {
                        ShowHint(Localization.TFormat("Не хватает {0} монет.", "Need {0} more coins.", w.Price - save.Data.coins));
                    }
                    return;
                }

                save.EquipWeapon(w.Id);
                AudioManager.PlayClick();
                ShowHint(Localization.TFormat("{0} теперь в бою.", "\"{0}\" equipped for battle.", w.LocalizedTitle));
                Refresh();
            }
            else
            {
                var s = ShipSkinCatalog.All[selectedSkinIndex];
                if (!save.IsSkinAccessible(s.Id))
                {
                    ShowHint(Localization.TFormat("Корпус откроется с миссии {0}.", "Hull unlocks at Mission {0}.", s.RequiredMissionIndex + 1));
                    return;
                }

                if (!save.Data.unlockedSkinIds.Contains(s.Id))
                {
                    if (save.UnlockSkin(s.Id))
                    {
                        save.EquipSkin(s.Id);
                        AudioManager.PlayPowerup();
                        ShowHint(Localization.TFormat("{0} куплен и надет. Осталось {1} монет.", "\"{0}\" purchased and equipped. Remaining: {1} coins.", s.LocalizedTitle, save.Data.coins));
                        Refresh();
                    }
                    else
                    {
                        ShowHint(Localization.TFormat("Не хватает {0} монет.", "Need {0} more coins.", s.Price - save.Data.coins));
                    }
                    return;
                }

                save.EquipSkin(s.Id);
                AudioManager.PlayClick();
                ShowHint(Localization.TFormat("{0} теперь на тарелке.", "\"{0}\" equipped on saucer.", s.LocalizedTitle));
                Refresh();
            }
        }

        private GameObject CreateListItem(Transform parent, string title, string sub, int price, bool isOwned, bool isEquipped, bool isSelected, System.Action onClick, bool isLocked, string lockBadge)
        {
            var btnObj = new GameObject("Item_" + title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnObj.transform.SetParent(parent, false);

            var le = btnObj.GetComponent<LayoutElement>();
            le.minHeight = 64f;
            le.preferredHeight = 64f;
            le.flexibleHeight = 0f;

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.SubPanel;
            img.type = Image.Type.Sliced;
            if (isSelected)
                img.color = new Color(0.2f, 0.45f, 0.75f, 1f);
            else if (isEquipped)
                img.color = new Color(0.12f, 0.28f, 0.38f, 0.95f);
            else
                img.color = new Color(0.08f, 0.12f, 0.2f, 0.95f);

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                onClick?.Invoke();
            });

            // Accent bar on left edge
            var accentObj = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentObj.transform.SetParent(btnObj.transform, false);
            var accRect = accentObj.GetComponent<RectTransform>();
            accRect.anchorMin = new Vector2(0f, 0f);
            accRect.anchorMax = new Vector2(0f, 1f);
            accRect.pivot = new Vector2(0f, 0.5f);
            accRect.anchoredPosition = Vector2.zero;
            accRect.sizeDelta = new Vector2(4f, 0f);
            var accImg = accentObj.GetComponent<Image>();
            accImg.sprite = PixelUiAtlas.White;
            accImg.color = isSelected ? new Color(1f, 0.85f, 0.3f) : (isEquipped ? new Color(0.3f, 0.9f, 0.6f) : new Color(0.3f, 0.45f, 0.65f, 0.5f));

            // Left Labels: Title & Sub
            var titleText = CreateText(btnObj.transform, "T", title, 16, Vector2.zero, new Vector2(300f, 26f), isSelected ? Color.white : (isOwned ? new Color(0.95f, 0.98f, 1f) : new Color(0.8f, 0.85f, 0.9f)), FontStyle.Bold);
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
                statusStr = Localization.T("★ В БОЮ", "★ EQUIPPED");
                statusCol = new Color(0.4f, 1f, 0.5f);
            }
            else if (isOwned)
            {
                statusStr = Localization.T("В АНГАРЕ", "OWNED");
                statusCol = new Color(0.85f, 0.95f, 1f);
            }
            else if (isLocked)
            {
                statusStr = lockBadge ?? Localization.T("🔒 БЛОК", "🔒 LOCKED");
                statusCol = new Color(0.95f, 0.45f, 0.45f);
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

        private static Button CreateTabButton(Transform parent, string name, string label, Vector2 pos, System.Action onClick, out Text labelRef)
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

            labelRef = CreateText(btnObj.transform, "T", label, 18, Vector2.zero, new Vector2(360f, 36f), Color.white, FontStyle.Bold);
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
            label.font = GameFont.Get();
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
            return Localization.TFormat("БАЛАНС: {0} 💰    {1}", "BALANCE: {0} 💰    {1}", coins, NextDeal(save, coins));
        }

        private static string NextDeal(SaveManager save, int coins)
        {
            if (save == null)
                return Localization.T("Сначала зайди в игру, чтобы кошелёк сохранился.", "Launch flight to initialize your wallet.");

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
                return Localization.T("Доступные корпуса уже в ангаре.", "All available hulls are already in your hangar.");

            if (coins >= cheapest.Price)
                return Localization.TFormat("Можно купить «{0}» за {1}.", "You can purchase \"{0}\" for {1} 💰.", cheapest.LocalizedTitle, cheapest.Price);

            return Localization.TFormat("До «{0}» ещё {1} монет.", "Need {1} more coins for \"{0}\".", cheapest.LocalizedTitle, cheapest.Price - coins);
        }
    }
}
