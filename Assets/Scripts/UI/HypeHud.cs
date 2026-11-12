// Assets/Scripts/UI/HypeHud.cs
// Namespace: KANYE.UI
// Cuphead & Broforce style arcade HUD with segmented Hype cards, Boss Health Bar, and Lives hearts.

using System.Collections;
using KANYE.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class HypeHud : MonoBehaviour
    {
        public static HypeHud Instance { get; private set; }

        [SerializeField] private float fillSpeed = 0.4f;

        private HypeState hypeState;
        private ShipHealth shipHealth;

        private Image hypeFill;
        private Image[] cardPips = new Image[4];
        private Text albumLabel;
        private Text livesLabel;
        private Text parryLabel;

        // EX & Super Action Prompts
        private GameObject exButtonObj;
        private GameObject superButtonObj;

        // Boss Health Bar UI
        private GameObject bossBarObj;
        private Text bossNameText;
        private Image bossFillImage;

        // Big Banner System ("WALLOP!", "WARNING!", "KNOCKOUT!")
        private GameObject bannerObj;
        private Text bannerTitleText;
        private Text bannerSubText;
        private CanvasGroup bannerCanvasGroup;

        private float targetHypeFill;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying || scene.name != "Gameplay")
                return;

            if (FindAnyObjectByType<HypeHud>() != null)
                return;

            new GameObject("HypeHud").AddComponent<HypeHud>();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            hypeState = FindAnyObjectByType<HypeState>();
            shipHealth = FindAnyObjectByType<ShipHealth>();

            BuildHUD();

            if (hypeState != null)
                hypeState.Changed += Refresh;
            if (shipHealth != null)
                shipHealth.Changed += Refresh;

            Refresh();
            StartCoroutine(ShowMissionStartBanner());
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (hypeState != null)
                hypeState.Changed -= Refresh;
            if (shipHealth != null)
                shipHealth.Changed -= Refresh;
        }

        private IEnumerator ShowMissionStartBanner()
        {
            yield return new WaitForSeconds(0.4f);
            ShowBanner("READY? FIGHT!", "Сокруши вражеский флот и собери ресурсы!", 2.2f, new Color(1f, 0.85f, 0.2f));
        }

        private void Update()
        {
            // 1. Smooth Hype Meter Fill
            if (hypeFill != null)
            {
                var step = fillSpeed * Time.unscaledDeltaTime;
                hypeFill.fillAmount = Mathf.MoveTowards(hypeFill.fillAmount, targetHypeFill, step);

                var isFull = hypeFill.fillAmount >= 0.99f;
                if (isFull)
                {
                    var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f);
                    hypeFill.color = Color.Lerp(new Color(1f, 0.72f, 0.15f), new Color(1f, 0.98f, 0.65f), pulse);
                }
                else
                {
                    hypeFill.color = new Color(0.2f, 0.85f, 1f, 1f); // Sleek cyan energy
                }
            }

            // 2. EX & Super Button visibility
            var canEx = hypeState != null && hypeState.CanUseEX;
            var canSuper = hypeState != null && hypeState.CanUseSuper;

            if (exButtonObj != null && exButtonObj.activeSelf != (canEx && !canSuper))
                exButtonObj.SetActive(canEx && !canSuper);

            if (superButtonObj != null && superButtonObj.activeSelf != canSuper)
                superButtonObj.SetActive(canSuper);

            // 3. Boss Health Bar Binding
            var boss = BossController.ActiveBoss;
            if (boss != null && !boss.IsDead)
            {
                if (bossBarObj != null && !bossBarObj.activeSelf)
                {
                    bossBarObj.SetActive(true);
                    if (bossNameText != null)
                        bossNameText.text = "⚠️ " + boss.BossName;
                    ShowBanner("ВНИМАНИЕ: БОСС!", boss.BossName, 2.0f, new Color(1f, 0.25f, 0.25f));
                }

                if (bossFillImage != null && boss.MaxHealth > 0)
                {
                    var target = (float)boss.CurrentHealth / boss.MaxHealth;
                    bossFillImage.fillAmount = Mathf.MoveTowards(bossFillImage.fillAmount, target, Time.deltaTime * 1.5f);
                    bossFillImage.color = boss.IsEnraged ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.5f, 0.1f);
                }
            }
            else
            {
                if (bossBarObj != null && bossBarObj.activeSelf)
                    bossBarObj.SetActive(false);
            }
        }

        public void ShowBanner(string title, string subtitle, float duration, Color titleColor)
        {
            if (bannerObj == null) return;
            StartCoroutine(BannerRoutine(title, subtitle, duration, titleColor));
        }

        private IEnumerator BannerRoutine(string title, string subtitle, float duration, Color titleColor)
        {
            if (bannerTitleText != null)
            {
                bannerTitleText.text = title;
                bannerTitleText.color = titleColor;
            }
            if (bannerSubText != null)
                bannerSubText.text = subtitle;

            bannerObj.SetActive(true);
            bannerCanvasGroup.alpha = 0f;

            // Fade in
            var t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                bannerCanvasGroup.alpha = t / 0.25f;
                yield return null;
            }

            bannerCanvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(duration);

            // Fade out
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                bannerCanvasGroup.alpha = 1f - (t / 0.35f);
                yield return null;
            }

            bannerObj.SetActive(false);
        }

        private void Refresh()
        {
            if (hypeState != null)
            {
                targetHypeFill = hypeState.HypeMax <= 0f ? 0f : hypeState.Hype / hypeState.HypeMax;
                var req = hypeState.AlbumsRequired > 0 ? hypeState.AlbumsRequired : 6;
                if (albumLabel != null)
                    albumLabel.text = $"💿 АЛЬБОМЫ: {hypeState.Albums} / {req}";

                // Update EX Card pips
                var cards = hypeState.CardsCount;
                for (var i = 0; i < cardPips.Length; i++)
                {
                    if (cardPips[i] != null)
                    {
                        var filled = i < cards;
                        cardPips[i].color = filled 
                            ? (cards == 4 ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.25f, 0.95f, 1f, 1f)) 
                            : new Color(0.12f, 0.2f, 0.3f, 0.65f);
                    }
                }
            }

            if (shipHealth != null && livesLabel != null)
            {
                var hp = shipHealth.Lives;
                var hearts = "";
                for (var i = 0; i < shipHealth.MaxLives; i++)
                    hearts += (i < hp) ? "❤️ " : "🖤 ";

                livesLabel.text = shipHealth.IsDefeated ? "СБИТ" : hearts.TrimEnd();
            }

            if (parryLabel != null)
            {
                parryLabel.text = $"⚡ ПАРИ: {Systems.GameRules.ParriesThisRun}";
            }
        }

        private void BuildHUD()
        {
            var canvasObj = new GameObject("HypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // 1. Top-Left: Pilot Vitality (Lives hearts) & Parry count
            var livesPanel = CreateImage("LivesPanel", canvasObj.transform, PixelUiAtlas.SubPanel, Color.white);
            livesPanel.type = Image.Type.Sliced;
            livesPanel.rectTransform.anchorMin = new Vector2(0f, 1f);
            livesPanel.rectTransform.anchorMax = new Vector2(0f, 1f);
            livesPanel.rectTransform.pivot = new Vector2(0f, 1f);
            livesPanel.rectTransform.anchoredPosition = new Vector2(24f, -24f);
            livesPanel.rectTransform.sizeDelta = new Vector2(280f, 56f);

            livesLabel = CreateText("LivesText", livesPanel.transform, "❤️ ❤️ ❤️", 22, Vector2.zero, Color.white);
            livesLabel.fontStyle = FontStyle.Bold;

            parryLabel = CreateText("ParryText", canvasObj.transform, "⚡ ПАРИ: 0", 18, new Vector2(164f, -92f), new Color(1f, 0.45f, 0.85f, 1f));
            parryLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            parryLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            parryLabel.alignment = TextAnchor.MiddleLeft;

            // 2. Top-Center: Segmented Hype / EX Meter & Album Objectives
            var centerPanel = CreateImage("CenterPanel", canvasObj.transform, PixelUiAtlas.SubPanel, Color.white);
            centerPanel.type = Image.Type.Sliced;
            centerPanel.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            centerPanel.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            centerPanel.rectTransform.pivot = new Vector2(0.5f, 1f);
            centerPanel.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            centerPanel.rectTransform.sizeDelta = new Vector2(520f, 76f);

            albumLabel = CreateText("AlbumLabel", centerPanel.transform, "💿 АЛЬБОМЫ: 0 / 6", 20, new Vector2(0f, 16f), new Color(1f, 0.88f, 0.4f, 1f));
            albumLabel.fontStyle = FontStyle.Bold;

            // Meter Frame
            var meterFrame = CreateImage("MeterFrame", centerPanel.transform, PixelUiAtlas.White, new Color(0.04f, 0.08f, 0.14f, 0.95f));
            meterFrame.rectTransform.anchoredPosition = new Vector2(0f, -16f);
            meterFrame.rectTransform.sizeDelta = new Vector2(460f, 22f);

            // Continuous fill behind cards
            var fillObj = CreateImage("Fill", meterFrame.transform, PixelUiAtlas.White, new Color(0.2f, 0.85f, 1f, 1f));
            var fillRect = fillObj.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            hypeFill = fillObj;
            hypeFill.type = Image.Type.Filled;
            hypeFill.fillMethod = Image.FillMethod.Horizontal;
            hypeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            hypeFill.fillAmount = 0f;

            // 4 Card segment markers (Cuphead card style!)
            for (var i = 0; i < 4; i++)
            {
                var pip = CreateImage($"CardPip_{i}", meterFrame.transform, PixelUiAtlas.SubPanel, new Color(0.12f, 0.2f, 0.3f, 0.65f));
                pip.type = Image.Type.Sliced;
                var pipRect = pip.rectTransform;
                pipRect.anchorMin = new Vector2((float)i / 4f, 0f);
                pipRect.anchorMax = new Vector2((float)(i + 1) / 4f, 1f);
                pipRect.offsetMin = new Vector2(2f, 2f);
                pipRect.offsetMax = new Vector2(-2f, -2f);
                cardPips[i] = pip;
            }

            // 3. Action Prompts / Buttons for EX [Q] and Super [E]
            exButtonObj = CreatePromptButton(canvasObj.transform, "EXButton", "🚀 EX РАКЕТА [Q]", new Vector2(0f, -112f), new Color(0.2f, 0.75f, 1f), () =>
            {
                var shooter = FindAnyObjectByType<ShipShooter>();
                shooter?.TryFireEX();
            });
            exButtonObj.SetActive(false);

            superButtonObj = CreatePromptButton(canvasObj.transform, "SuperButton", "⚡ МЕГА-ЛАЗЕР [E]", new Vector2(0f, -112f), new Color(1f, 0.8f, 0.15f), () =>
            {
                var shooter = FindAnyObjectByType<ShipShooter>();
                shooter?.TryFireSuper();
            });
            superButtonObj.SetActive(false);

            // 4. Boss Health Bar (Top screen, below meters)
            bossBarObj = new GameObject("BossHealthBar", typeof(RectTransform));
            bossBarObj.transform.SetParent(canvasObj.transform, false);
            var bossRect = bossBarObj.GetComponent<RectTransform>();
            bossRect.anchorMin = new Vector2(0.5f, 1f);
            bossRect.anchorMax = new Vector2(0.5f, 1f);
            bossRect.pivot = new Vector2(0.5f, 1f);
            bossRect.anchoredPosition = new Vector2(0f, -105f);
            bossRect.sizeDelta = new Vector2(680f, 52f);

            var bossBg = CreateImage("BossBg", bossBarObj.transform, PixelUiAtlas.SubPanel, Color.white);
            bossBg.type = Image.Type.Sliced;
            Stretch(bossBg.rectTransform);

            bossNameText = CreateText("BossName", bossBarObj.transform, "⚠️ VANGUARD DREAD-DRONE", 17, new Vector2(0f, 12f), new Color(1f, 0.35f, 0.35f, 1f));
            bossNameText.fontStyle = FontStyle.Bold;

            var bossBarInner = CreateImage("BossInner", bossBarObj.transform, PixelUiAtlas.White, new Color(0.06f, 0.08f, 0.12f, 0.95f));
            bossBarInner.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            bossBarInner.rectTransform.sizeDelta = new Vector2(640f, 16f);

            var bFill = CreateImage("BossFill", bossBarInner.transform, PixelUiAtlas.White, new Color(1f, 0.45f, 0.15f, 1f));
            Stretch(bFill.rectTransform);
            bossFillImage = bFill;
            bossFillImage.type = Image.Type.Filled;
            bossFillImage.fillMethod = Image.FillMethod.Horizontal;
            bossFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            bossFillImage.fillAmount = 1f;

            bossBarObj.SetActive(false);

            // 5. Epic Banner Notifications ("READY? FIGHT!", "A KNOCKOUT!")
            bannerObj = new GameObject("BannerContainer", typeof(RectTransform), typeof(CanvasGroup));
            bannerObj.transform.SetParent(canvasObj.transform, false);
            bannerCanvasGroup = bannerObj.GetComponent<CanvasGroup>();
            var bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 80f);
            bannerRect.sizeDelta = new Vector2(720f, 140f);

            var bannerBg = CreateImage("BannerBg", bannerObj.transform, PixelUiAtlas.Panel, Color.white);
            bannerBg.type = Image.Type.Sliced;
            Stretch(bannerBg.rectTransform);

            bannerTitleText = CreateText("BannerTitle", bannerObj.transform, "READY? FIGHT!", 36, new Vector2(0f, 22f), new Color(1f, 0.85f, 0.2f));
            bannerTitleText.fontStyle = FontStyle.Bold;

            bannerSubText = CreateText("BannerSub", bannerObj.transform, "Сокруши вражеский флот!", 20, new Vector2(0f, -24f), new Color(0.85f, 0.92f, 1f));

            bannerObj.SetActive(false);
        }

        private static GameObject CreatePromptButton(Transform parent, string name, string text, Vector2 pos, Color highlight, System.Action onClick)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(260f, 38f);

            var img = btnObj.GetComponent<Image>();
            img.sprite = PixelUiAtlas.ButtonPrimary;
            img.type = Image.Type.Sliced;
            img.color = highlight;

            var label = CreateText("Label", btnObj.transform, text, 18, Vector2.zero, Color.white);
            label.fontStyle = FontStyle.Bold;

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btnObj;
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

        private static Text CreateText(string name, Transform parent, string text, int fontSize, Vector2 pos, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(500f, 36f);

            var label = obj.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            return label;
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
