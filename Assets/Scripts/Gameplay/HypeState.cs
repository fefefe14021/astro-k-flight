using System;
using KANYE.Systems;
using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class HypeState : MonoBehaviour
    {
        public static HypeState Instance { get; private set; }

        [SerializeField] private float hypeMax = 100f;
        [SerializeField] private float hypePerKill = 25f;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public int Albums { get; private set; }
        public int AlbumsRequired { get; private set; }
        public float Route { get; private set; }
        public float RouteRequired { get; private set; }
        public float Hype { get; private set; }
        public float HypeMax => hypeMax;

        public const float EXCost = 25f;
        public bool CanUseEX => Hype >= EXCost && (GameRules.IsEXUnlocked || GameRules.TutorialArsenal) && TutorialHints.PracticeAllowsEX;
        public bool CanUseSuper => Hype >= hypeMax - 0.01f && (GameRules.IsSuperUnlocked || GameRules.TutorialArsenal) && TutorialHints.PracticeAllowsSuper;
        public int CardsCount => Mathf.Clamp(Mathf.FloorToInt(Hype / EXCost), 0, 4);
        public static bool RageCanCharge => GameRules.IsEXUnlocked || GameRules.TutorialArsenal;

        public event Action Changed;
        public event Action SuperFired;
        public event Action EXFired;

        public void BeginMission(int albumsRequired)
        {
            Albums = 0;
            Hype = 0f;
            AlbumsRequired = albumsRequired;
            Route = 0f;
            RouteRequired = Mathf.Max(1, albumsRequired);
            Changed?.Invoke();
        }

        public void AdvanceRoute(float kilometers)
        {
            if (kilometers <= 0f || Route >= RouteRequired)
                return;

            Route = Mathf.Min(RouteRequired, Route + kilometers);
            Changed?.Invoke();
        }

        public void AddAlbum(int count, bool grantHype = true)
        {
            if (count <= 0)
                return;

            Albums += count;
            if (grantHype && RageCanCharge)
            {
                var gain = count * hypePerKill;
                if (GameRules.CurrentMode == GameMode.RogueExpedition)
                    gain *= RogueExpeditionController.HypeMultiplier;

                Hype = Mathf.Min(hypeMax, Hype + gain);
            }

            Changed?.Invoke();
        }

        public void AddHype(float amount)
        {
            if (amount <= 0f || !RageCanCharge) return;
            if (GameRules.CurrentMode == GameMode.RogueExpedition)
                amount *= RogueExpeditionController.HypeMultiplier;

            Hype = Mathf.Min(hypeMax, Hype + amount);
            Changed?.Invoke();
        }

        public bool TryUseEX()
        {
            if (!CanUseEX)
                return false;

            Hype = Mathf.Max(0f, Hype - EXCost);
            Changed?.Invoke();
            EXFired?.Invoke();
            return true;
        }

        public bool TryUseSuper()
        {
            if (!CanUseSuper)
                return false;

            Hype = 0f;
            Changed?.Invoke();
            SuperFired?.Invoke();
            return true;
        }
    }
}
