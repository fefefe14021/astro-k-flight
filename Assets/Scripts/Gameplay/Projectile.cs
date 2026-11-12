// Assets/Scripts/Gameplay/Projectile.cs
// Namespace: KANYE.Gameplay
// GameObject: Projectile prefab or dynamically created bullet with distinct per-weapon particle effects.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 13.5f;
        [SerializeField] private float lifetime = 2.4f;
        [SerializeField] private float hitRadius = 0.22f;
        [SerializeField] private int damage = 1;
        [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 1f);

        public int Damage => damage;
        public bool IsPiercing { get; private set; }
        public bool IsBouncing { get; private set; }
        public bool IsHoming { get; private set; }
        public bool IsExplosive { get; private set; }
        public WeaponType WeaponType { get; private set; }

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Vector2 direction = Vector2.right;
        private int piercesRemaining;
        private int bouncesRemaining;
        private Transform homingTarget;
        private string customSpriteName = "Sprites/bolt";
        private float trailTimer;

        public void Launch(Vector2 newDirection)
        {
            if (newDirection.sqrMagnitude > 0.0001f)
                direction = newDirection.normalized;

            UpdateVisualAngle();
        }

        public void Configure(WeaponDefinition def, Vector2 launchDirection, int customDamage = -1, int level = 1)
        {
            if (def == null) return;

            damage = customDamage > 0 ? customDamage : def.Damage;
            color = def.ThemeColor;
            customSpriteName = def.ProjectileSprite;
            WeaponType = def.Type;

            switch (def.Type)
            {
                case WeaponType.ScatterShotgun:
                    speed = 15.5f + level * 1.0f;
                    lifetime = 0.7f;
                    hitRadius = 0.22f;
                    break;
                case WeaponType.HomingMissiles:
                    speed = 12f + level * 1.5f;
                    lifetime = 2.8f;
                    hitRadius = 0.26f;
                    IsHoming = true;
                    FindHomingTarget();
                    break;
                case WeaponType.HeavyRailgun:
                    speed = 28f + level * 2.0f;
                    lifetime = 1.2f;
                    hitRadius = 0.38f + level * 0.05f;
                    IsPiercing = true;
                    piercesRemaining = 4 + level * 2;
                    break;
                case WeaponType.SawDisc:
                    speed = 10f + level * 0.8f;
                    lifetime = 3.5f;
                    hitRadius = 0.34f;
                    IsBouncing = true;
                    bouncesRemaining = 3 + level;
                    break;
                case WeaponType.PlasmaMortar:
                    speed = 9f + level * 0.6f;
                    lifetime = 2.2f;
                    hitRadius = 0.44f + level * 0.08f;
                    IsExplosive = true;
                    break;
                default:
                    speed = 14.5f + level * 1.0f;
                    lifetime = 2.0f;
                    hitRadius = 0.22f;
                    break;
            }

            transform.localScale = Vector3.one * (1f + (level - 1) * 0.16f);
            Launch(launchDirection);
            EnsureVisible();
        }

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

            hitBox.isTrigger = true;
            hitBox.radius = hitRadius;

            EnsureVisible();
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            // 1. Spinning Buzzsaw Disc
            if (WeaponType == WeaponType.SawDisc)
            {
                transform.Rotate(0f, 0f, -850f * Time.fixedDeltaTime);
            }

            // 2. Homing trajectory steering & missile smoke trail
            if (IsHoming)
            {
                if (homingTarget == null || !homingTarget.gameObject.activeInHierarchy)
                    FindHomingTarget();

                if (homingTarget != null)
                {
                    var desiredDir = ((Vector2)homingTarget.position - body.position).normalized;
                    direction = Vector2.MoveTowards(direction, desiredDir, 8.5f * Time.fixedDeltaTime).normalized;
                    UpdateVisualAngle();
                }

                trailTimer += Time.fixedDeltaTime;
                if (trailTimer >= 0.045f)
                {
                    trailTimer = 0f;
                    var trailPos = body.position - direction * 0.22f;
                    EffectsManager.EmitExhaustParticle(trailPos, new Color(1f, 0.5f, 0.2f), -direction * 2f, "steam");
                }
            }
            else if (IsExplosive)
            {
                // Plasma mortar unstable energy particles
                trailTimer += Time.fixedDeltaTime;
                if (trailTimer >= 0.05f)
                {
                    trailTimer = 0f;
                    EffectsManager.EmitExhaustParticle(body.position, color, Vector2.zero, "bio");
                }
            }
            else if (IsPiercing)
            {
                // Railgun electric spark trail
                trailTimer += Time.fixedDeltaTime;
                if (trailTimer >= 0.035f)
                {
                    trailTimer = 0f;
                    EffectsManager.EmitExhaustParticle(body.position, Color.Lerp(color, Color.white, 0.5f), Vector2.zero, "ion");
                }
            }

            // 3. Bouncing trajectory off screen top/bottom borders
            if (IsBouncing && bouncesRemaining > 0)
            {
                if ((body.position.y > 4.1f && direction.y > 0f) || (body.position.y < -4.1f && direction.y < 0f))
                {
                    direction.y = -direction.y;
                    bouncesRemaining--;
                    EffectsManager.PlayWeaponImpact(body.position, Color.yellow, 0.4f, isAoE: false);
                    UpdateVisualAngle();
                }
            }

            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
            lifetime -= Time.fixedDeltaTime;

            if (lifetime <= 0f || body.position.x > 15f || body.position.x < -14f || Mathf.Abs(body.position.y) > 7f)
                Destroy(gameObject);
        }

        private void UpdateVisualAngle()
        {
            if (WeaponType == WeaponType.SawDisc)
                return; // Let it spin freely on Z

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void FindHomingTarget()
        {
            var boss = BossController.ActiveBoss;
            if (boss != null && !boss.IsDead)
            {
                homingTarget = boss.transform;
                return;
            }

            var enemies = FindObjectsByType<Enemy>();
            float closestDist = float.MaxValue;
            Transform best = null;
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].transform.position.x < transform.position.x - 0.5f)
                    continue;
                var d = Vector2.Distance(transform.position, enemies[i].transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    best = enemies[i].transform;
                }
            }
            homingTarget = best;
        }

        public void HandleImpact(Vector3 impactPos)
        {
            if (IsExplosive)
            {
                // Massive AoE plasma detonation!
                EffectsManager.PlayWeaponImpact(impactPos, color, blastRadius: 2.2f, isAoE: true);
                CameraShake.Shake(0.38f, 0.28f);

                var enemies = FindObjectsByType<Enemy>();
                for (var i = 0; i < enemies.Length; i++)
                {
                    if (Vector2.Distance(impactPos, enemies[i].transform.position) < 2.2f)
                    {
                        GameRules.NotifyAsteroidDestroyed(1);
                        Destroy(enemies[i].gameObject);
                    }
                }
                Destroy(gameObject);
                return;
            }

            if (IsPiercing && piercesRemaining > 0)
            {
                piercesRemaining--;
                EffectsManager.PlayWeaponImpact(impactPos, color, blastRadius: 0.5f, isAoE: false);
                return;
            }

            EffectsManager.PlayWeaponImpact(impactPos, color, blastRadius: 0.4f, isAoE: false);
            Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 8;

            var art = Resources.Load<Sprite>(customSpriteName);
            if (art != null)
            {
                spriteRenderer.sprite = art;
                spriteRenderer.color = Color.white;
                return;
            }

            var fallback = Resources.Load<Sprite>("Sprites/bolt");
            if (fallback != null)
            {
                spriteRenderer.sprite = fallback;
                spriteRenderer.color = color;
            }
        }
    }
}
