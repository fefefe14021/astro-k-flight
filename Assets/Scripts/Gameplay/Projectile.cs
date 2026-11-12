// Assets/Scripts/Gameplay/Projectile.cs
// Namespace: KANYE.Gameplay
// GameObject: Projectile prefab or dynamically created bullet

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

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Vector2 direction = Vector2.right;
        private int piercesRemaining;
        private int bouncesRemaining;
        private Transform homingTarget;
        private string customSpriteName = "Sprites/bolt";

        public void Launch(Vector2 newDirection)
        {
            if (newDirection.sqrMagnitude > 0.0001f)
                direction = newDirection.normalized;

            UpdateVisualAngle();
        }

        public void Configure(WeaponDefinition def, Vector2 launchDirection)
        {
            if (def == null) return;

            damage = def.Damage;
            color = def.ThemeColor;
            customSpriteName = def.ProjectileSprite;

            switch (def.Type)
            {
                case WeaponType.ScatterShotgun:
                    speed = 15f;
                    lifetime = 0.65f;
                    hitRadius = 0.20f;
                    break;
                case WeaponType.HomingMissiles:
                    speed = 11.5f;
                    lifetime = 2.8f;
                    hitRadius = 0.25f;
                    IsHoming = true;
                    FindHomingTarget();
                    break;
                case WeaponType.HeavyRailgun:
                    speed = 26f;
                    lifetime = 1.2f;
                    hitRadius = 0.35f;
                    IsPiercing = true;
                    piercesRemaining = 3;
                    break;
                case WeaponType.SawDisc:
                    speed = 9.5f;
                    lifetime = 3.2f;
                    hitRadius = 0.32f;
                    IsBouncing = true;
                    bouncesRemaining = 3;
                    break;
                case WeaponType.PlasmaMortar:
                    speed = 8.5f;
                    lifetime = 2.0f;
                    hitRadius = 0.40f;
                    IsExplosive = true;
                    break;
                default:
                    speed = 14f;
                    lifetime = 2.0f;
                    hitRadius = 0.22f;
                    break;
            }

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

            // Homing trajectory steering
            if (IsHoming)
            {
                if (homingTarget == null || !homingTarget.gameObject.activeInHierarchy)
                    FindHomingTarget();

                if (homingTarget != null)
                {
                    var desiredDir = ((Vector2)homingTarget.position - body.position).normalized;
                    direction = Vector2.MoveTowards(direction, desiredDir, 7.5f * Time.fixedDeltaTime).normalized;
                    UpdateVisualAngle();
                }
            }

            // Bouncing trajectory off screen top/bottom borders
            if (IsBouncing && bouncesRemaining > 0)
            {
                if ((body.position.y > 4.1f && direction.y > 0f) || (body.position.y < -4.1f && direction.y < 0f))
                {
                    direction.y = -direction.y;
                    bouncesRemaining--;
                    EffectsManager.PlayImpact(body.position, direction, Color.yellow);
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
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void FindHomingTarget()
        {
            // First check for active Boss
            var boss = BossController.ActiveBoss;
            if (boss != null && !boss.IsDead)
            {
                homingTarget = boss.transform;
                return;
            }

            // Otherwise check closest enemy
            var enemies = FindObjectsByType<Enemy>();
            float closestDist = float.MaxValue;
            Transform best = null;
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].transform.position.x < transform.position.x - 0.5f)
                    continue; // Skip enemies far behind
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
                // Area of Effect detonation!
                EffectsManager.PlayExplosion(impactPos, color, isMechanical: true);
                CameraShake.Shake(0.35f, 0.25f);

                // Damage surrounding enemies
                var enemies = FindObjectsByType<Enemy>();
                for (var i = 0; i < enemies.Length; i++)
                {
                    if (Vector2.Distance(impactPos, enemies[i].transform.position) < 2.0f)
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
                EffectsManager.PlayImpact(impactPos, direction, Color.white);
                return;
            }

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
