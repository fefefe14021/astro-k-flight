// Assets/Scripts/UI/MenuParticleSystem.cs
// Namespace: KANYE.UI
// High-performance Canvas UI Particle System for ambient space dust, thruster embers, and victory bursts.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class MenuParticleSystem : MonoBehaviour
    {
        public static MenuParticleSystem Instance { get; private set; }

        private const int MaxAmbient = 28;
        private const int PoolSize = 80;

        private RectTransform canvasRect;
        private readonly List<UIParticle> activeParticles = new List<UIParticle>(PoolSize);
        private readonly Queue<UIParticle> pool = new Queue<UIParticle>(PoolSize);

        private class UIParticle
        {
            public GameObject go;
            public RectTransform rect;
            public Image img;
            public Vector2 velocity;
            public float lifetime;
            public float maxLifetime;
            public Color startColor;
            public Color endColor;
            public float startScale;
            public float endScale;
            public bool isAmbient;
            public float twinkleSpeed;
            public float twinkleOffset;
        }

        public static MenuParticleSystem Create(Canvas canvas)
        {
            if (canvas == null) return null;
            var go = new GameObject("MenuParticleSystem", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            // Place it behind dialog cards but in front of background
            go.transform.SetSiblingIndex(2);

            var ps = go.AddComponent<MenuParticleSystem>();
            ps.canvasRect = canvas.GetComponent<RectTransform>();
            ps.InitPool();
            ps.SpawnInitialAmbientDust();
            Instance = ps;
            return ps;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            if (canvasRect == null)
                canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
        }

        private void InitPool()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var pGo = new GameObject($"UIP_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                pGo.transform.SetParent(transform, false);
                var img = pGo.GetComponent<Image>();
                img.sprite = PixelUiAtlas.White;
                img.raycastTarget = false;
                pGo.SetActive(false);

                pool.Enqueue(new UIParticle
                {
                    go = pGo,
                    rect = pGo.GetComponent<RectTransform>(),
                    img = img
                });
            }
        }

        private void SpawnInitialAmbientDust()
        {
            if (canvasRect == null) return;
            var halfW = canvasRect.rect.width * 0.5f;
            var halfH = canvasRect.rect.height * 0.5f;

            for (var i = 0; i < MaxAmbient; i++)
            {
                var x = Random.Range(-halfW, halfW);
                var y = Random.Range(-halfH, halfH);
                SpawnAmbientParticle(new Vector2(x, y));
            }
        }

        private void SpawnAmbientParticle(Vector2 pos)
        {
            if (pool.Count == 0) return;
            var p = pool.Dequeue();
            p.go.SetActive(true);
            p.rect.anchoredPosition = pos;
            p.isAmbient = true;
            p.lifetime = 0f;
            p.maxLifetime = Random.Range(6f, 12f);

            var speed = Random.Range(10f, 28f);
            var angle = Random.Range(-160f, -130f) * Mathf.Deg2Rad;
            p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            var size = Random.Range(2f, 4.5f);
            p.rect.sizeDelta = new Vector2(size, size);
            p.startScale = 1f;
            p.endScale = 1f;

            // Cosmic stardust colors: cyan, amber, or lavender
            var colorRoll = Random.value;
            if (colorRoll < 0.5f)
                p.startColor = new Color(0.2f, 0.85f, 1f, Random.Range(0.25f, 0.75f));
            else if (colorRoll < 0.8f)
                p.startColor = new Color(1f, 0.85f, 0.35f, Random.Range(0.25f, 0.65f));
            else
                p.startColor = new Color(0.85f, 0.45f, 1f, Random.Range(0.25f, 0.65f));

            p.endColor = p.startColor;
            p.twinkleSpeed = Random.Range(1.8f, 4.5f);
            p.twinkleOffset = Random.Range(0f, 6.28f);
            p.img.color = p.startColor;

            activeParticles.Add(p);
        }

        public void EmitBurst(Vector2 localPos, Color color, int count = 24, Sprite customSprite = null)
        {
            for (var i = 0; i < count; i++)
            {
                if (pool.Count == 0) break;
                var p = pool.Dequeue();
                p.go.SetActive(true);
                p.rect.anchoredPosition = localPos;
                p.isAmbient = false;
                p.lifetime = 0f;
                p.maxLifetime = Random.Range(0.6f, 1.2f);

                var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var speed = Random.Range(120f, 320f);
                p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                var size = customSprite != null ? Random.Range(14f, 22f) : Random.Range(4f, 8f);
                p.rect.sizeDelta = new Vector2(size, size);
                p.img.sprite = customSprite != null ? customSprite : PixelUiAtlas.White;

                p.startScale = 1.2f;
                p.endScale = 0f;
                p.startColor = color;
                p.endColor = new Color(color.r, color.g, color.b, 0f);
                p.img.color = color;

                activeParticles.Add(p);
            }
        }

        public void EmitThrusterEmber(Vector2 localPos, Color color)
        {
            if (pool.Count == 0) return;
            var p = pool.Dequeue();
            p.go.SetActive(true);
            p.rect.anchoredPosition = localPos + new Vector2(Random.Range(-4f, 4f), Random.Range(-4f, 4f));
            p.isAmbient = false;
            p.lifetime = 0f;
            p.maxLifetime = Random.Range(0.25f, 0.55f);

            // Drift backwards (left) with slight spread
            p.velocity = new Vector2(Random.Range(-140f, -80f), Random.Range(-25f, 25f));
            var size = Random.Range(3f, 6f);
            p.rect.sizeDelta = new Vector2(size, size);
            p.img.sprite = PixelUiAtlas.White;

            p.startScale = 1f;
            p.endScale = 0.1f;
            p.startColor = color;
            p.endColor = new Color(color.r * 0.4f, color.g * 0.4f, 1f, 0f);
            p.img.color = color;

            activeParticles.Add(p);
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            var halfW = canvasRect != null ? canvasRect.rect.width * 0.5f : 480f;
            var halfH = canvasRect != null ? canvasRect.rect.height * 0.5f : 270f;
            var ambientCount = 0;

            for (var i = activeParticles.Count - 1; i >= 0; i--)
            {
                var p = activeParticles[i];
                p.lifetime += dt;

                if (p.isAmbient)
                {
                    ambientCount++;
                    // Soft drifting motion
                    p.rect.anchoredPosition += p.velocity * dt;

                    // Twinkle brightness
                    var tw = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * p.twinkleSpeed + p.twinkleOffset);
                    var c = p.startColor;
                    c.a *= tw;
                    p.img.color = c;

                    // Wrap around screen bounds
                    var pos = p.rect.anchoredPosition;
                    if (pos.x < -halfW - 20f) pos.x = halfW + 20f;
                    if (pos.y < -halfH - 20f) pos.y = halfH + 20f;
                    p.rect.anchoredPosition = pos;

                    if (p.lifetime >= p.maxLifetime)
                    {
                        // Reset ambient life
                        p.lifetime = 0f;
                    }
                }
                else
                {
                    // Dynamic burst / ember with deceleration and gravity
                    p.velocity += new Vector2(0f, -180f) * dt; // subtle downward gravity
                    p.velocity = Vector2.Lerp(p.velocity, Vector2.zero, dt * 1.5f); // drag
                    p.rect.anchoredPosition += p.velocity * dt;

                    var norm = Mathf.Clamp01(p.lifetime / p.maxLifetime);
                    var scale = Mathf.Lerp(p.startScale, p.endScale, norm);
                    p.rect.localScale = Vector3.one * scale;
                    p.img.color = Color.Lerp(p.startColor, p.endColor, norm);

                    if (p.lifetime >= p.maxLifetime)
                    {
                        p.go.SetActive(false);
                        activeParticles.RemoveAt(i);
                        pool.Enqueue(p);
                    }
                }
            }

            // Keep ambient count full
            while (ambientCount < MaxAmbient && pool.Count > 0)
            {
                var startX = halfW + 10f;
                var startY = Random.Range(-halfH, halfH);
                SpawnAmbientParticle(new Vector2(startX, startY));
                ambientCount++;
            }
        }
    }
}
