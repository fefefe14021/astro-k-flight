// Assets/Scripts/UI/DefeatModal.cs
// Namespace: KANYE.UI
// Responsive pixel-styled Game Over / Revive Modal.

using System;
using KANYE.Audio;
using KANYE.Services;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class DefeatModal : MonoBehaviour
    {
        public static DefeatModal Instance { get; private set; }

        private GameObject modalRoot;
        private Action onReviveCallback;
        private Button reviveButton;
        private Text statusText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance == null)
            {
                var go = new GameObject("DefeatModal");
                Instance = go.AddComponent<DefeatModal>();
            }
        }

        private void Awake()
        {
            Instance = this;
            BuildUI();
            Hide();
        }

        public static void Show(bool canRevive, Action onRevive)
        {
            if (Instance == null)
                AutoCreate();

            Instance?.DoShow(canRevive, onRevive);
        }

        public static void Hide()
        {
            Instance?.DoHide();
        }

        private void DoShow(bool canRevive, Action onRevive)
        {
            onReviveCallback = onRevive;
            if (modalRoot != null)
                modalRoot.SetActive(true);

            if (reviveButton != null)
                reviveButton.gameObject.SetActive(canRevive);
        }

        private void DoHide()
        {
            if (modalRoot != null)
                modalRoot.SetActive(false);
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("DefeatCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 350;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            modalRoot = new GameObject("ModalRoot", typeof(RectTransform));
            modalRoot.transform.SetParent(canvasObj.transform, false);
            Stretch(modalRoot.GetComponent<RectTransform>());

            // Dim backdrop
            var dim = CreateImage("Dim", modalRoot.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.08f, 0.75f));
            Stretch(dim.rectTransform);

            // Center Panel
            var panel = CreateImage("Panel", modalRoot.transform, PixelUiAtlas.Panel, Color.white);
            panel.type = Image.Type.Sliced;
            panel.rectTransform.sizeDelta = new Vector2(620f, 440f);

            // Title
            var title = CreateLabel(panel.transform, "Title", "КОРАБЛЬ РАЗБИТ", 34, new Vector2(0f, 140f), new Color(1f, 0.35f, 0.35f, 1f));
            title.fontStyle = FontStyle.Bold;

            statusText = CreateLabel(panel.transform, "Subtitle", "Сектор не пройден. Связь с ядром потеряна.", 20, new Vector2(0f, 85f), new Color(0.8f, 0.88f, 0.95f, 1f));

            // Buttons
            reviveButton = CreateButton(panel.transform, "ReviveButton", "⚡ ВОЗРОДИТЬСЯ (ВТОРОЙ ШАНС)", new Vector2(0f, 10f), new Vector2(460f, 58f), PixelUiAtlas.ButtonPrimary);
            reviveButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                onReviveCallback?.Invoke();
            });

            var retryButton = CreateButton(panel.transform, "RetryButton", "ПОВТОРИТЬ МИССИЮ", new Vector2(0f, -65f), new Vector2(460f, 54f), PixelUiAtlas.ButtonSecondary);
            retryButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                Time.timeScale = 1f;
                SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
            });

            var menuButton = CreateButton(panel.transform, "MenuButton", "В ГЛАВНОЕ МЕНЮ", new Vector2(0f, -135f), new Vector2(460f, 54f), PixelUiAtlas.ButtonDanger);
            menuButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                Time.timeScale = 1f;
                SceneLoader.ReturnToMainMenu();
            });
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            var img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static Text CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 pos, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(580f, 40f);

            var label = obj.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Sprite bgSprite)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = obj.GetComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var label = CreateLabel(obj.transform, "Label", text, 20, Vector2.zero, Color.white);
            label.fontStyle = FontStyle.Bold;
            return obj.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
