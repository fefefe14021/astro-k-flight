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
            cardRect.sizeDelta = new Vector2(880f, 760f);
            var cardImg = cardObj.GetComponent<Image>();
            cardImg.sprite = PixelUiAtlas.Panel;
            cardImg.type = Image.Type.Sliced;

            CreateLabel(cardObj.transform, "БОРТОВЫЕ ДОСТИЖЕНИЯ", new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(800f, 44f), 32, new Color(1f, 0.85f, 0.4f));

            var viewport = CreatePanel(cardObj.transform, "List", new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(800f, 540f));
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
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
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 8f;
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

            var back = CreateButton(cardObj.transform, "Back", "В ГЛАВНОЕ МЕНЮ", new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(280f, 54f));
            back.onClick.AddListener(SceneLoader.ReturnToMainMenu);
        }

        private static void CreateRow(RectTransform parent, AchievementData data)
        {
            var manager = AchievementManager.Instance;
            var unlocked = manager != null && manager.IsUnlocked(data.id);
            var progress = manager != null ? manager.GetProgress(data.id) : 0;
            var target = Mathf.Max(1, data.targetAmount);
            var status = unlocked ? "ОТКРЫТО" : progress + " / " + target;
            CreateRow(parent, data.displayName, data.description, status, unlocked);
        }

        private static void CreateRow(RectTransform parent, string title, string description, string status, bool unlocked)
        {
            var row = CreatePanel(parent, title, new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 80f));
            var rowImg = row.GetComponent<Image>();
            rowImg.sprite = PixelUiAtlas.SubPanel;
            rowImg.type = Image.Type.Sliced;
            rowImg.color = Color.white;

            var element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 88f;
            element.preferredHeight = 88f;

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accent.transform.SetParent(row, false);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(4f, 0f);
            var accentImg = accent.GetComponent<Image>();
            accentImg.sprite = PixelUiAtlas.White;
            accentImg.color = unlocked ? new Color(0.3f, 1f, 0.6f, 1f) : new Color(0.25f, 0.4f, 0.6f, 0.6f);

            var badge = unlocked ? "★ " : "○ ";
            var header = CreateLabel(row, badge + title + "  —  [" + status + "]", new Vector2(0f, 1f), new Vector2(24f, -22f), new Vector2(720f, 28f), 19, unlocked ? new Color(0.7f, 1f, 0.75f) : Color.white);
            header.alignment = TextAnchor.MiddleLeft;

            var desc = CreateLabel(row, description, new Vector2(0f, 1f), new Vector2(24f, -54f), new Vector2(720f, 26f), 15, new Color(0.75f, 0.85f, 0.95f));
            desc.alignment = TextAnchor.MiddleLeft;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            var rect = panelObject.GetComponent<RectTransform>();
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
            rect.pivot = new Vector2(anchor.x == 0f ? 0f : (anchor.x == 1f ? 1f : 0.5f), 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = UIFont.DefaultFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

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
            var label = textObject.GetComponent<Text>();
            label.font = UIFont.DefaultFont;
            label.fontSize = 19;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = text;
            return buttonObject.GetComponent<Button>();
        }
    }
}
