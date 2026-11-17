using UnityEngine;
using KANYE.Systems;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed = 2.8f;
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private float radius = 0.48f;
        [SerializeField] private float despawnX = -11f;
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private bool isHeavy;

        private int currentHealth;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D hitBox;
        private float hoverOffset;
        private float initialY;
        private Transform thrusterTransform;

        private float shootCooldown;
        private bool isTelegraphing;
        private float telegraphTimer;
        private Color originalColor = Color.white;

        public bool IsHeavy => isHeavy;

        private void Awake()
        {
            currentHealth = maxHealth;

            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            hitBox = GetComponent<CircleCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<CircleCollider2D>();

            hitBox.isTrigger = false;
            hitBox.radius = radius;
            EnsureVisible();
            CreateThruster();

            hoverOffset = Random.Range(0f, 10f);
            initialY = transform.position.y;
            shootCooldown = Random.Range(0.6f, 1.4f);
        }

        public void Configure(bool heavy, int hp, float moveSpeed)
        {
            isHeavy = heavy;
            maxHealth = hp;
            currentHealth = hp;
            speed = moveSpeed;
            radius = heavy ? 0.62f : 0.48f;
            albumsOnHit = heavy ? 2 : 1;

            if (hitBox != null)
                hitBox.radius = radius;

            EnsureVisible();

            if (thrusterTransform != null)
                thrusterTransform.localPosition = new Vector3(heavy ? 0.65f : 0.55f, 0f, 0f);
        }

        private void CreateThruster()
        {
            var thrusterObj = new GameObject("EnemyThruster");
            thrusterTransform = thrusterObj.transform;
            thrusterTransform.SetParent(transform, false);
            thrusterTransform.localPosition = new Vector3(isHeavy ? 0.65f : 0.55f, 0f, 0f);
            thrusterTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            var sr = thrusterObj.AddComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>("Sprites/thruster");
            if (sprite != null)
                sr.sprite = sprite;
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.color = isHeavy ? new Color(1f, 0.45f, 0.1f, 0.95f) : new Color(1f, 0.25f, 0.35f, 0.85f);
            sr.sortingOrder = 0;
        }

        private void FixedUpdate()
        {
            // Smooth fast entry dive until inside the visible combat zone
            var currentSpeed = body.position.x > 8.2f ? speed * 1.55f : speed;
            var bob = Mathf.Sin((Time.time + hoverOffset) * (isHeavy ? 2.5f : 4f)) * (isHeavy ? 0.35f : 0.25f);
            var targetPos = new Vector2(body.position.x - currentSpeed * Time.fixedDeltaTime, initialY + bob);
            body.MovePosition(targetPos);

            if (thrusterTransform != null)
            {
                var flicker = 0.9f + Mathf.Sin(Time.time * 30f) * 0.2f;
                var scale = isHeavy ? 1.25f : 0.95f;
                thrusterTransform.localScale = new Vector3(flicker * scale, flicker * scale * 0.75f, 1f);
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
            if (UI.TutorialHints.IsTutorialActive)
                return;

            if (transform.position.x > 8.3f || transform.position.x < -6.5f)
                return;

            if (isTelegraphing)
            {
                telegraphTimer -= Time.deltaTime;
                if (spriteRenderer != null)
                {
                    var flash = Mathf.PingPong(Time.time * 24f, 1f);
                    spriteRenderer.color = Color.Lerp(originalColor, new Color(1f, 0.9f, 0.2f), flash);
                }

                if (telegraphTimer <= 0f)
                {
                    isTelegraphing = false;
                    if (spriteRenderer != null)
                        spriteRenderer.color = originalColor;
                    FireAttack();
                }
                return;
            }

            shootCooldown -= Time.deltaTime;
            if (shootCooldown <= 0f)
            {
                shootCooldown = isHeavy ? Random.Range(1.8f, 2.8f) : Random.Range(1.5f, 2.5f);
                StartTelegraph();
            }
        }

        private void StartTelegraph()
        {
            isTelegraphing = true;
            telegraphTimer = isHeavy ? 0.45f : 0.35f;
            EffectsManager.PlayImpact(transform.position + new Vector3(-0.6f, 0f, 0f), Vector2.left, Color.yellow);
        }

        private void FireAttack()
        {
            var spawnPos = transform.position + new Vector3(isHeavy ? -0.75f : -0.6f, 0f, 0f);

            // Targeted aiming at player's ship!
            var player = FindAnyObjectByType<PlayerHealth>();
            Vector2 fireDir = Vector2.left;
            if (player != null && !player.IsDead)
            {
                var diff = (Vector2)player.transform.position - (Vector2)spawnPos;
                if (diff.x < -0.2f)
                {
                    fireDir = diff.normalized;
                    if (fireDir.x > -0.65f)
                        fireDir = new Vector2(-0.75f, Mathf.Sign(fireDir.y) * 0.5f).normalized;
                }
            }

            // Heavy gunship occasionally fires parry hazard if unlocked
            if (isHeavy && GameRules.IsParryUnlocked && Random.value < 0.40f)
            {
                var hazardObj = new GameObject("EnemyParryOrb", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
                hazardObj.transform.position = spawnPos;
                var hazard = hazardObj.AddComponent<ParryHazard>();
                hazard.Launch(fireDir, 4.4f);
                return;
            }

            // Targeted laser fire
            if (isHeavy)
            {
                SpawnLaser(spawnPos + Vector3.up * 0.16f, fireDir, 7.5f);
                SpawnLaser(spawnPos - Vector3.up * 0.16f, fireDir, 7.5f);
            }
            else
            {
                SpawnLaser(spawnPos, fireDir, 6.8f);
            }

            EffectsManager.PlayMuzzleFlash(spawnPos);
        }

        private static void SpawnLaser(Vector3 pos, Vector2 dir, float laserSpeed)
        {
            var laserObj = new GameObject("EnemyLaser", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            laserObj.transform.position = pos;
            var proj = laserObj.AddComponent<EnemyProjectile>();
            proj.Launch(dir, laserSpeed);
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
            // Off-screen protection: cannot be spawn-killed before entering visible combat arena
            if (transform.position.x > 8.5f)
                return;

            if (other.TryGetComponent<SuperProjectile>(out _))
                return;

            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            var damage = Mathf.Max(1, projectile.Damage);
            currentHealth -= damage;

            EffectsManager.PlayImpact(projectile.transform.position, Vector2.left, new Color(1f, 0.5f, 0.2f));
            projectile.HandleImpact(transform.position);

            if (currentHealth > 0)
            {
                // Damage flash & sound
                StartCoroutine(HitFlashRoutine());
                Audio.AudioManager.PlayHit();
                return;
            }

            // Enemy defeated
            EffectsManager.PlayExplosion(transform.position, isHeavy ? new Color(1f, 0.5f, 0.1f) : new Color(0.95f, 0.25f, 0.15f), isMechanical: true);
            CameraShake.Shake(isHeavy ? 0.3f : 0.2f, 0.25f);

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);

            // Chance to drop Nanite Repair Capsule (10% standard, 25% heavy)
            var dropChance = isHeavy ? 0.25f : 0.10f;
            if (Random.value < dropChance)
            {
                var drop = new GameObject("NaniteRepair", typeof(SpriteRenderer), typeof(CircleCollider2D));
                drop.transform.position = transform.position;
                drop.AddComponent<NaniteRepair>();
            }

            Destroy(gameObject);
        }

        private System.Collections.IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = new Color(1f, 0.9f, 0.9f, 1f);
            yield return new WaitForSeconds(0.06f);
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }

        private void EnsureVisible()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 1;

            var spritePath = isHeavy ? "Sprites/enemy_heavy" : "Sprites/enemy";
            var loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
                return;
            }

            spriteRenderer.sprite = MakeSprite();
        }

        private static Sprite MakeSprite()
        {
            const int width = 56;
            const int height = 38;
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[width * height];
            var body = new Color(0.2f, 0.24f, 0.3f, 1f);

            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = body;

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 48f);
        }
    }
}
