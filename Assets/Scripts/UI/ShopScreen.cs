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
        private Text coinsLabel;
        private readonly Text[] buttonLabels = new Text[ShipSkinCatalog.All.Length];

        private void Start()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var canvasObject = new GameObject("ShopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Background
            var bgObj = new GameObject("Bg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(canvasObject.transform, false);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = PixelUiAtlas.Background;
            bgImg.color = new Color(0.4f, 0.45f, 0.55f, 1f);

            var cardObj = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardObj.transform.SetParent(canvasObject.transform, false);
            var cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 680f);
            var cardImg = cardObj.GetComponent<Image>();
            cardImg.sprite = PixelUiAtlas.Panel;
            cardImg.type = Image.Type.Sliced;

            CreateLabel(cardObj.transform, "МАГАЗИН КОРПУСОВ", new Vector2(0f, 270f), 34, new Color(1f, 0.85f, 0.4f));
            coinsLabel = CreateLabel(cardObj.transform, CoinsText(), new Vector2(0f, 220f), 22, Color.white);

            for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
            {
                var skin = ShipSkinCatalog.All[i];
                var button = CreateButton(cardObj.transform, skin.Id, new Vector2(0f, 120f - i * 85f));
                buttonLabels[i] = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => BuyOrEquip(skin));
            }

            var back = CreateButton(cardObj.transform, "back", new Vector2(0f, -240f));
            back.GetComponentInChildren<Text>().text = "В ГЛАВНОЕ МЕНЮ";
            back.onClick.AddListener(SceneLoader.ReturnToMainMenu);
            Refresh();

            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Changed -= Refresh;
        }

        private void BuyOrEquip(ShipSkin skin)
        {
            var save = SaveManager.Instance;
            if (save == null)
                return;

            var owned = save.Data.unlockedSkinIds.Contains(skin.Id);
            if (!owned && skin.Price > 0 && !save.TrySpendCoins(skin.Price))
                return;

            if (!owned)
                save.UnlockSkin(skin.Id);

            save.EquipSkin(skin.Id);
            Refresh();
        }

        private void Refresh()
        {
            if (coinsLabel != null)
                coinsLabel.text = CoinsText();

            var save = SaveManager.Instance;
            for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
            {
                if (buttonLabels[i] == null)
                    continue;

                var skin = ShipSkinCatalog.All[i];
                var owned = save != null && save.Data.unlockedSkinIds.Contains(skin.Id);
                var equipped = save != null && save.Data.equippedSkinId == skin.Id;
                if (equipped)
                    buttonLabels[i].text = skin.Title + " — НАДЕТ";
                else if (owned || skin.Price == 0)
                    buttonLabels[i].text = skin.Title + " — ВЫБРАТЬ";
                else
                    buttonLabels[i].text = skin.Title + " — КУПИТЬ (" + skin.Price + ")";
            }
        }

        private static string CoinsText()
        {
            var coins = SaveManager.Instance != null ? SaveManager.Instance.Data.coins : 0;
            return "🪙 " + coins + " МОНЕТ";
        }

        private static Text CreateLabel(Transform parent, string value, Vector2 position, int fontSize, Color color)
        {
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(700f, 50f);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = value;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(480f, 62f);

            var img = buttonObject.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonSecondary;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return buttonObject.GetComponent<Button>();
        }
    }
}
