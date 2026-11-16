// Assets/Scripts/Gameplay/EXMissile.cs
// Namespace: KANYE.Gameplay
// Broforce/Cuphead EX Heavy Rocket.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EXMissile : MonoBehaviour
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private float maxSpeed = 22f;
        [SerializeField] private float acceleration = 25f;
        [SerializeField] private float blastRadius = 1.6f;
        [SerializeField] private int damage = 4;

        private Rigidbody2D body;
        private Vector2 direction = Vector2.right;
        private float trailTimer;

        public void Launch(Vector2 newDirection)
        {
            if (newDirection.sqrMagnitude > 0.0001f)
                direction = newDirection.normalized;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
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

        private void FixedUpdate()
        {
            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.fixedDeltaTime);
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);

            trailTimer += Time.fixedDeltaTime;
            if (trailTimer >= 0.04f)
            {
                trailTimer = 0f;
                var exhaustPos = transform.position - (Vector3)(direction * 0.5f);
                EffectsManager.PlayImpact(exhaustPos, -direction, new Color(1f, 0.6f, 0.2f));
            }

            if (transform.position.x > 15f || Mathf.Abs(transform.position.y) > 8f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Do not hit player
            if (other.GetComponent<PlayerHealth>() != null)
                return;

            Detonate();
        }

        private void Detonate()
        {
            var pos = transform.position;
            EffectsManager.PlayExplosion(pos, new Color(1f, 0.4f, 0.1f), isMechanical: true);
            CameraShake.Shake(0.42f, 0.35f);
            CameraShake.HitStop(0.04f);

            // Blast radius damage
            var hits = Physics2D.OverlapCircleAll(pos, blastRadius);
            for (var i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h.TryGetComponent<Asteroid>(out var asteroid))
                {
                    GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                    Destroy(asteroid.gameObject);
                }
                else if (h.TryGetComponent<Enemy>(out var enemy))
                {
                    GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                    Destroy(enemy.gameObject);
                }
                else if (h.TryGetComponent<Missile>(out var missile))
                {
                    GameRules.NotifyMissileDestroyed();
                    Destroy(missile.gameObject);
                }
                else if (h.TryGetComponent<BossController>(out var boss))
                {
                    boss.TakeDamage(damage);
                }
                else if (h.TryGetComponent<EnemyProjectile>(out var ep))
                {
                    Destroy(ep.gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
