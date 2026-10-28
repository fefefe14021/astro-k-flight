using System.Collections.Generic;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class AchievementsScreen : MonoBehaviour
    {
        private void Start()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var canvasObject = new GameObject("AchievementsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateLabel(canvas.transform, "Достижения", new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 60f), 40, Color.white);

            var viewport = CreatePanel(canvas.transform, "List", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(980f, 760f));
            viewport.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.1f, 0.92f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.viewport = viewport;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewport, false);
            var content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);
            var layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 16, 16);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            var definitions = new List<AchievementData>();
            if (AchievementManager.Instance != null)
                definitions.AddRange(AchievementManager.Instance.Definitions);
            definitions.Sort((a, b) => string.Compare(a.displayName, b.displayName, System.StringComparison.Ordinal));

            if (definitions.Count == 0)
                CreateRow(content, "Пока нет достижений", "Они появятся по ходу полёта.", "—", false);
            else
            {
                for (var i = 0; i < definitions.Count; i++)
                    CreateRow(content, definitions[i]);
            }

            var back = CreateButton(canvas.transform, "Back", "В меню", new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(280f, 64f));
            back.onClick.AddListener(SceneLoader.ReturnToMainMenu);
        }

        private static void CreateRow(RectTransform parent, AchievementData data)
        {
            var manager = AchievementManager.Instance;
            var unlocked = manager != null && manager.IsUnlocked(data.id);
            var progress = manager != null ? manager.GetProgress(data.id) : 0;
            var target = Mathf.Max(1, data.targetAmount);
            var status = unlocked ? "получено" : progress + "/" + target;
            CreateRow(parent, data.displayName, data.description, status, unlocked);
        }

        private static void CreateRow(RectTransform parent, string title, string description, string status, bool unlocked)
        {
            var row = CreatePanel(parent, title, new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 88f));
            row.GetComponent<Image>().color = unlocked
                ? new Color(0.12f, 0.28f, 0.18f, 0.95f)
                : new Color(0.12f, 0.13f, 0.16f, 0.95f);
            var element = row.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 88f;

            var tint = unlocked ? new Color(0.75f, 1f, 0.8f, 1f) : new Color(0.75f, 0.78f, 0.84f, 1f);
            CreateLabel(row, title, new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(620f, 36f), 24, tint).alignment = TextAnchor.UpperLeft;
            CreateLabel(row, description, new Vector2(0f, 0f), new Vector2(16f, 8f), new Vector2(620f, 36f), 18, Color.white).alignment = TextAnchor.LowerLeft;
            var statusLabel = CreateLabel(row, status, new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(180f, 40f), 22, tint);
            statusLabel.alignment = TextAnchor.MiddleRight;
            statusLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Text CreateLabel(Transform parent, string value, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = value;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.92f);

            var text = CreateLabel(buttonObject.transform, label, new Vector2(0.5f, 0.5f), Vector2.zero, size, 28, Color.white);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }
    }
}
