// Assets/Scripts/UI/ShopScreen.cs
// Namespace: KANYE.UI
// Complete Armory & Hangar Shop: Flying Saucer Hulls and Diverse Weapon Arsenal.

using System.Collections.Generic;
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

        private Text coinsLabel;
        private Text tabTitleLabel;
        private Transform listContainer;
        private List<GameObject> activeListButtons = new List<GameObject>();

        // Detail View UI Elements
        private Image previewImage;
        private Text previewTitle;
        private Text previewSubtitle;
        private Text previewDescription;
        private Text previewStats;
        private Button actionButton;
        private Text actionButtonText;
        private Button tabWeaponsBtn;
        private Button tabSkinsBtn;

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
            mainRect.sizeDelta = new Vector2(1100f, 740f);
            var panelImg = mainPanel.GetComponent<Image>();
            panelImg.sprite = PixelUiAtlas.Panel;
            panelImg.type = Image.Type.Sliced;

            // Header Title
            CreateText(mainPanel.transform, "HeaderTitle", "АНГАР И АРСЕНАЛ КАПИТАНА ЗИГГИ", 30, new Vector2(0f, 325f), new Color(1f, 0.85f, 0.35f));
            coinsLabel = CreateText(mainPanel.transform, "CoinsLabel", CoinsText(), 22, new Vector2(0f, 285f), new Color(1f, 0.95f, 0.7f));

            // Tab Buttons
            tabWeaponsBtn = CreateTabButton(mainPanel.transform, "TabWeapons", "🚀 ОРУЖЕЙНЫЙ АРСЕНАЛ", new Vector2(-220f, 235f), () => SetTab(ShopTab.Weapons));
            tabSkinsBtn = CreateTabButton(mainPanel.transform, "TabSkins", "🛸 КОРПУСА ТАРЕЛКИ", new Vector2(220f, 235f), () => SetTab(ShopTab.Skins));

            // Left Side: List Container
            var listPanel = new GameObject("ListPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            listPanel.transform.SetParent(mainPanel.transform, false);
            var listRect = listPanel.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0.5f);
            listRect.anchorMax = new Vector2(0f, 0.5f);
            listRect.pivot = new Vector2(0f, 0.5f);
            listRect.anchoredPosition = new Vector2(40f, -25f);
            listRect.sizeDelta = new Vector2(480f, 440f);
            var listBg = listPanel.GetComponent<Image>();
            listBg.sprite = PixelUiAtlas.SubPanel;
            listBg.type = Image.Type.Sliced;
            listBg.color = new Color(0.08f, 0.12f, 0.2f, 0.85f);
            listContainer = listPanel.transform;

            // Right Side: Card Preview & Specs
            var previewPanel = new GameObject("PreviewPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewPanel.transform.SetParent(mainPanel.transform, false);
            var prevRect = previewPanel.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(1f, 0.5f);
            prevRect.anchorMax = new Vector2(1f, 0.5f);
            prevRect.pivot = new Vector2(1f, 0.5f);
            prevRect.anchoredPosition = new Vector2(-40f, -25f);
            prevRect.sizeDelta = new Vector2(500f, 440f);
            var prevBg = previewPanel.GetComponent<Image>();
            prevBg.sprite = PixelUiAtlas.SubPanel;
            prevBg.type = Image.Type.Sliced;

            // Preview Frame for Sprite
            var frameObj = new GameObject("SpriteFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameObj.transform.SetParent(previewPanel.transform, false);
            var frameRect = frameObj.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 1f);
            frameRect.anchorMax = new Vector2(0.5f, 1f);
            frameRect.pivot = new Vector2(0.5f, 1f);
            frameRect.anchoredPosition = new Vector2(0f, -20f);
            frameRect.sizeDelta = new Vector2(260f, 140f);
            var frameImg = frameObj.GetComponent<Image>();
            frameImg.sprite = PixelUiAtlas.SubPanel;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = new Color(0.04f, 0.06f, 0.12f, 0.95f);

            var spriteObj = new GameObject("SpritePreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spriteObj.transform.SetParent(frameObj.transform, false);
            var sprRect = spriteObj.GetComponent<RectTransform>();
            sprRect.anchorMin = new Vector2(0.5f, 0.5f);
            sprRect.anchorMax = new Vector2(0.5f, 0.5f);
            sprRect.pivot = new Vector2(0.5f, 0.5f);
            sprRect.sizeDelta = new Vector2(160f, 110f);
            previewImage = spriteObj.GetComponent<Image>();
            previewImage.preserveAspect = true;

            previewTitle = CreateText(previewPanel.transform, "PrevTitle", "ОРУЖИЕ", 22, new Vector2(0f, 25f), new Color(1f, 0.85f, 0.3f));
            previewSubtitle = CreateText(previewPanel.transform, "PrevSub", "Категория", 15, new Vector2(0f, 0f), new Color(0.4f, 0.85f, 1f));
            previewStats = CreateText(previewPanel.transform, "PrevStats", "Урон: 1 | Скорость: 14", 16, new Vector2(0f, -32f), new Color(0.9f, 1f, 0.8f));
            previewDescription = CreateText(previewPanel.transform, "PrevDesc", "Описание технологии...", 15, new Vector2(0f, -85f), new Color(0.8f, 0.88f, 0.95f));
            previewDescription.rectTransform.sizeDelta = new Vector2(440f, 80f);

            // Action Button (Buy / Equip)
            var btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(previewPanel.transform, false);
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 24f);
            btnRect.sizeDelta = new Vector2(340f, 48f);
            var btnImg = btnObj.GetComponent<Image>();
            btnImg.sprite = PixelUiAtlas.ButtonPrimary;
            btnImg.type = Image.Type.Sliced;
            actionButton = btnObj.GetComponent<Button>();
            actionButton.onClick.AddListener(OnActionButtonClicked);
            actionButtonText = CreateText(btnObj.transform, "BtnText", "ВЫБРАТЬ", 19, Vector2.zero, Color.white);

            // Return to Main Menu
            var backBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            backBtn.transform.SetParent(mainPanel.transform, false);
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 18f);
            backRect.sizeDelta = new Vector2(280f, 42f);
            var backImg = backBtn.GetComponent<Image>();
            backImg.sprite = PixelUiAtlas.ButtonSecondary;
            backImg.type = Image.Type.Sliced;
            backBtn.GetComponent<Button>().onClick.AddListener(SceneLoader.ReturnToMainMenu);
            CreateText(backBtn.transform, "BackText", "В ГЛАВНОЕ МЕНЮ", 17, Vector2.zero, Color.white);
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

                    var btn = CreateListItem(listContainer, w.Title, w.Subtitle, w.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedWeaponIndex = index;
                        Refresh();
                    });
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

                    var btn = CreateListItem(listContainer, s.Title, s.Subtitle, s.Price, isOwned, isEquipped, isSelected, () =>
                    {
                        selectedSkinIndex = index;
                        Refresh();
                    });
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

            if (previewImage != null)
            {
                var sprite = Resources.Load<Sprite>(weapon.ProjectileSprite);
                previewImage.sprite = sprite;
                previewImage.color = Color.white;
            }

            if (previewTitle != null) previewTitle.text = weapon.Title;
            if (previewSubtitle != null) previewSubtitle.text = weapon.Subtitle;
            if (previewDescription != null) previewDescription.text = weapon.Description;

            if (previewStats != null)
            {
                var rps = 1f / weapon.FireInterval;
                previewStats.text = $"УРОН: {weapon.Damage}  |  ТЕМП: {rps:0.0} выстр/сек";
            }

            if (isEquipped)
            {
                actionButtonText.text = "УЖЕ В БОЮ";
                actionButton.interactable = false;
                actionButton.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
            }
            else if (isOwned)
            {
                actionButtonText.text = "ЭКИПИРОВАТЬ";
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
            }
            else
            {
                var canAfford = save != null && save.Data.coins >= weapon.Price;
                actionButtonText.text = $"КУПИТЬ ЗА {weapon.Price} 💰";
                actionButton.interactable = canAfford;
                actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.5f, 0.25f, 0.25f);
            }
        }

        private void DisplaySkinDetails(ShipSkin skin)
        {
            if (skin == null) return;

            var save = SaveManager.Instance;
            var isOwned = save != null && save.Data.unlockedSkinIds.Contains(skin.Id);
            var isEquipped = save != null && save.Data.equippedSkinId == skin.Id;

            if (previewImage != null)
            {
                var sprite = Resources.Load<Sprite>(skin.SpriteName);
                previewImage.sprite = sprite;
                previewImage.color = Color.white;
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
            }
            else if (isOwned)
            {
                actionButtonText.text = "ЭКИПИРОВАТЬ";
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.35f);
            }
            else
            {
                var canAfford = save != null && save.Data.coins >= skin.Price;
                actionButtonText.text = $"КУПИТЬ ЗА {skin.Price} 💰";
                actionButton.interactable = canAfford;
                actionButton.GetComponent<Image>().color = canAfford ? new Color(1f, 0.6f, 0.15f) : new Color(0.5f, 0.25f, 0.25f);
            }
        }

        private void OnActionButtonClicked()
        {
            var save = SaveManager.Instance;
            if (save == null) return;

            if (currentTab == ShopTab.Weapons)
            {
                var w = WeaponCatalog.All[selectedWeaponIndex];
                var isOwned = save.Data.unlockedWeaponIds.Contains(w.Id);
                if (!isOwned)
                {
                    if (save.TrySpendCoins(w.Price))
                    {
                        save.UnlockWeapon(w.Id);
                        save.EquipWeapon(w.Id);
                    }
                }
                else
                {
                    save.EquipWeapon(w.Id);
                }
            }
            else
            {
                var s = ShipSkinCatalog.All[selectedSkinIndex];
                var isOwned = save.Data.unlockedSkinIds.Contains(s.Id);
                if (!isOwned)
                {
                    if (save.TrySpendCoins(s.Price))
                    {
                        save.UnlockSkin(s.Id);
                        save.EquipSkin(s.Id);
                    }
                }
                else
                {
                    save.EquipSkin(s.Id);
                }
            }

            Refresh();
        }

        private GameObject CreateListItem(Transform parent, string title, string sub, int price, bool isOwned, bool isEquipped, bool isSelected, System.Action onClick)
        {
            var btnObj = new GameObject("ItemBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(-20f, 62f);
            rect.anchoredPosition = new Vector2(0f, -10f - activeListButtons.Count * 70f);

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonSecondary;
            img.type = Image.Type.Sliced;
            img.color = isSelected 
                ? new Color(0.3f, 0.65f, 0.95f, 1f) 
                : new Color(0.12f, 0.18f, 0.28f, 0.9f);

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Title
            var titleText = CreateText(btnObj.transform, "T", title, 16, new Vector2(-70f, 12f), Color.white);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            titleText.rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
            titleText.alignment = TextAnchor.MiddleLeft;

            // Subtitle
            var subText = CreateText(btnObj.transform, "S", sub, 12, new Vector2(-70f, -14f), new Color(0.6f, 0.8f, 1f));
            subText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            subText.rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
            subText.alignment = TextAnchor.MiddleLeft;

            // Status Badge
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
                statusCol = new Color(0.85f, 0.9f, 1f);
            }
            else
            {
                statusStr = $"{price} 💰";
                statusCol = new Color(1f, 0.85f, 0.3f);
            }

            var badge = CreateText(btnObj.transform, "B", statusStr, 14, new Vector2(10f, 0f), statusCol);
            badge.rectTransform.anchorMin = new Vector2(0.7f, 0.5f);
            badge.rectTransform.anchorMax = new Vector2(0.98f, 0.5f);
            badge.alignment = TextAnchor.MiddleRight;

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

            CreateText(btnObj.transform, "T", label, 18, Vector2.zero, Color.white);
            return btn;
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 pos, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(460f, 40f);

            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            return label;
        }

        private static string CoinsText()
        {
            var c = SaveManager.Instance != null ? SaveManager.Instance.Data.coins : 0;
            return $"БАЛАНС: {c} 💰 МОНЕТ";
        }
    }
}
