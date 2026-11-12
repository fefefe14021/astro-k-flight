using System;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class HypeState : MonoBehaviour
    {
        [SerializeField] private float hypeMax = 100f;
        [SerializeField] private float hypePerKill = 25f;

        public int Albums { get; private set; }
        public int AlbumsRequired { get; private set; }
        public float Hype { get; private set; }
        public float HypeMax => hypeMax;

        public const float EXCost = 25f;
        public bool CanUseEX => Hype >= EXCost && GameRules.IsEXUnlocked;
        public bool CanUseSuper => Hype >= hypeMax - 0.01f && GameRules.IsSuperUnlocked;
        public int CardsCount => Mathf.Clamp(Mathf.FloorToInt(Hype / EXCost), 0, 4);

        public event Action Changed;
        public event Action SuperFired;
        public event Action EXFired;

        public void BeginMission(int albumsRequired)
        {
            Albums = 0;
            Hype = 0f;
            AlbumsRequired = albumsRequired;
            Changed?.Invoke();
        }

        public void AddAlbum(int count)
        {
            if (count <= 0)
                return;

            Albums += count;
            var gain = count * hypePerKill;
            if (GameRules.CurrentMode == GameMode.RogueExpedition)
                gain *= RogueExpeditionController.HypeMultiplier;

            Hype = Mathf.Min(hypeMax, Hype + gain);
            Changed?.Invoke();
        }

        public void AddHype(float amount)
        {
            if (amount <= 0f) return;
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
