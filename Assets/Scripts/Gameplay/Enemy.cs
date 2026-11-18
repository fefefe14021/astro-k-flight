using System.Collections;
using UnityEngine;
using KANYE.Systems;
using KANYE.UI;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour
    {
        public enum AIState
        {
            Entering,       // Spawns off-screen and dives quickly into visible combat arena
            CombatPatrol,   // Holds combat position on-screen, strafes vertically, aims & fires
            AttackDive,     // Aggressive dive run towards player's flank
            LoopReEngage    // Loops up/down and curves back to right flank instead of vanishing
        }

        [SerializeField] private float speed = 2.8f;
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private float radius = 0.48f;
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private bool isHeavy;

        private int currentHealth;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D hitBox;
        private Transform thrusterTransform;
        private SpriteRenderer thrusterRenderer;

        // Combat Positioning & AI
        private AIState state = AIState.Entering;
        private float targetCombatX;
        private float initialY;
        private float patrolTargetY;
        private float patrolRetargetTimer;
        private float diveCooldown;
        private float loopAngle;
        private int loopDirection = 1; // 1 = loop top, -1 = loop bottom

        // Weapon Telegraphing & Firing
        private float shootCooldown;
        private bool isTelegraphing;
        private float telegraphTimer;
        private Color originalColor = Color.white;

        // Heavy Enemy Health Bar & Damage Feedback
        private Transform healthBarRoot;
        private SpriteRenderer healthBarFill;
        private float smokeTimer;

        public bool IsHeavy => isHeavy;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private bool isDying;
        public bool IsDead => isDying || currentHealth <= 0 || (gameObject == null) || !gameObject.activeInHierarchy;

        public static int ActiveEnemyCount
        {
            get
            {
                var enemies = FindObjectsByType<Enemy>();
                var count = 0;
                for (var i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] != null && !enemies[i].IsDead)
                        count++;
                }
                return count;
            }
        }

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

            initialY = transform.position.y;
            patrolTargetY = initialY;
            targetCombatX = isHeavy ? Random.Range(5.4f, 7.2f) : Random.Range(3.2f, 6.2f);
            shootCooldown = Random.Range(0.8f, 1.8f);
            diveCooldown = Random.Range(5.0f, 9.0f);
        }

        public void Configure(bool heavy, int hp, float moveSpeed)
        {
            isHeavy = heavy;
            maxHealth = hp;
            currentHealth = hp;
            speed = moveSpeed;
            radius = heavy ? 0.62f : 0.48f;
            albumsOnHit = heavy ? 2 : 1;

            targetCombatX = heavy ? Random.Range(5.4f, 7.2f) : Random.Range(3.2f, 6.2f);

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

            thrusterRenderer = thrusterObj.AddComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>("Sprites/thruster");
            if (sprite != null)
                thrusterRenderer.sprite = sprite;
            PlaceholderShipVisual.UseSpriteMaterial(thrusterRenderer);
            thrusterRenderer.color = isHeavy ? new Color(1f, 0.45f, 0.1f, 0.95f) : new Color(1f, 0.25f, 0.35f, 0.85f);
            thrusterRenderer.sortingOrder = 0;
        }

        private void FixedUpdate()
        {
            if (MissionController.IsMissionFinished)
            {
                // When mission finishes, fly away with warp boost
                body.MovePosition(body.position + Vector2.left * 14f * Time.fixedDeltaTime);
                if (body.position.x < -14f)
                    Destroy(gameObject);
                return;
            }

            switch (state)
            {
                case AIState.Entering:
                    UpdateEnteringState();
                    break;

                case AIState.CombatPatrol:
                    UpdateCombatPatrolState();
                    break;

                case AIState.AttackDive:
                    UpdateAttackDiveState();
                    break;

                case AIState.LoopReEngage:
                    UpdateLoopReEngageState();
                    break;
            }

            // Animate thruster flame
            if (thrusterTransform != null)
            {
                var flicker = 0.9f + Mathf.Sin(Time.time * 32f) * 0.2f;
                var scale = isHeavy ? 1.25f : 0.95f;
                if (state == AIState.LoopReEngage || state == AIState.AttackDive)
                    scale *= 1.4f;
                thrusterTransform.localScale = new Vector3(flicker * scale, flicker * scale * 0.75f, 1f);
            }

            // Failsafe cleanup only if astronomically far out of game bounds
            if (Mathf.Abs(body.position.y) > 20f || body.position.x < -30f)
            {
                isDying = true;
                Destroy(gameObject);
            }
        }

        private void UpdateEnteringState()
        {
            // Swift approach into visible combat arena
            var currentPos = body.position;
            var step = speed * 1.6f * Time.fixedDeltaTime;
            var newX = Mathf.MoveTowards(currentPos.x, targetCombatX, step);
            var bob = Mathf.Sin(Time.time * 3.5f) * 0.2f;

            body.MovePosition(new Vector2(newX, initialY + bob));

            if (currentPos.x <= targetCombatX + 0.1f)
            {
                state = AIState.CombatPatrol;
                patrolTargetY = Random.Range(-3.0f, 3.0f);
            }
        }

        private void UpdateCombatPatrolState()
        {
            var currentPos = body.position;

            // Retarget altitude periodically to engage player's flight path
            patrolRetargetTimer -= Time.fixedDeltaTime;
            if (patrolRetargetTimer <= 0f)
            {
                patrolRetargetTimer = Random.Range(1.8f, 3.2f);
                var player = FindAnyObjectByType<PlayerHealth>();
                if (player != null && !player.IsDead && Random.value < 0.65f)
                {
                    patrolTargetY = Mathf.Clamp(player.transform.position.y + Random.Range(-0.8f, 0.8f), -3.6f, 3.6f);
                }
                else
                {
                    patrolTargetY = Random.Range(-3.4f, 3.4f);
                }
            }

            // Smooth patrol on Y and subtle hover oscillation on X
            var targetX = targetCombatX + Mathf.Sin(Time.time * 1.8f) * (isHeavy ? 0.45f : 0.75f);
            var newX = Mathf.MoveTowards(currentPos.x, targetX, speed * 0.65f * Time.fixedDeltaTime);
            var newY = Mathf.MoveTowards(currentPos.y, patrolTargetY, (speed * 0.85f) * Time.fixedDeltaTime);

            body.MovePosition(new Vector2(newX, newY));

            // Fast scouts periodically launch an aggressive forward attack run
            if (!isHeavy)
            {
                diveCooldown -= Time.fixedDeltaTime;
                if (diveCooldown <= 0f)
                {
                    diveCooldown = Random.Range(6.0f, 10.0f);
                    if (Random.value < 0.45f)
                    {
                        state = AIState.AttackDive;
                        EffectsManager.PlayImpact(transform.position, Vector2.right, Color.yellow);
                    }
                }
            }
        }

        private void UpdateAttackDiveState()
        {
            var currentPos = body.position;
            var diveSpeed = speed * 1.75f;
            var targetPos = currentPos + Vector2.left * diveSpeed * Time.fixedDeltaTime;
            body.MovePosition(targetPos);

            // Once the drone passes the player towards left border, begin combat loop re-engagement!
            if (currentPos.x <= -7.8f)
            {
                state = AIState.LoopReEngage;
                loopDirection = currentPos.y >= 0f ? 1 : -1;
                loopAngle = 0f;
            }
        }

        private void UpdateLoopReEngageState()
        {
            // Performs an evasive high-speed curve up/down around screen boundaries and returns to right flank
            loopAngle += 150f * Time.fixedDeltaTime;
            var rad = loopAngle * Mathf.Deg2Rad;

            var currentPos = body.position;
            var nextY = currentPos.y + (loopDirection * 4.8f * Time.fixedDeltaTime);
            nextY = Mathf.Clamp(nextY, -4.3f, 4.3f);

            // Zoom rightwards back into deployment space
            var nextX = currentPos.x + (speed * 2.2f * Time.fixedDeltaTime);
            body.MovePosition(new Vector2(nextX, nextY));

            if (nextX >= 9.2f)
            {
                // Re-aligned on right wing! Dive back into combat
                initialY = Mathf.Clamp(nextY, -3.2f, 3.2f);
                targetCombatX = Random.Range(3.5f, 6.5f);
                state = AIState.Entering;
            }
        }

        private void Update()
        {
            HandleCombatAI();

            if (isHeavy && currentHealth > 0 && currentHealth <= maxHealth * 0.35f && !isDying)
            {
                smokeTimer += Time.deltaTime;
                if (smokeTimer >= 0.12f)
                {
                    smokeTimer = 0f;
                    var offset = (Vector3)Random.insideUnitCircle * 0.35f;
                    EffectsManager.EmitExhaustParticle(transform.position + offset, new Color(0.35f, 0.35f, 0.4f, 0.85f), Vector2.up * 1.4f, "steam");
                }
            }
        }

        private void HandleCombatAI()
        {
            if (UI.TutorialHints.IsTutorialActive)
                return;

            // Only fire when active inside visible combat boundaries
            if (transform.position.x > 8.2f || transform.position.x < -6.5f)
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
                shootCooldown = isHeavy ? Random.Range(2.0f, 3.0f) : Random.Range(1.6f, 2.6f);
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

            // Heavy gunship occasionally fires Cuphead rhythmic parry orb
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
            Audio.AudioManager.PlayEnemyLaser();
        }

        private void BuildHealthBar()
        {
            if (!isHeavy || healthBarRoot != null) return;

            var rootObj = new GameObject("HealthBar");
            healthBarRoot = rootObj.transform;
            healthBarRoot.SetParent(transform, false);
            healthBarRoot.localPosition = new Vector3(0f, 0.68f, 0f);

            var bgObj = new GameObject("Bg", typeof(SpriteRenderer));
            bgObj.transform.SetParent(healthBarRoot, false);
            var bgSr = bgObj.GetComponent<SpriteRenderer>();
            bgSr.sprite = GetPixelSprite();
            bgSr.color = new Color(0.06f, 0.08f, 0.12f, 0.90f);
            bgSr.transform.localScale = new Vector3(1.15f, 0.12f, 1f);
            bgSr.sortingOrder = 20;

            var fillObj = new GameObject("Fill", typeof(SpriteRenderer));
            fillObj.transform.SetParent(healthBarRoot, false);
            healthBarFill = fillObj.GetComponent<SpriteRenderer>();
            healthBarFill.sprite = GetPixelSprite();
            healthBarFill.color = new Color(0.25f, 0.95f, 0.4f, 0.95f);
            fillObj.transform.localPosition = new Vector3(-0.55f, 0f, 0f);
            fillObj.transform.localScale = new Vector3(1.10f, 0.08f, 1f);
            healthBarFill.sortingOrder = 21;

            healthBarRoot.gameObject.SetActive(false);
        }

        private static Sprite cachedPixelSprite;
        private static Sprite GetPixelSprite()
        {
            if (cachedPixelSprite != null) return cachedPixelSprite;
            var tex = new Texture2D(2, 2);
            var px = new Color[] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            cachedPixelSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0f, 0.5f), 2f);
            return cachedPixelSprite;
        }

        public void TakeDamage(int damage, Vector2 hitPos = default)
        {
            if (currentHealth <= 0)
                return;

            damage = Mathf.Max(1, damage);
            currentHealth -= damage;

            if (hitPos == default)
                hitPos = transform.position;

            FloatingCombatText.ShowDamage(hitPos, damage, damage >= 10);
            EffectsManager.PlayImpact(hitPos, Vector2.left, new Color(1f, 0.7f, 0.2f));

            if (isHeavy)
            {
                if (healthBarRoot == null)
                    BuildHealthBar();

                if (healthBarRoot != null)
                {
                    healthBarRoot.gameObject.SetActive(true);
                    var hpRatio = Mathf.Clamp01(currentHealth / (float)maxHealth);
                    if (healthBarFill != null)
                    {
                        healthBarFill.transform.localScale = new Vector3(1.10f * hpRatio, 0.08f, 1f);
                        if (hpRatio > 0.5f)
                            healthBarFill.color = new Color(0.25f, 0.95f, 0.4f);
                        else if (hpRatio > 0.25f)
                            healthBarFill.color = new Color(1f, 0.85f, 0.2f);
                        else
                            healthBarFill.color = new Color(1f, 0.25f, 0.25f);
                    }
                }
            }

            if (currentHealth > 0)
            {
                StartCoroutine(HitFlashRoutine());
                Audio.AudioManager.PlayHit();
                return;
            }

            if (isDying)
                return;
            isDying = true;

            // Defeated! Spectacular mechanical explosion
            var coinsReward = isHeavy ? 50 : 20;
            SaveManager.Instance?.AddCoins(coinsReward);
            FloatingCombatText.ShowCoins(transform.position, coinsReward);
            EffectsManager.PlayExplosion(transform.position, isHeavy ? new Color(1f, 0.45f, 0.1f) : new Color(0.95f, 0.25f, 0.15f), isMechanical: true);
            CameraShake.Shake(isHeavy ? 0.35f : 0.22f, 0.25f);
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            if (isDying)
                return;
            isDying = true;

            EffectsManager.PlayExplosion(transform.position, new Color(0.9f, 0.2f, 0.15f), isMechanical: true);
            CameraShake.Shake(0.35f, 0.35f);

            health.TakeDamage(1);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Off-screen protection
            if (transform.position.x > 8.8f)
                return;

            if (other.TryGetComponent<SuperProjectile>(out _))
                return; // SuperProjectile laser beam ticks damage directly via TakeDamage

            if (other.TryGetComponent<Projectile>(out var projectile))
            {
                TakeDamage(projectile.Damage, projectile.transform.position);
                projectile.HandleImpact(transform.position);
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = new Color(1f, 0.95f, 0.95f, 1f);
            yield return new WaitForSeconds(0.08f);
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
