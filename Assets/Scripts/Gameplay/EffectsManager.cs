// Assets/Scripts/Gameplay/EffectsManager.cs
// Namespace: KANYE.Gameplay
// GameObject: [EffectsManager] (создается автоматически или в Bootstrap)

using System.Collections.Generic;
using KANYE.Audio;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        private Sprite sparkSprite;
        private Sprite smokeSprite;
        private Sprite muzzleSprite;
        private Sprite debrisSprite;
        private Material spriteMaterial;

        // Pooled debris fragments for zero GC
        private readonly List<DebrisPiece> activeDebris = new List<DebrisPiece>(64);
        private readonly Queue<DebrisPiece> debrisPool = new Queue<DebrisPiece>(64);

        private class DebrisPiece
        {
            public GameObject go;
            public SpriteRenderer renderer;
            public Vector2 velocity;
            public float angularVelocity;
            public float lifetime;
            public float maxLifetime;
            public Color baseColor;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance == null)
            {
                var go = new GameObject("[EffectsManager]");
                Instance = go.AddComponent<EffectsManager>();
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
            LoadSprites();
            EnsureMaterial();
            PrewarmPool(32);
        }

        private void LoadSprites()
        {
            sparkSprite = Resources.Load<Sprite>("Sprites/spark");
            smokeSprite = Resources.Load<Sprite>("Sprites/smoke");
            muzzleSprite = Resources.Load<Sprite>("Sprites/muzzle");
            debrisSprite = Resources.Load<Sprite>("Sprites/debris");
        }

        private void EnsureMaterial()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                spriteMaterial = new Material(shader);
        }

        private void PrewarmPool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var piece = CreateDebrisObject();
                piece.go.SetActive(false);
                debrisPool.Enqueue(piece);
            }
        }

        private DebrisPiece CreateDebrisObject()
        {
            var go = new GameObject("Debris");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = debrisSprite;
            if (spriteMaterial != null)
                sr.sharedMaterial = spriteMaterial;
            sr.sortingOrder = 15;

            return new DebrisPiece
            {
                go = go,
                renderer = sr
            };
        }

        public static void PlayExplosion(Vector3 position, Color baseColor, bool isMechanical = false)
        {
            if (Instance == null)
                AutoCreate();

            AudioManager.PlayExplosion(isMechanical);
            Instance?.DoExplosion(position, baseColor, isMechanical);
        }

        public static void PlayImpact(Vector3 position, Vector2 normal, Color sparkColor)
        {
            if (Instance == null)
                AutoCreate();

            AudioManager.PlayHit();
            Instance?.DoImpact(position, normal, sparkColor);
        }

        public static void PlayParry(Vector3 position)
        {
            if (Instance == null)
                AutoCreate();

            AudioManager.PlayParry();
            Instance?.DoParry(position);
        }

        public static void PlayMuzzleFlash(Vector3 position)
        {
            PlayMuzzleFlash(position, new Color(1f, 0.9f, 0.5f, 1f), 1.2f);
        }

        public static void PlayMuzzleFlash(Vector3 position, Color flashColor, float scale = 1.2f)
        {
            if (Instance == null)
                AutoCreate();

            Instance?.DoMuzzleFlashCustom(position, flashColor, scale);
        }

        public static void PlayWeaponImpact(Vector3 position, Color impactColor, float blastRadius = 0.5f, bool isAoE = false)
        {
            if (Instance == null)
                AutoCreate();

            Instance?.DoWeaponImpact(position, impactColor, blastRadius, isAoE);
        }

        public static void EmitExhaustParticle(Vector3 position, Color particleColor, Vector2 velocity, string particleType)
        {
            if (Instance == null)
                AutoCreate();

            Instance?.DoExhaustParticle(position, particleColor, velocity, particleType);
        }

        private void DoExplosion(Vector3 position, Color baseColor, bool isMechanical)
        {
            // 1. Spawning debris chunks (Broforce/Terraria style physical debris)
            var chunkCount = isMechanical ? Random.Range(6, 10) : Random.Range(5, 8);
            for (var i = 0; i < chunkCount; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position;
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var speed = Random.Range(2.5f, 7.5f);
                piece.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                piece.angularVelocity = Random.Range(-450f, 450f);
                piece.lifetime = 0f;
                piece.maxLifetime = Random.Range(0.4f, 0.75f);
                piece.baseColor = isMechanical 
                    ? (Random.value > 0.4f ? new Color(0.95f, 0.55f, 0.15f) : new Color(0.45f, 0.5f, 0.6f))
                    : Color.Lerp(baseColor, new Color(0.2f, 0.18f, 0.15f), Random.Range(0f, 0.6f));
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = debrisSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.7f, 1.3f);
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }

            // 2. Fiery / glowing sparks
            var sparkCount = isMechanical ? 12 : 8;
            for (var i = 0; i < sparkCount; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.2f);
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var speed = Random.Range(4.5f, 11f);
                piece.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                piece.angularVelocity = 0f;
                piece.lifetime = 0f;
                piece.maxLifetime = Random.Range(0.25f, 0.45f);
                piece.baseColor = Random.value > 0.3f ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.45f, 0.1f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = sparkSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.6f, 1.1f);
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }

            // 3. Smoke puffs expanding outward
            for (var i = 0; i < 4; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.3f);
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var speed = Random.Range(0.8f, 2.2f);
                piece.velocity = new Vector2(Mathf.Cos(angle) - 0.8f, Mathf.Sin(angle)) * speed;
                piece.angularVelocity = Random.Range(-80f, 80f);
                piece.lifetime = 0f;
                piece.maxLifetime = Random.Range(0.5f, 0.85f);
                piece.baseColor = new Color(0.25f, 0.25f, 0.3f, 0.85f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = smokeSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.5f);
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }
        }

        private void DoImpact(Vector3 position, Vector2 normal, Color sparkColor)
        {
            var sparkCount = Random.Range(4, 7);
            for (var i = 0; i < sparkCount; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position;
                var spread = Random.Range(-0.8f, 0.8f);
                var dir = (normal + new Vector2(spread, spread)).normalized;
                var speed = Random.Range(3f, 8f);
                piece.velocity = dir * speed;
                piece.angularVelocity = 0f;
                piece.lifetime = 0f;
                piece.maxLifetime = Random.Range(0.15f, 0.3f);
                piece.baseColor = Color.Lerp(sparkColor, Color.white, Random.value * 0.6f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = sparkSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.5f, 0.9f);
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }
        }

        private void DoParry(Vector3 position)
        {
            // Cuphead-style vibrant pink/magenta spark ring burst
            var sparkCount = 18;
            for (var i = 0; i < sparkCount; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position;
                var angle = (float)i / sparkCount * Mathf.PI * 2f;
                var speed = Random.Range(6.5f, 12f);
                piece.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                piece.angularVelocity = 0f;
                piece.lifetime = 0f;
                piece.maxLifetime = Random.Range(0.35f, 0.55f);
                // Glowing magenta / hot pink with white center
                piece.baseColor = (i % 2 == 0) ? new Color(1f, 0.25f, 0.65f, 1f) : new Color(1f, 0.85f, 0.95f, 1f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = sparkSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(1.0f, 1.6f);
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }
        }

        private void DoMuzzleFlash(Vector3 position)
        {
            DoMuzzleFlashCustom(position, new Color(1f, 0.9f, 0.5f, 1f), 1.2f);
        }

        private void DoMuzzleFlashCustom(Vector3 position, Color flashColor, float scale)
        {
            var piece = GetDebrisPiece();
            piece.go.transform.position = position;
            piece.velocity = Vector2.zero;
            piece.angularVelocity = Random.Range(-360f, 360f);
            piece.lifetime = 0f;
            piece.maxLifetime = 0.06f;
            piece.baseColor = flashColor;
            piece.renderer.color = flashColor;
            piece.renderer.sprite = muzzleSprite;
            piece.go.transform.localScale = Vector3.one * (scale * Random.Range(0.9f, 1.2f));
            piece.go.SetActive(true);
            activeDebris.Add(piece);

            // Muzzle spark burst forward
            for (var i = 0; i < 2; i++)
            {
                var spark = GetDebrisPiece();
                spark.go.transform.position = position;
                var angle = Random.Range(-0.35f, 0.35f);
                var speed = Random.Range(4f, 8f);
                spark.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                spark.angularVelocity = 0f;
                spark.lifetime = 0f;
                spark.maxLifetime = Random.Range(0.08f, 0.16f);
                spark.baseColor = Color.Lerp(flashColor, Color.white, 0.6f);
                spark.renderer.color = spark.baseColor;
                spark.renderer.sprite = sparkSprite;
                spark.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.75f);
                spark.go.SetActive(true);
                activeDebris.Add(spark);
            }
        }

        private void DoWeaponImpact(Vector3 position, Color impactColor, float blastRadius, bool isAoE)
        {
            var sparkCount = isAoE ? 16 : Random.Range(4, 7);
            for (var i = 0; i < sparkCount; i++)
            {
                var piece = GetDebrisPiece();
                piece.go.transform.position = position;
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var speed = isAoE ? Random.Range(4f, 10f) : Random.Range(2.5f, 7f);
                piece.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                piece.angularVelocity = Random.Range(-300f, 300f);
                piece.lifetime = 0f;
                piece.maxLifetime = isAoE ? Random.Range(0.3f, 0.55f) : Random.Range(0.12f, 0.28f);
                piece.baseColor = Color.Lerp(impactColor, Color.white, Random.value * 0.45f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = isAoE ? (i % 2 == 0 ? smokeSprite : sparkSprite) : sparkSprite;
                piece.go.transform.localScale = Vector3.one * (isAoE ? Random.Range(0.8f, 1.5f) : Random.Range(0.4f, 0.85f));
                piece.go.SetActive(true);
                activeDebris.Add(piece);
            }
        }

        private void DoExhaustParticle(Vector3 position, Color particleColor, Vector2 velocity, string particleType)
        {
            var piece = GetDebrisPiece();
            piece.go.transform.position = position;
            piece.velocity = velocity;
            piece.angularVelocity = Random.Range(-120f, 120f);
            piece.lifetime = 0f;

            if (particleType == "steam")
            {
                piece.maxLifetime = Random.Range(0.28f, 0.42f);
                piece.baseColor = new Color(0.9f, 0.92f, 0.95f, 0.75f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = smokeSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.5f, 0.9f);
            }
            else
            {
                piece.maxLifetime = Random.Range(0.18f, 0.32f);
                piece.baseColor = Color.Lerp(particleColor, Color.white, Random.value * 0.35f);
                piece.renderer.color = piece.baseColor;
                piece.renderer.sprite = sparkSprite;
                piece.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.75f);
            }

            piece.go.SetActive(true);
            activeDebris.Add(piece);
        }

        private DebrisPiece GetDebrisPiece()
        {
            if (debrisPool.Count > 0)
                return debrisPool.Dequeue();

            return CreateDebrisObject();
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            for (var i = activeDebris.Count - 1; i >= 0; i--)
            {
                var piece = activeDebris[i];
                piece.lifetime += dt;

                if (piece.lifetime >= piece.maxLifetime)
                {
                    piece.go.SetActive(false);
                    activeDebris.RemoveAt(i);
                    debrisPool.Enqueue(piece);
                    continue;
                }

                // Physics simulation
                var progress = piece.lifetime / piece.maxLifetime;
                piece.velocity *= Mathf.Pow(0.85f, dt * 60f); // drag
                piece.velocity += Vector2.left * 1.5f * dt; // background inertia

                piece.go.transform.position += (Vector3)(piece.velocity * dt);
                if (piece.angularVelocity != 0f)
                    piece.go.transform.Rotate(0f, 0f, piece.angularVelocity * dt);

                // Fade out
                var alpha = 1f - progress * progress;
                var c = piece.baseColor;
                c.a = c.a * alpha;
                piece.renderer.color = c;
            }
        }
    }
}
