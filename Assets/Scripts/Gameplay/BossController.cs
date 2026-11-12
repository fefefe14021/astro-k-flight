// Assets/Scripts/Gameplay/BossController.cs
// Namespace: KANYE.Gameplay
// Cuphead & Broforce style multi-phase sector bosses.

using System;
using System.Collections;
using KANYE.Audio;
using KANYE.Systems;
using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossController : MonoBehaviour
    {
        public static BossController ActiveBoss { get; private set; }

        public string BossName { get; private set; } = "VANGUARD DREAD-DRONE";
        public int MaxHealth { get; private set; } = 45;
        public int CurrentHealth { get; private set; } = 45;
        public bool IsEnraged => CurrentHealth <= MaxHealth * 0.4f;
        public bool IsDead { get; private set; }

        public event Action<int, int> HealthChanged;
        public event Action Defeated;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private BoxCollider2D hitBox;
        private Color normalColor = Color.white;

        private float targetX = 6.2f;
        private float hoverSeed;
        private float attackTimer = 1.4f;
        private int attackCycle;
        private bool isEntering = true;

        public static BossController SpawnBoss(int sectorIndex)
        {
            var go = new GameObject("Boss", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
            go.transform.position = new Vector3(14f, 0f, 0f);
            var boss = go.AddComponent<BossController>();
            boss.Configure(sectorIndex);
            return boss;
        }

        public void Configure(int sectorIndex)
        {
            string spriteName;
            switch (sectorIndex)
            {
                case 0:
                    BossName = "VANGUARD DREAD-DRONE";
                    MaxHealth = 45;
                    spriteName = "boss_drone";
                    break;
                case 1:
                    BossName = "LEVIATHAN TORPEDO CRUISER";
                    MaxHealth = 65;
                    spriteName = "boss_cruiser";
                    break;
                case 2:
                    BossName = "CRYO-TITAN DREADNOUGHT";
                    MaxHealth = 90;
                    spriteName = "boss_dreadnought";
                    break;
                case 3:
                    BossName = "VOID REAPER";
                    MaxHealth = 120;
                    spriteName = "boss_reaper";
                    break;
                default:
                    BossName = "FORTRESS PRIME";
                    MaxHealth = 160;
                    spriteName = "boss_fortress";
                    break;
            }

            CurrentHealth = MaxHealth;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            var art = Resources.Load<Sprite>("Sprites/" + spriteName);
            if (art != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = art;
                if (hitBox != null)
                {
                    hitBox.size = new Vector2(art.bounds.size.x * 0.85f, art.bounds.size.y * 0.85f);
                }
            }

            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Awake()
        {
            ActiveBoss = this;
            hoverSeed = UnityEngine.Random.Range(0f, 10f);

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 10;

            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            hitBox = GetComponent<BoxCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<BoxCollider2D>();
            hitBox.isTrigger = true;
            hitBox.size = new Vector2(2.5f, 1.8f);
        }

        private void Start()
        {
            AudioManager.PlayBossWarning();
            CameraShake.Shake(0.5f, 0.6f);
        }

        private void OnDestroy()
        {
            if (ActiveBoss == this)
                ActiveBoss = null;
        }

        private void FixedUpdate()
        {
            if (IsDead || body == null) return;

            // Entrance glide
            if (isEntering)
            {
                var curX = Mathf.MoveTowards(transform.position.x, targetX, 5.5f * Time.fixedDeltaTime);
                transform.position = new Vector3(curX, 0f, 0f);
                if (Mathf.Abs(curX - targetX) < 0.05f)
                {
                    isEntering = false;
                }
                return;
            }

            // Hover motion (bobbing up and down)
            var bobRange = IsEnraged ? 2.8f : 2.2f;
            var bobSpeed = IsEnraged ? 2.5f : 1.8f;
            var targetY = Mathf.Sin((Time.time + hoverSeed) * bobSpeed) * bobRange;
            var targetPos = new Vector2(targetX, targetY);
            body.MovePosition(Vector2.Lerp(body.position, targetPos, 4f * Time.fixedDeltaTime));
        }

        private void Update()
        {
            if (IsDead || isEntering) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                ExecuteNextAttack();
                var interval = IsEnraged ? UnityEngine.Random.Range(1.2f, 1.8f) : UnityEngine.Random.Range(1.8f, 2.8f);
                attackTimer = interval;
            }
        }

        public void ExecuteNextAttack()
        {
            attackCycle = (attackCycle + 1) % 4;

            switch (attackCycle)
            {
                case 0:
                    // Twin blasters targeted at player
                    StartCoroutine(TwinBlasterRoutine());
                    break;
                case 1:
                    // Cuphead Parry Pattern: Ring of Pink Energy Orbs!
                    SpawnParryVolley();
                    break;
                case 2:
                    // Heavy missile / torpedo salvo
                    SpawnTorpedoSalvo();
                    break;
                case 3:
                    // Spread laser fan
                    SpawnSpreadBlaster();
                    break;
            }
        }

        private IEnumerator TwinBlasterRoutine()
        {
            for (var i = 0; i < 3; i++)
            {
                if (IsDead) yield break;

                var player = FindAnyObjectByType<PlayerHealth>();
                var targetDir = player != null 
                    ? ((Vector2)player.transform.position - (Vector2)transform.position).normalized 
                    : Vector2.left;

                FireEnemyLaser(transform.position + new Vector3(-1.2f, 0.45f, 0f), targetDir);
                FireEnemyLaser(transform.position + new Vector3(-1.2f, -0.45f, 0f), targetDir);

                yield return new WaitForSeconds(0.18f);
            }
        }

        private void SpawnParryVolley()
        {
            // Fires 3-4 glowing pink parryable orbs towards the player!
            var count = IsEnraged ? 4 : 3;
            for (var i = 0; i < count; i++)
            {
                var yOffset = Mathf.Lerp(-1.8f, 1.8f, (float)i / (count - 1));
                var pos = transform.position + new Vector3(-1.1f, yOffset, 0f);

                var orbObj = new GameObject("BossParryOrb", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
                orbObj.transform.position = pos;
                var hazard = orbObj.AddComponent<ParryHazard>();
                var dir = new Vector2(-1f, UnityEngine.Random.Range(-0.25f, 0.25f)).normalized;
                hazard.Launch(dir, 3.8f);
            }
        }

        private void SpawnTorpedoSalvo()
        {
            // Spawns missiles plunging from above
            for (var i = 0; i < (IsEnraged ? 3 : 2); i++)
            {
                var missileObj = new GameObject("BossMissile", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
                var x = UnityEngine.Random.Range(-5f, 4f);
                missileObj.transform.position = new Vector3(x, 6.2f, 0f);
                var m = missileObj.AddComponent<Missile>();
                m.Configure(4.2f, 1);
            }
        }

        private void SpawnSpreadBlaster()
        {
            float[] angles = { -20f, -10f, 0f, 10f, 20f };
            for (var i = 0; i < angles.Length; i++)
            {
                var rad = (180f + angles[i]) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                FireEnemyLaser(transform.position + new Vector3(-1.1f, 0f, 0f), dir);
            }
        }

        private void FireEnemyLaser(Vector3 spawnPos, Vector2 direction)
        {
            var laserObj = new GameObject("BossLaser", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            laserObj.transform.position = spawnPos;
            var proj = laserObj.AddComponent<EnemyProjectile>();
            proj.Launch(direction, 6.8f);
            EffectsManager.PlayMuzzleFlash(spawnPos);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            // Cuphead boss hit flash
            StartCoroutine(HitFlashRoutine());
            EffectsManager.PlayImpact(transform.position, Vector2.right, Color.yellow);
            CameraShake.Shake(0.18f, 0.15f);

            if (CurrentHealth <= 0)
            {
                StartCoroutine(DefeatSequenceRoutine());
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = Color.white * 2f; // Overbright flash
            yield return new WaitForSeconds(0.06f);
            if (spriteRenderer != null && !IsDead)
            {
                spriteRenderer.color = IsEnraged ? new Color(1f, 0.5f, 0.4f) : normalColor;
            }
        }

        private IEnumerator DefeatSequenceRoutine()
        {
            IsDead = true;
            ActiveBoss = null;

            CameraShake.HitStop(0.12f);
            CameraShake.Shake(0.6f, 2.2f);
            AudioManager.PlayExplosion(true);

            // Broforce style explosion cascade across the boss hull
            HypeHud.Instance?.ShowBanner("A KNOCKOUT!", "БОСС ПОВЕРЖЕН!", 2.2f, new Color(1f, 0.85f, 0.2f));

            var elapsed = 0f;
            while (elapsed < 2.0f)
            {
                elapsed += 0.14f;
                var offset = (Vector3)(UnityEngine.Random.insideUnitCircle * 1.5f);
                EffectsManager.PlayExplosion(transform.position + offset, new Color(1f, 0.5f, 0.1f), isMechanical: true);
                yield return new WaitForSeconds(0.14f);
            }

            // Final catastrophic explosion
            EffectsManager.PlayExplosion(transform.position, Color.yellow, isMechanical: true);
            CameraShake.Shake(0.7f, 0.5f);

            Defeated?.Invoke();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsDead) return;

            // Damage player upon collision
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(1);
                CameraShake.Shake(0.35f, 0.25f);
                return;
            }

            if (other.TryGetComponent<Projectile>(out var p))
            {
                TakeDamage(p.Damage);
                p.HandleImpact(transform.position);
            }
            else if (other.TryGetComponent<SuperProjectile>(out _))
            {
                TakeDamage(12); // Huge chunk
            }
        }
    }
}
