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

            CreateLabel(canvas.transform, "Магазин корпусов", new Vector2(0f, 250f), 40);
            coinsLabel = CreateLabel(canvas.transform, CoinsText(), new Vector2(0f, 190f), 24);

            for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
            {
                var skin = ShipSkinCatalog.All[i];
                var button = CreateButton(canvas.transform, skin.Id, new Vector2(0f, 80f - i * 90f));
                buttonLabels[i] = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => BuyOrEquip(skin));
            }

            var back = CreateButton(canvas.transform, "back", new Vector2(0f, -220f));
            back.GetComponentInChildren<Text>().text = "В меню";
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
                    buttonLabels[i].text = skin.Title + " — надет";
                else if (owned || skin.Price == 0)
                    buttonLabels[i].text = skin.Title + " — надеть";
                else
                    buttonLabels[i].text = skin.Title + " — купить за " + skin.Price;
            }
        }

        private static string CoinsText()
        {
            var coins = SaveManager.Instance != null ? SaveManager.Instance.Data.coins : 0;
            return "Монеты: " + coins;
        }

        private static Text CreateLabel(Transform parent, string value, Vector2 position, int fontSize)
        {
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(800f, 60f);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
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
            rect.sizeDelta = new Vector2(460f, 70f);
            buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.92f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return buttonObject.GetComponent<Button>();
        }
    }
}
