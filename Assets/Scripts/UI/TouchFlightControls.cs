// Assets/Scripts/UI/TouchFlightControls.cs
// Namespace: KANYE.UI
// Mobile Touch Controls HUD: Virtual Joystick (left thumb) and Action Buttons (right thumb):
// - Primary Fire (large button)
// - EX Special Rocket [Q] (left of Fire)
// - Mega-Laser Super Art [E] (above Fire)
// Auto-hides on PC without touch, auto-shows on mobile and touch devices.

using System;
using KANYE.Gameplay;
using KANYE.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class TouchFlightControls : MonoBehaviour
    {
        public static Vector2 TouchMove { get; private set; }
        public static bool TouchFireHeld { get; private set; }

        private static bool touchExRequested;
        private static bool touchSuperRequested;

        public static bool ConsumeTouchEx()
        {
            var val = touchExRequested;
            touchExRequested = false;
            return val;
        }

        public static bool ConsumeTouchSuper()
        {
            var val = touchSuperRequested;
            touchSuperRequested = false;
            return val;
        }

        public static bool IsMobileDevice
        {
            get
            {
                return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
            }
        }

        private CanvasGroup canvasGroup;
        private RectTransform stickKnobRect;
        private float stickRadius = 85f;

        private Text fireLabel;
        private Text exLabel;
        private Text superLabel;

        private Image exImage;
        private Image superImage;

        private void Awake()
        {
            EnsureEventSystem();
            BuildControls();
            Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
            TouchMove = Vector2.zero;
            TouchFireHeld = false;
            touchExRequested = false;
            touchSuperRequested = false;
        }

        private void OnLanguageChanged()
        {
            if (fireLabel != null)
                fireLabel.text = Localization.T("ОГОНЬ", "FIRE");
            if (exLabel != null)
                exLabel.text = Localization.T("🚀 EX\n[Q]", "🚀 EX\n[Q]");
            if (superLabel != null)
                superLabel.text = Localization.T("⚡ МЕГА\n[E]", "⚡ MEGA\n[E]");
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            var module = eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private void BuildControls()
        {
            var canvasObj = new GameObject("TouchFlightCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 1.25f;

            canvasGroup = canvasObj.GetComponent<CanvasGroup>();

            // 1. LEFT SIDE: Virtual Joystick Base & Knob
            var stickBase = CreateImage("StickBase", canvasObj.transform, PixelUiAtlas.SubPanel, new Color(0.1f, 0.2f, 0.35f, 0.55f));
            stickBase.type = Image.Type.Sliced;
            var baseRect = stickBase.rectTransform;
            baseRect.anchorMin = new Vector2(0f, 0f);
            baseRect.anchorMax = new Vector2(0f, 0f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.anchoredPosition = new Vector2(210f, 210f);
            baseRect.sizeDelta = new Vector2(240f, 240f);

            var stickKnob = CreateImage("StickKnob", stickBase.transform, PixelUiAtlas.ButtonPrimary, new Color(0.2f, 0.85f, 1f, 0.85f));
            stickKnob.type = Image.Type.Sliced;
            stickKnobRect = stickKnob.rectTransform;
            stickKnobRect.anchorMin = new Vector2(0.5f, 0.5f);
            stickKnobRect.anchorMax = new Vector2(0.5f, 0.5f);
            stickKnobRect.pivot = new Vector2(0.5f, 0.5f);
            stickKnobRect.anchoredPosition = Vector2.zero;
            stickKnobRect.sizeDelta = new Vector2(100f, 100f);

            var stickHandler = stickBase.gameObject.AddComponent<TouchStickHandler>();
            stickHandler.Init(this);

            // 2. RIGHT SIDE: Primary FIRE Button (clean, prominent arcade fire button)
            var fireBtnObj = CreateButton("TouchFireButton", canvasObj.transform, Localization.T("ОГОНЬ", "FIRE"), 24, new Vector2(1f, 0f), new Vector2(-155f, 145f), new Vector2(150f, 150f), PixelUiAtlas.ButtonPrimary, new Color(1f, 0.45f, 0.2f, 0.88f));
            fireLabel = fireBtnObj.GetComponentInChildren<Text>();
            var fireHandler = fireBtnObj.AddComponent<TouchFireHandler>();
            fireHandler.OnStateChanged += (held) => TouchFireHeld = held;

            // 3. EX-Rocket [Q] Button (placed left of Fire)
            var exBtnObj = CreateButton("TouchExButton", canvasObj.transform, Localization.T("🚀 EX\n[Q]", "🚀 EX\n[Q]"), 15, new Vector2(1f, 0f), new Vector2(-295f, 130f), new Vector2(105f, 105f), PixelUiAtlas.ButtonSecondary, new Color(0.18f, 0.4f, 0.65f, 0.65f));
            exLabel = exBtnObj.GetComponentInChildren<Text>();
            exImage = exBtnObj.GetComponent<Image>();
            var exHandler = exBtnObj.AddComponent<TouchSpecialHandler>();
            exHandler.OnPressed += OnExTapped;
            var exBtn = exBtnObj.GetComponent<Button>();
            exBtn.onClick.AddListener(OnExTapped);

            // 4. Mega-Laser [E] Button (placed above Fire)
            var superBtnObj = CreateButton("TouchSuperButton", canvasObj.transform, Localization.T("⚡ МЕГА\n[E]", "⚡ MEGA\n[E]"), 15, new Vector2(1f, 0f), new Vector2(-155f, 285f), new Vector2(105f, 105f), PixelUiAtlas.ButtonPrimary, new Color(0.5f, 0.15f, 0.4f, 0.65f));
            superLabel = superBtnObj.GetComponentInChildren<Text>();
            superImage = superBtnObj.GetComponent<Image>();
            var superHandler = superBtnObj.AddComponent<TouchSpecialHandler>();
            superHandler.OnPressed += OnSuperTapped;
            var superBtn = superBtnObj.GetComponent<Button>();
            superBtn.onClick.AddListener(OnSuperTapped);

            // Set initial alpha based on platform
            canvasGroup.alpha = IsMobileDevice ? 1f : 0f;
            canvasGroup.interactable = IsMobileDevice;
            canvasGroup.blocksRaycasts = IsMobileDevice;
        }

        private void OnExTapped()
        {
            touchExRequested = true;
            var shooter = FindAnyObjectByType<ShipShooter>();
            if (shooter != null)
                shooter.TryFireEX();
        }

        private void OnSuperTapped()
        {
            touchSuperRequested = true;
            var shooter = FindAnyObjectByType<ShipShooter>();
            if (shooter != null)
                shooter.TryFireSuper();
        }

        private void Update()
        {
            if (canvasGroup == null)
                return;

            var hasTouch = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var touches = UnityEngine.InputSystem.Touchscreen.current.touches;
                for (var i = 0; i < touches.Count; i++)
                {
                    if (touches[i].isInProgress)
                    {
                        hasTouch = true;
                        break;
                    }
                }
            }
#else
            hasTouch = Input.touchCount > 0;
#endif
            var show = IsMobileDevice || hasTouch;
            var targetAlpha = show ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 4f);
            canvasGroup.interactable = canvasGroup.alpha > 0.1f;
            canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.1f;

            if (show)
            {
                UpdateSuperButtonsVisual();
            }
        }

        private void UpdateSuperButtonsVisual()
        {
            var hype = HypeState.Instance;
            if (hype == null) return;

            var canEx = hype.CanUseEX;
            if (exImage != null)
            {
                var targetCol = canEx ? new Color(0.15f, 0.85f, 1f, 0.95f) : new Color(0.12f, 0.32f, 0.48f, 0.45f);
                exImage.color = Color.Lerp(exImage.color, targetCol, Time.unscaledDeltaTime * 8f);
            }

            var canSuper = hype.CanUseSuper;
            if (superImage != null)
            {
                var pulse = canSuper ? 0.82f + 0.18f * Mathf.Sin(Time.unscaledTime * 7f) : 1f;
                var targetCol = canSuper ? new Color(1f * pulse, 0.2f * pulse, 0.85f, 0.98f) : new Color(0.42f, 0.14f, 0.32f, 0.45f);
                superImage.color = Color.Lerp(superImage.color, targetCol, Time.unscaledDeltaTime * 8f);
            }
        }

        public void OnStickDrag(Vector2 localPoint)
        {
            var clamped = Vector2.ClampMagnitude(localPoint, stickRadius);
            if (stickKnobRect != null)
                stickKnobRect.anchoredPosition = clamped;
            TouchMove = clamped / stickRadius;
        }

        public void OnStickRelease()
        {
            if (stickKnobRect != null)
                stickKnobRect.anchoredPosition = Vector2.zero;
            TouchMove = Vector2.zero;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, int fontSize, Vector2 anchor, Vector2 pos, Vector2 size, Sprite sprite, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = true;

            var btn = obj.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;

            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObj.transform.SetParent(obj.transform, false);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObj.GetComponent<Text>();
            label.font = GameFont.Get();
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
            label.lineSpacing = 0.95f;
            label.raycastTarget = false;
            label.text = text;

            return obj;
        }
    }

    internal class TouchStickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private TouchFlightControls controls;
        private RectTransform rectTransform;

        internal void Init(TouchFlightControls ctrl)
        {
            controls = ctrl;
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controls == null || rectTransform == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
            controls.OnStickDrag(localPoint);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            controls?.OnStickRelease();
        }
    }

    internal class TouchFireHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<bool> OnStateChanged;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnStateChanged?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnStateChanged?.Invoke(false);
        }
    }

    internal class TouchSpecialHandler : MonoBehaviour, IPointerDownHandler
    {
        public event Action OnPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPressed?.Invoke();
        }
    }
}
