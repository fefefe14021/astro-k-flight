using System;
using System.Collections.Generic;
using UnityEngine;

namespace KANYE.Systems
{
    [DefaultExecutionOrder(-100)]
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        private readonly List<AchievementData> definitions = new List<AchievementData>();

        public event Action Changed;
        public event Action<AchievementData> Unlocked;

        public IReadOnlyList<AchievementData> Definitions => definitions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            definitions.AddRange(Resources.LoadAll<AchievementData>("Achievements"));
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public AchievementData Find(string id)
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].id == id)
                    return definitions[i];
            }

            return null;
        }

        public int GetProgress(string id)
        {
            if (SaveManager.Instance == null)
                return 0;

            return SaveManager.Instance.GetAchievement(id).amount;
        }

        public bool IsUnlocked(string id)
        {
            if (SaveManager.Instance == null)
                return false;

            return SaveManager.Instance.GetAchievement(id).unlocked;
        }

        public void AddProgress(string id, int amount)
        {
            if (amount <= 0 || SaveManager.Instance == null)
                return;

            var definition = Find(id);
            if (definition == null)
            {
                Debug.LogWarning($"AchievementManager: нет ачивки с id {id}.");
                return;
            }

            var entry = SaveManager.Instance.GetAchievement(id);
            if (entry.unlocked)
                return;

            entry.amount += amount;
            if (entry.amount >= definition.targetAmount)
            {
                entry.amount = definition.targetAmount;
                entry.unlocked = true;
                SaveManager.Instance.Save();
                Changed?.Invoke();
                Unlocked?.Invoke(definition);
                return;
            }

            SaveManager.Instance.Save();
            Changed?.Invoke();
        }

        public void UnlockAchievement(string id)
        {
            if (SaveManager.Instance == null)
                return;

            var definition = Find(id);
            if (definition == null)
            {
                Debug.LogWarning($"AchievementManager: нет ачивки с id {id}.");
                return;
            }

            var entry = SaveManager.Instance.GetAchievement(id);
            if (entry.unlocked)
                return;

            entry.amount = definition.targetAmount;
            entry.unlocked = true;
            SaveManager.Instance.Save();
            Changed?.Invoke();
            Unlocked?.Invoke(definition);
        }
    }
}
