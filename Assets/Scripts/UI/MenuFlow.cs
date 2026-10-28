using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class MenuFlow : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying)
                return;

            if (scene.name == "Bootstrap")
            {
                SceneLoader.LoadSceneByName(SceneLoader.MainMenuScene);
                return;
            }

            if (scene.name == SceneLoader.MainMenuScene)
                EnsureMenu();
            else if (scene.name == SceneLoader.ShopScene)
                EnsureShop();
            else if (scene.name == SceneLoader.AchievementsScene)
                EnsureAchievements();
            else if (scene.name == SceneLoader.GameplayScene)
                EnsureBackButton();
        }

        private static void EnsureMenu()
        {
            if (Object.FindAnyObjectByType<MenuPanelSwitcher>() != null)
                return;
            if (Object.FindAnyObjectByType<MenuFlow>() != null)
                return;

            new GameObject("MenuFlow").AddComponent<MenuFlow>();
        }

        private static void EnsureShop()
        {
            if (Object.FindAnyObjectByType<ShopScreen>() != null)
                return;

            new GameObject("ShopScreen").AddComponent<ShopScreen>();
        }

        private static void EnsureAchievements()
        {
            if (Object.FindAnyObjectByType<AchievementsScreen>() != null)
                return;

            new GameObject("AchievementsScreen").AddComponent<AchievementsScreen>();
        }

        private static void EnsureBackButton()
        {
            if (GameObject.Find("TemporaryBackButton") != null)
                return;

            EnsureEventSystem();
            var canvas = CreateCanvas("TemporaryBackCanvas", 120);
            var button = CreateButton(canvas.transform, "TemporaryBackButton", "Назад", new Vector2(0f, 1f), new Vector2(120f, -40f), new Vector2(180f, 56f));
            button.onClick.AddListener(SceneLoader.ReturnToMainMenu);
        }

        private void Start()
        {
            EnsureEventSystem();
            var canvas = CreateCanvas("TemporaryMenuCanvas", 50);
            var missionIndex = SaveManager.Instance != null ? SaveManager.Instance.CurrentMission : 0;
            var cleared = missionIndex >= MissionCatalog.All.Length;
            var continueMission = MissionCatalog.All[Mathf.Clamp(missionIndex, 0, MissionCatalog.All.Length - 1)];
            var missionLine = cleared ? "Все миссии пройдены" : "Дальше: " + continueMission.Title;
            CreateLabel(canvas.transform, "Title", "Astro K", new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), 42);
            CreateLabel(canvas.transform, "MissionLine", missionLine, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), 22);
            var play = CreateButton(canvas.transform, "PlayButton", cleared ? "Сначала" : "Играть", new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(280f, 72f));
            var y = 20f;
            if (missionIndex > 0 && !cleared)
            {
                var restart = CreateButton(canvas.transform, "RestartButton", "С начала", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(280f, 64f));
                restart.onClick.AddListener(SceneLoader.RestartFromFirstMission);
                y -= 80f;
            }

            var achievements = CreateButton(canvas.transform, "AchievementsButton", "Достижения", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(280f, 64f));
            y -= 80f;
            var shop = CreateButton(canvas.transform, "ShopButton", "Магазин", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(280f, 64f));
            y -= 80f;
            var quit = CreateButton(canvas.transform, "QuitButton", "Выход", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(280f, 64f));
            play.onClick.AddListener(() =>
            {
                var index = SaveManager.Instance != null ? SaveManager.Instance.CurrentMission : 0;
                if (index >= MissionCatalog.All.Length)
                {
                    SceneLoader.RestartFromFirstMission();
                    return;
                }

                MissionCatalog.Select(index);
                SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
            });
            achievements.onClick.AddListener(SceneLoader.OpenAchievements);
            shop.onClick.AddListener(SceneLoader.OpenShop);
            quit.onClick.AddListener(SceneLoader.Quit);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
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

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            return buttonObject.GetComponent<Button>();
        }

        private static Text CreateLabel(Transform parent, string name, string label, Vector2 anchor, Vector2 position, int fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900f, 120f);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            return text;
        }
    }
}
