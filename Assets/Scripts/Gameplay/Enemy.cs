using UnityEngine;
using KANYE.Systems;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed = 2.8f;
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private float radius = 0.4f;
        [SerializeField] private float despawnX = -11f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private float hoverOffset;
        private float initialY;
        private Transform thrusterTransform;

        private float shootCooldown;
        private bool isTelegraphing;
        private float telegraphTimer;
        private Color originalColor = Color.white;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var hitBox = GetComponent<CircleCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<CircleCollider2D>();

            hitBox.isTrigger = false;
            hitBox.radius = radius;
            EnsureVisible();
            CreateThruster();

            hoverOffset = Random.Range(0f, 10f);
            initialY = transform.position.y;
            shootCooldown = Random.Range(1.8f, 3.2f);
        }

        private void CreateThruster()
        {
            var thrusterObj = new GameObject("EnemyThruster");
            thrusterTransform = thrusterObj.transform;
            thrusterTransform.SetParent(transform, false);
            thrusterTransform.localPosition = new Vector3(0.5f, 0f, 0f); // Rear of enemy (flying left)
            thrusterTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            var sr = thrusterObj.AddComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>("Sprites/thruster");
            if (sprite != null)
                sr.sprite = sprite;
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.color = new Color(1f, 0.35f, 0.2f, 0.85f);
            sr.sortingOrder = 0;
        }

        private void FixedUpdate()
        {
            // Hover bobbing motion
            var bob = Mathf.Sin((Time.time + hoverOffset) * 4f) * 0.25f;
            var targetPos = new Vector2(body.position.x - speed * Time.fixedDeltaTime, initialY + bob);
            body.MovePosition(targetPos);

            if (thrusterTransform != null)
            {
                var flicker = 0.9f + Mathf.Sin(Time.time * 30f) * 0.2f;
                thrusterTransform.localScale = new Vector3(flicker * 0.8f, flicker * 0.6f, 1f);
            }

            if (body.position.x < despawnX)
                Destroy(gameObject);
        }

        private void Update()
        {
            HandleCombatAI();
        }

        private void HandleCombatAI()
        {
            // Don't shoot during tutorial or when off-screen
            if (UI.TutorialHints.IsTutorialActive)
                return;

            if (transform.position.x > 8.5f || transform.position.x < -6.5f)
                return;

            if (isTelegraphing)
            {
                telegraphTimer -= Time.deltaTime;
                // Telegraph flash (Cuphead / retro shmup attack telegraph)
                if (spriteRenderer != null)
                {
                    var flash = Mathf.PingPong(Time.time * 20f, 1f);
                    spriteRenderer.color = Color.Lerp(originalColor, new Color(1f, 0.9f, 0.2f), flash);
                }

                if (telegraphTimer <= 0f)
                {
                    isTelegraphing = false;
                    if (spriteRenderer != null)
                        spriteRenderer.color = originalColor;
                    FireLaser();
                }
                return;
            }

            shootCooldown -= Time.deltaTime;
            if (shootCooldown <= 0f)
            {
                shootCooldown = Random.Range(2.5f, 4.0f);
                StartTelegraph();
            }
        }

        private void StartTelegraph()
        {
            isTelegraphing = true;
            telegraphTimer = 0.42f; // 420ms attack warning
            EffectsManager.PlayImpact(transform.position + new Vector3(-0.55f, 0f, 0f), Vector2.left, Color.yellow);
        }

        private void FireLaser()
        {
            var spawnPos = transform.position + new Vector3(-0.6f, 0f, 0f);
            var laserObj = new GameObject("EnemyLaser", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            laserObj.transform.position = spawnPos;
            var proj = laserObj.AddComponent<EnemyProjectile>();
            proj.Launch(Vector2.left, 6.2f);

            EffectsManager.PlayMuzzleFlash(spawnPos);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            EffectsManager.PlayExplosion(transform.position, new Color(0.9f, 0.2f, 0.15f), isMechanical: true);
            CameraShake.Shake(0.35f, 0.35f);

            health.TakeDamage(1);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<SuperProjectile>(out _))
                return; // SuperProjectile handles its own destruction

            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            EffectsManager.PlayImpact(projectile.transform.position, Vector2.left, new Color(1f, 0.4f, 0.2f));
            EffectsManager.PlayExplosion(transform.position, new Color(0.95f, 0.25f, 0.15f), isMechanical: true);
            CameraShake.Shake(0.2f, 0.22f);

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);

            Destroy(projectile.gameObject);
            Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 1;

            var loadedSprite = Resources.Load<Sprite>("Sprites/enemy");
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
                return;
            }

            spriteRenderer.sprite = MakeSprite();
        }

        private static Sprite MakeSprite()
        {
            const int width = 16;
            const int height = 10;
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[width * height];
            var body = new Color(0.85f, 0.18f, 0.22f, 1f);
            var dark = new Color(0.35f, 0.05f, 0.08f, 1f);
            var window = new Color(1f, 0.85f, 0.45f, 1f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var inHull = y >= 3 && y <= 6 && x >= 1 && x <= 13;
                    var inDome = y >= 5 && y <= 8 && x >= 5 && x <= 10;
                    if (!inHull && !inDome)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    var edge = y == 3 || y == 6 || x == 1 || x == 13 || y == 8;
                    var cockpit = y >= 6 && y <= 7 && x >= 6 && x <= 8;
                    pixels[y * width + x] = cockpit ? window : edge ? dark : body;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
