// Assets/Scripts/Services/YandexGamesService.cs
// Namespace: KANYE.Services
// GameObject: YandexGames, создаётся сам и живёт между сценами.
// В редакторе подменяет SDK, чтобы игра запускалась локально. В WebGL на Яндекс Играх вызывает YaGames.init и читает имя игрока.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KANYE.Services
{
    public class YandexGamesService : MonoBehaviour
    {
        public static YandexGamesService Instance { get; private set; }

        public bool IsReady { get; private set; }
        public bool IsAuthorized { get; private set; }
        public string PlayerName { get; private set; } = "Гость";
        public string PlayerId { get; private set; } = "";
        public string Status { get; private set; } = "SDK ещё не подключён";

        public event Action Changed;

        private Action pendingReward;
        private Action pendingInterstitial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;

            var host = new GameObject("YandexGames");
            DontDestroyOnLoad(host);
            host.AddComponent<YandexGamesService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Connect();
        }

        public void Connect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            IsReady = false;
            Status = "Подключение SDK Яндекс Игр...";
            Changed?.Invoke();
            YandexGamesBridge.Init(gameObject.name, nameof(OnSdkMessage));
#else
            IsReady = true;
            IsAuthorized = false;
            PlayerName = "Гость";
            PlayerId = "local";
            Status = "Локальный запуск. Имя Яндекса появится в сборке WebGL на площадке.";
            Changed?.Invoke();
#endif
        }

        public void RequestAuthorization()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsReady)
                return;
            Status = "Ожидание авторизации Яндекс Игр...";
            Changed?.Invoke();
            YandexGamesBridge.Authorize(gameObject.name, nameof(OnSdkMessage));
#else
            IsAuthorized = true;
            PlayerName = "Локальный пилот";
            PlayerId = "local-pilot";
            Status = "Локальный вход. В сборке здесь откроется окно Яндекс Игр.";
            Changed?.Invoke();
#endif
        }

        public void ShowRewarded(Action onRewarded)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsReady)
            {
                onRewarded?.Invoke();
                return;
            }
            pendingReward = onRewarded;
            YandexGamesBridge.ShowRewarded(gameObject.name, nameof(OnAdMessage));
#else
            Status = "В редакторе ролик пропускается, награда выдаётся сразу.";
            Changed?.Invoke();
            onRewarded?.Invoke();
#endif
        }

        public void ShowInterstitial(Action onFinished)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsReady)
            {
                onFinished?.Invoke();
                return;
            }
            pendingInterstitial = onFinished;
            YandexGamesBridge.ShowInterstitial(gameObject.name, nameof(OnAdMessage));
#else
            onFinished?.Invoke();
#endif
        }

        public void OnAdMessage(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return;

            if (payload.StartsWith("rewarded|"))
            {
                var grant = payload.EndsWith("|1") || payload.EndsWith("|error");
                var callback = pendingReward;
                pendingReward = null;
                if (grant)
                    callback?.Invoke();
                return;
            }

            if (payload.StartsWith("fullscreen|"))
            {
                var callback = pendingInterstitial;
                pendingInterstitial = null;
                callback?.Invoke();
            }
        }

        public void MarkGameReady()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            YandexGamesBridge.GameReady();
#endif
        }

        public void OnSdkMessage(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return;

            var parts = payload.Split('|');
            var kind = parts[0];
            if (kind == "error" || kind == "cancel")
            {
                Status = parts.Length > 1 ? parts[1] : "SDK Яндекс Игр не ответил";
                IsReady = true;
                Changed?.Invoke();
                return;
            }

            var reportedName = parts.Length > 1 ? parts[1] : "";
            PlayerId = parts.Length > 2 ? parts[2] : "";
            IsAuthorized = parts.Length > 3 && parts[3] == "1";
            PlayerName = IsAuthorized && !string.IsNullOrEmpty(reportedName) ? reportedName : "Гость";
            IsReady = true;
            Status = IsAuthorized ? "Авторизация выполнена" : "SDK подключён";
            Changed?.Invoke();
        }
    }

    internal static class YandexGamesBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void YG_Init(string objectName, string methodName);

        [DllImport("__Internal")]
        public static extern void YG_Authorize(string objectName, string methodName);

        [DllImport("__Internal")]
        public static extern void YG_GameReady();

        [DllImport("__Internal")]
        public static extern void YG_ShowRewarded(string objectName, string methodName);

        [DllImport("__Internal")]
        public static extern void YG_ShowInterstitial(string objectName, string methodName);

        public static void Init(string objectName, string methodName) => YG_Init(objectName, methodName);
        public static void Authorize(string objectName, string methodName) => YG_Authorize(objectName, methodName);
        public static void GameReady() => YG_GameReady();
        public static void ShowRewarded(string objectName, string methodName) => YG_ShowRewarded(objectName, methodName);
        public static void ShowInterstitial(string objectName, string methodName) => YG_ShowInterstitial(objectName, methodName);
#else
        public static void Init(string objectName, string methodName) { }
        public static void Authorize(string objectName, string methodName) { }
        public static void GameReady() { }
        public static void ShowRewarded(string objectName, string methodName) { }
        public static void ShowInterstitial(string objectName, string methodName) { }
#endif
    }
}
