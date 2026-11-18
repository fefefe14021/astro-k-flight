// Assets/Scripts/UI/FloatingCombatText.cs
// Namespace: KANYE.UI
// Lightweight, zero-GC pooled floating arcade combat text for damage numbers, crits, parries, and rewards.

using System.Collections.Generic;
using UnityEngine;

namespace KANYE.UI
{
    public class FloatingCombatText : MonoBehaviour
    {
        public static FloatingCombatText Instance { get; private set; }

        private const int PoolSize = 36;
        private readonly Queue<TextItem> pool = new Queue<TextItem>(PoolSize);
        private readonly List<TextItem> activeItems = new List<TextItem>(PoolSize);

        private class TextItem
        {
            public GameObject go;
            public TextMesh textMesh;
            public Transform transform;
            public Vector3 velocity;
            public float timer;
            public float duration;
            public float baseScale;
            public bool isCrit;
            public Color baseColor;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoHook()
        {
            if (Instance == null)
            {
                var go = new GameObject("[FloatingCombatText]");
                Instance = go.AddComponent<FloatingCombatText>();
                if (Application.isPlaying)
                    DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            PrewarmPool();
        }

        private void PrewarmPool()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var item = CreateItem();
                item.go.SetActive(false);
                pool.Enqueue(item);
            }
        }

        private TextItem CreateItem()
        {
            var go = new GameObject("CombatTextItem");
            go.transform.SetParent(transform, false);

            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 28;
            tm.characterSize = 0.13f;
            tm.fontStyle = FontStyle.Bold;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sortingOrder = 150; // Render in front of entities & particles

            return new TextItem
            {
                go = go,
                textMesh = tm,
                transform = go.transform,
                duration = 0.55f,
                baseScale = 1.0f
            };
        }

        public static void ShowDamage(Vector3 worldPos, int amount, bool isCrit = false)
        {
            if (Instance == null || amount <= 0) return;

            var str = isCrit ? $"-{amount}!" : $"-{amount}";
            var col = isCrit ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(1f, 0.55f, 0.25f, 0.95f);
            var scale = isCrit ? 1.35f : 1.0f;

            Instance.Spawn(worldPos, str, col, scale, isCrit);
        }

        public static void ShowParry(Vector3 worldPos)
        {
            if (Instance == null) return;
            Instance.Spawn(worldPos, "⚡ PARRY!", new Color(1f, 0.25f, 0.85f, 1f), 1.4f, true);
        }

        public static void ShowHeal(Vector3 worldPos, int amount)
        {
            if (Instance == null) return;
            Instance.Spawn(worldPos, $"+{amount} HP", new Color(0.3f, 1f, 0.4f, 1f), 1.15f, false);
        }

        public static void ShowCoins(Vector3 worldPos, int amount)
        {
            if (Instance == null || amount <= 0) return;
            Instance.Spawn(worldPos, $"+{amount}¢", new Color(1f, 0.9f, 0.3f, 1f), 1.1f, false);
        }

        public static void ShowText(Vector3 worldPos, string text, Color color, float scale = 1.0f)
        {
            if (Instance == null) return;
            Instance.Spawn(worldPos, text, color, scale, false);
        }

        private void Spawn(Vector3 worldPos, string text, Color color, float scale, bool isCrit)
        {
            TextItem item;
            if (pool.Count > 0)
            {
                item = pool.Dequeue();
            }
            else if (activeItems.Count > 0)
            {
                // Recycle oldest active item
                item = activeItems[0];
                activeItems.RemoveAt(0);
            }
            else
            {
                item = CreateItem();
            }

            // Slight random scatter on X/Y so overlapping hits remain legible
            var scatter = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.15f, 0.25f), 0f);
            item.transform.position = worldPos + scatter;
            item.transform.localScale = Vector3.one * (scale * (isCrit ? 1.4f : 0.8f));

            item.textMesh.text = text;
            item.textMesh.color = color;
            item.baseColor = color;
            item.baseScale = scale;
            item.isCrit = isCrit;
            item.timer = 0f;
            item.duration = isCrit ? 0.65f : 0.50f;
            item.velocity = new Vector2(Random.Range(-0.35f, 0.35f), isCrit ? 2.2f : 1.7f);

            item.go.SetActive(true);
            activeItems.Add(item);
        }

        private void Update()
        {
            if (activeItems.Count == 0) return;

            var dt = Time.deltaTime;
            for (var i = activeItems.Count - 1; i >= 0; i--)
            {
                var item = activeItems[i];
                item.timer += dt;

                // Drift upward with deceleration
                item.transform.position += (Vector3)(item.velocity * dt);
                item.velocity.y = Mathf.MoveTowards(item.velocity.y, 0.4f, dt * 2.5f);

                // Punchy scale curve: pop in -> settle to base scale
                var t = item.timer / item.duration;
                if (t < 0.2f)
                {
                    var pop = Mathf.Lerp(item.baseScale * (item.isCrit ? 1.4f : 0.8f), item.baseScale, t / 0.2f);
                    item.transform.localScale = Vector3.one * pop;
                }
                else
                {
                    item.transform.localScale = Vector3.one * item.baseScale;
                }

                // Alpha fade out in last 40% of duration
                if (t > 0.6f)
                {
                    var fadeT = (t - 0.6f) / 0.4f;
                    var col = item.baseColor;
                    col.a = Mathf.Lerp(item.baseColor.a, 0f, fadeT);
                    item.textMesh.color = col;
                }

                if (item.timer >= item.duration)
                {
                    item.go.SetActive(false);
                    activeItems.RemoveAt(i);
                    pool.Enqueue(item);
                }
            }
        }
    }
}
