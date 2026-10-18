using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class TouchFlightControls : MonoBehaviour
    {
        [SerializeField] private string moveControlPath = "<Gamepad>/leftStick";
        [SerializeField] private string fireControlPath = "<Gamepad>/buttonSouth";
        [SerializeField] private float stickRange = 80f;

        private void Awake()
        {
            EnsureEventSystem();
            BuildControls();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            var module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private void BuildControls()
        {
            var canvasObject = new GameObject("FlightCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var stick = CreateGraphic("MoveStick", canvasObject.transform, new Vector2(0f, 0f), new Vector2(180f, 180f), new Vector2(220f, 220f), new Color(1f, 1f, 1f, 0.28f));
            var onScreenStick = stick.gameObject.AddComponent<OnScreenStick>();
            onScreenStick.controlPath = moveControlPath;
            onScreenStick.movementRange = stickRange;
            onScreenStick.useIsolatedInputActions = true;

            var fire = CreateGraphic("FireButton", canvasObject.transform, new Vector2(1f, 0f), new Vector2(-180f, 180f), new Vector2(160f, 160f), new Color(1f, 0.45f, 0.35f, 0.45f));
            var onScreenButton = fire.gameObject.AddComponent<OnScreenButton>();
            onScreenButton.controlPath = fireControlPath;
        }

        private static RectTransform CreateGraphic(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var graphicObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            graphicObject.transform.SetParent(parent, false);

            var rect = graphicObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = graphicObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return rect;
        }
    }
}
