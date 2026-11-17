// Assets/Scripts/UI/VictoryResultsModal.cs
// Namespace: KANYE.UI
// Cuphead-style Victory Grade Card & Results Modal.

using System;
using KANYE.Audio;
using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class VictoryResultsModal : MonoBehaviour
    {
        public static VictoryResultsModal Instance { get; private set; }

        private GameObject modalRoot;
        private Text titleText;
        private Text sectorText;
        private Text timeText;
        private Text hpText;
        private Text parryText;
        private Text superText;
        private Text gradeText;
        private Text rewardText;
        private Button nextButton;
        private Button doubleRewardBtn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance == null)
            {
                var go = new GameObject("VictoryResultsModal");
                Instance = go.AddComponent<VictoryResultsModal>();
            }
        }

        private void Awake()
        {
            Instance = this;
            BuildUI();
            Hide();
        }

        public static void Show(int missionIndex, int hp, int maxHp, int parries, int supersUsed, float clearTime, Action onNext)
        {
            if (Instance == null)
                AutoCreate();

            Instance?.DoShow(missionIndex, hp, maxHp, parries, supersUsed, clearTime, onNext);
        }

        public static void Hide()
        {
            Instance?.modalRoot?.SetActive(false);
        }

        private void DoShow(int missionIndex, int hp, int maxHp, int parries, int supersUsed, float clearTime, Action onNext)
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
                UITween.FadeInModal(this, modalRoot);
            }

            AudioManager.PlayVictory();

            var mission = (missionIndex >= 0 && missionIndex < MissionCatalog.All.Length)
                ? MissionCatalog.All[missionIndex]
                : null;

            if (titleText != null)
                titleText.text = (mission != null && mission.HasBoss) ? "БОСС ПОВЕРЖЕН!" : "МИССИЯ ВЫПОЛНЕНА!";

            if (sectorText != null)
                sectorText.text = mission != null ? mission.Title : "Сектор завершен";

            var mins = Mathf.FloorToInt(clearTime / 60f);
            var secs = Mathf.FloorToInt(clearTime % 60f);
            if (timeText != null)
                timeText.text = $"Время: {mins:00}:{secs:00}";

            if (hpText != null)
                hpText.text = $"Прочность: {hp} / {maxHp}";

            if (parryText != null)
                parryText.text = $"Парирования: {parries}";

            if (superText != null)
                superText.text = $"Супервыстрелы: {supersUsed}";

            // Cuphead Grade
            var grade = GameRules.CalculateGrade(hp, maxHp, parries, clearTime);
            if (gradeText != null)
            {
                gradeText.text = grade;
                gradeText.color = (grade == "S") ? new Color(1f, 0.85f, 0.2f) : (grade.StartsWith("A")) ? new Color(0.3f, 1f, 0.5f) : Color.white;
            }

            var baseReward = 30 + (missionIndex + 1) * 8;
            var gradeBonus = (grade == "S") ? 25 : (grade.StartsWith("A") ? 15 : 5);
            var reward = baseReward + gradeBonus;
            SaveManager.Instance?.AddCoins(reward);

            if (grade == "S")
                SaveManager.Instance?.AddStarCores(1);

            if (rewardText != null)
                rewardText.text = (grade == "S")
                    ? $"Награда: +{reward} кредитов   +1 Звёздное Ядро!"
                    : $"Награда: +{reward} кредитов";

            if (doubleRewardBtn != null)
            {
                doubleRewardBtn.gameObject.SetActive(true);
                doubleRewardBtn.interactable = true;
                doubleRewardBtn.onClick.RemoveAllListeners();
                doubleRewardBtn.onClick.AddListener(() =>
                {
                    AudioManager.PlayClick();
                    YandexGamesService.Instance?.ShowRewarded(() =>
                    {
                        SaveManager.Instance?.AddCoins(reward);
                        doubleRewardBtn.gameObject.SetActive(false);
                        rewardText.text = (grade == "S")
                            ? $"Награда: +{reward * 2} кредитов (УДВОЕНО!)   +1 Звёздное Ядро!"
                            : $"Награда: +{reward * 2} кредитов (УДВОЕНО!)";
                        AudioManager.PlayPowerup();
                    });
                });
            }

            var hasNext = (missionIndex + 1) < MissionCatalog.All.Length;
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(hasNext);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() =>
                {
                    AudioManager.PlayClick();
                    Time.timeScale = 1f;
                    onNext?.Invoke();
                });
            }
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("VictoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            var dim = CreateImage("Dim", modalRoot.transform, PixelUiAtlas.White, new Color(0.02f, 0.04f, 0.08f, 0.78f));
            Stretch(dim.rectTransform);

            // Center Panel
            var panel = CreateImage("Panel", modalRoot.transform, PixelUiAtlas.Panel, Color.white);
            panel.type = Image.Type.Sliced;
            panel.rectTransform.sizeDelta = new Vector2(740f, 620f);

            // Banner Title
            titleText = CreateLabel(panel.transform, "Title", "СЕКТОР ЗАЧИЩЕН!", 34, new Vector2(0f, 245f), new Color(1f, 0.85f, 0.25f, 1f));
            titleText.fontStyle = FontStyle.Bold;

            sectorText = CreateLabel(panel.transform, "Sector", "Сектор 1", 20, new Vector2(0f, 198f), new Color(0.75f, 0.88f, 1f, 1f));

            // Stats Sub-Panel
            var subPanel = CreateImage("StatsPanel", panel.transform, PixelUiAtlas.SubPanel, Color.white);
            subPanel.type = Image.Type.Sliced;
            subPanel.rectTransform.anchoredPosition = new Vector2(0f, 65f);
            subPanel.rectTransform.sizeDelta = new Vector2(640f, 180f);

            timeText = CreateLabel(subPanel.transform, "Time", "⏱ Время: 00:00", 20, new Vector2(35f, 45f), Color.white);
            timeText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            timeText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            timeText.rectTransform.pivot = new Vector2(0f, 0.5f);
            timeText.alignment = TextAnchor.MiddleLeft;

            hpText = CreateLabel(subPanel.transform, "HP", "❤️ Прочность: 3 / 3", 20, new Vector2(35f, 10f), Color.white);
            hpText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            hpText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            hpText.rectTransform.pivot = new Vector2(0f, 0.5f);
            hpText.alignment = TextAnchor.MiddleLeft;

            parryText = CreateLabel(subPanel.transform, "Parry", "Парирования: 0", 20, new Vector2(35f, -25f), Color.white);
            parryText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            parryText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            parryText.rectTransform.pivot = new Vector2(0f, 0.5f);
            parryText.alignment = TextAnchor.MiddleLeft;

            superText = CreateLabel(subPanel.transform, "Super", "Супервыстрелы: 0", 20, new Vector2(35f, -60f), Color.white);
            superText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            superText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            superText.rectTransform.pivot = new Vector2(0f, 0.5f);
            superText.alignment = TextAnchor.MiddleLeft;

            // Cuphead Grade Stamp Box
            var gradeBox = CreateImage("GradeBox", subPanel.transform, PixelUiAtlas.Panel, new Color(0.06f, 0.15f, 0.24f, 1f));
            gradeBox.type = Image.Type.Sliced;
            gradeBox.rectTransform.anchoredPosition = new Vector2(180f, -5f);
            gradeBox.rectTransform.sizeDelta = new Vector2(120f, 130f);

            var gradeHeader = CreateLabel(gradeBox.transform, "GHeader", " РАНГ ", 16, new Vector2(0f, 42f), new Color(0.7f, 0.85f, 1f));
            gradeText = CreateLabel(gradeBox.transform, "Grade", "S", 58, new Vector2(0f, -8f), new Color(1f, 0.85f, 0.2f));
            gradeText.fontStyle = FontStyle.Bold;

            rewardText = CreateLabel(panel.transform, "Reward", "Награда: +70 кредитов", 22, new Vector2(0f, -70f), new Color(1f, 0.85f, 0.35f));
            rewardText.fontStyle = FontStyle.Bold;

            // Action Buttons
            doubleRewardBtn = CreateButton(panel.transform, "DoubleRewardBtn", "⚡ УДВОИТЬ КРЕДИТЫ (РЕКЛАМА x2)", new Vector2(0f, -125f), new Vector2(480f, 48f), PixelUiAtlas.ButtonPrimary);

            nextButton = CreateButton(panel.transform, "NextButton", "СЛЕДУЮЩИЙ СЕКТОР  ▶", new Vector2(0f, -185f), new Vector2(480f, 52f), PixelUiAtlas.ButtonPrimary);

            var retryButton = CreateButton(panel.transform, "RetryButton", "ПЕРЕИГРАТЬ СЕКТОР", new Vector2(-135f, -248f), new Vector2(230f, 46f), PixelUiAtlas.ButtonSecondary);
            retryButton.onClick.AddListener(() =>
            {
                AudioManager.PlayClick();
                Time.timeScale = 1f;
                SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
            });

            var menuButton = CreateButton(panel.transform, "MenuButton", "В ГЛАВНОЕ МЕНЮ", new Vector2(135f, -248f), new Vector2(230f, 46f), PixelUiAtlas.ButtonDanger);
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
            rect.sizeDelta = new Vector2(400f, 40f);

            var label = obj.GetComponent<Text>();
            label.font = UIFont.DefaultFont;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
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

            var label = CreateLabel(obj.transform, "Label", text, 18, Vector2.zero, Color.white);
            label.rectTransform.sizeDelta = size;
            label.fontStyle = FontStyle.Bold;
            var btn = obj.GetComponent<Button>();
            UITween.AttachButtonFeedback(btn);
            return btn;
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
