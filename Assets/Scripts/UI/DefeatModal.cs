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
        private Text titleText;
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

            if (Systems.GameRules.CurrentMode == Systems.GameMode.RogueExpedition)
            {
                if (titleText != null)
                    titleText.text = Localization.T("ЭКСПЕДИЦИЯ ЗАВЕРШЕНА", "EXPEDITION FINISHED");
                if (statusText != null)
                    statusText.text = Localization.TFormat("Пройдено волн: {0}\n🪙 Собрано ресурсов: +{1} / 150 (сохранено в ангар)", "Waves cleared: {0}\n🪙 Resources collected: +{1} / 150 (banked to hangar)", Gameplay.RogueExpeditionController.CurrentWave, Gameplay.RogueExpeditionController.CoinsEarnedThisRun);
            }
            else
            {
                if (titleText != null)
                    titleText.text = Localization.T("КОРАБЛЬ РАЗБИТ", "SHIP DESTROYED");
                if (statusText != null)
                    statusText.text = Localization.T("Сектор не пройден. Связь с ядром потеряна.", "Sector failed. Connection to core lost.");
            }
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
            scaler.dynamicPixelsPerUnit = 1.25f;

            modalRoot = new GameObject("ModalRoot", typeof(RectTransform));
            modalRoot.transform.SetParent(canvasObj.transform, false);
            Stretch(modalRoot.GetComponent<RectTransform>());

            // Dim backdrop
            var dim = CreateImage("Dim", modalRoot.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.08f, 0.75f));
            Stretch(dim.rectTransform);

            // Center Panel (Comfortable proportions and margins)
            var panel = CreateImage("Panel", modalRoot.transform, PixelUiAtlas.Panel, Color.white);
            panel.type = Image.Type.Sliced;
            panel.rectTransform.sizeDelta = new Vector2(640f, 460f);

            // Title
            titleText = CreateLabel(panel.transform, "Title", Localization.T("КОРАБЛЬ РАЗБИТ", "SHIP DESTROYED"), 32, new Vector2(0f, 152f), new Color(1f, 0.35f, 0.35f, 1f));
            titleText.fontStyle = FontStyle.Bold;

            statusText = CreateLabel(panel.transform, "Subtitle", Localization.T("Сектор не пройден. Связь с ядром потеряна.", "Sector failed. Connection to core lost."), 18, new Vector2(0f, 92f), new Color(0.8f, 0.88f, 0.95f, 1f));
            statusText.rectTransform.sizeDelta = new Vector2(580f, 52f);

            // Buttons with distinct 18px separation
            reviveButton = CreateButton(panel.transform, "ReviveButton", Localization.T("⚡ ВОЗРОДИТЬСЯ ЗА РЕКЛАМУ", "⚡ REVIVE WITH AD"), new Vector2(0f, 14f), new Vector2(460f, 54f), PixelUiAtlas.ButtonPrimary);
            reviveButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                onReviveCallback?.Invoke();
            });

            var retryButton = CreateButton(panel.transform, "RetryButton", Localization.T("ПОВТОРИТЬ МИССИЮ", "RETRY MISSION"), new Vector2(0f, -56f), new Vector2(460f, 52f), PixelUiAtlas.ButtonSecondary);
            retryButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                Time.timeScale = 1f;
                SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
            });

            var menuButton = CreateButton(panel.transform, "MenuButton", Localization.T("В ГЛАВНОЕ МЕНЮ", "MAIN MENU"), new Vector2(0f, -126f), new Vector2(460f, 52f), PixelUiAtlas.ButtonDanger);
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
            label.font = GameFont.Get();
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
