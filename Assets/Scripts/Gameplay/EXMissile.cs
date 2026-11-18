// Assets/Scripts/Gameplay/EXMissile.cs
// Namespace: KANYE.Gameplay
// High-impact Armor-Piercing EX Rocket with Soft Homing and Shockwave Blast.

using KANYE.Audio;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EXMissile : MonoBehaviour
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private float maxSpeed = 24f;
        [SerializeField] private float acceleration = 26f;
        [SerializeField] private float blastRadius = 2.2f;
        [SerializeField] private int directDamage = 20;

        private Rigidbody2D body;
        private Vector2 direction = Vector2.right;
        private Transform homingTarget;
        private float trailTimer;
        private float homingScanTimer;

        public void Launch(Vector2 newDirection)
        {
            if (newDirection.sqrMagnitude > 0.0001f)
                direction = newDirection.normalized;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            FindInitialTarget();
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
            hitBox.radius = 0.45f;

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.sortingOrder = 12;

            var art = Resources.Load<Sprite>("Sprites/ex_missile");
            if (art != null)
                sr.sprite = art;
        }

        private void FindInitialTarget()
        {
            // Scan for closest enemy or boss ahead of the rocket
            var boss = BossController.ActiveBoss;
            if (boss != null && !boss.IsDead)
            {
                homingTarget = boss.transform;
                return;
            }

            var enemies = FindObjectsByType<Enemy>();
            var bestDist = float.MaxValue;
            Transform best = null;

            for (var i = 0; i < enemies.Length; i++)
            {
                var e = enemies[i];
                if (e == null || e.transform.position.x < transform.position.x - 0.5f)
                    continue;

                var toTarget = (Vector2)e.transform.position - (Vector2)transform.position;
                if (Vector2.Dot(toTarget.normalized, direction) < 0.2f)
                    continue;

                var dist = toTarget.sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = e.transform;
                }
            }

            homingTarget = best;
        }

        private void FixedUpdate()
        {
            // Smoothly steer towards target if available
            homingScanTimer -= Time.fixedDeltaTime;
            if (homingScanTimer <= 0f)
            {
                homingScanTimer = 0.15f;
                if (homingTarget == null)
                    FindInitialTarget();
            }

            if (homingTarget != null)
            {
                var toTarget = ((Vector2)homingTarget.position - body.position).normalized;
                // Soft homing angular turn rate (approx 190 deg/sec)
                direction = Vector3.RotateTowards(direction, toTarget, 3.4f * Time.fixedDeltaTime, 0f);
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.fixedDeltaTime);
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);

            // Fiery exhaust trail
            trailTimer += Time.fixedDeltaTime;
            if (trailTimer >= 0.03f)
            {
                trailTimer = 0f;
                var exhaustPos = transform.position - (Vector3)(direction * 0.5f);
                EffectsManager.PlayImpact(exhaustPos, -direction, new Color(1f, 0.55f, 0.15f));
            }

            if (transform.position.x > 16f || Mathf.Abs(transform.position.y) > 9f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore player and friendly projectiles
            if (other.GetComponent<PlayerHealth>() != null ||
                other.GetComponentInParent<PlayerHealth>() != null ||
                other.GetComponent<PlayerController>() != null ||
                other.GetComponent<Projectile>() != null ||
                other.GetComponent<EXMissile>() != null ||
                other.GetComponent<SuperProjectile>() != null)
            {
                return;
            }

            // Only detonate on actual hostile targets
            bool isHostile = other.GetComponent<Enemy>() != null ||
                             other.GetComponent<BossController>() != null ||
                             other.GetComponent<Asteroid>() != null ||
                             other.GetComponent<Missile>() != null ||
                             other.GetComponent<EnemyProjectile>() != null ||
                             other.GetComponent<ParryHazard>() != null;

            if (!isHostile)
                return;

            Detonate();
        }

        private void Detonate()
        {
            var pos = transform.position;

            // Big visceral explosion
            EffectsManager.PlayExplosion(pos, new Color(1f, 0.45f, 0.1f), isMechanical: true);
            EffectsManager.PlayImpact(pos, Vector2.up, new Color(1f, 0.85f, 0.25f));
            CameraShake.Shake(0.45f, 0.35f);
            CameraShake.HitStop(0.05f);
            AudioManager.PlayExplosion(large: true);

            // Shockwave blast radius damage
            var hits = Physics2D.OverlapCircleAll(pos, blastRadius);
            for (var i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;

                var dist = Vector2.Distance(pos, h.transform.position);
                var falloff = Mathf.Clamp01(1f - (dist / blastRadius));
                var dmg = Mathf.Max(10, Mathf.RoundToInt(directDamage * Mathf.Lerp(0.6f, 1f, falloff)));

                if (h.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(dmg, pos);
                }
                else if (h.TryGetComponent<Asteroid>(out var asteroid))
                {
                    GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                    Destroy(asteroid.gameObject);
                }
                else if (h.TryGetComponent<BossController>(out var boss))
                {
                    boss.TakeDamage(directDamage);
                }
                else if (h.TryGetComponent<Missile>(out var missile))
                {
                    GameRules.NotifyMissileDestroyed();
                    Destroy(missile.gameObject);
                }
                else if (h.TryGetComponent<EnemyProjectile>(out var ep))
                {
                    Destroy(ep.gameObject);
                }
                else if (h.TryGetComponent<ParryHazard>(out var parry))
                {
                    Destroy(parry.gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
