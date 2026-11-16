// Assets/Scripts/Gameplay/SuperProjectile.cs
// Namespace: KANYE.Gameplay
// GameObject: Dynamic Super Projectile

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SuperProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 18f;
        [SerializeField] private float lifetime = 1.8f;
        [SerializeField] private float hitRadius = 0.85f;
        [SerializeField] private Color beamColor = new Color(0.2f, 0.95f, 1f, 1f);

        private Rigidbody2D body;
        private Vector2 direction = Vector2.right;

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
            hitBox.radius = hitRadius;

            EnsureVisual();
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
            lifetime -= Time.fixedDeltaTime;

            // Pulsing beam size
            var pulse = 1f + 0.2f * Mathf.Sin(Time.time * 25f);
            transform.localScale = new Vector3(pulse * 1.8f, pulse * 1.5f, 1f);

            if (lifetime <= 0f || transform.position.x > 15f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Obliterate Asteroid
            if (other.TryGetComponent<Asteroid>(out var asteroid))
            {
                EffectsManager.PlayImpact(asteroid.transform.position, Vector2.right, Color.yellow);
                EffectsManager.PlayExplosion(asteroid.transform.position, Color.yellow, isMechanical: false);
                CameraShake.Shake(0.35f, 0.25f);
                GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                Destroy(asteroid.gameObject);
                return;
            }

            // Obliterate Enemy
            if (other.TryGetComponent<Enemy>(out var enemy))
            {
                EffectsManager.PlayImpact(enemy.transform.position, Vector2.right, Color.red);
                EffectsManager.PlayExplosion(enemy.transform.position, new Color(1f, 0.35f, 0.15f), isMechanical: true);
                CameraShake.Shake(0.35f, 0.25f);
                GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                Destroy(enemy.gameObject);
                return;
            }

            // Obliterate Missile
            if (other.TryGetComponent<Missile>(out var missile))
            {
                EffectsManager.PlayExplosion(missile.transform.position, Color.yellow, isMechanical: true);
                CameraShake.Shake(0.25f, 0.2f);
                GameRules.NotifyMissileDestroyed();
                Destroy(missile.gameObject);
                return;
            }

            // Obliterate Enemy Projectiles
            if (other.TryGetComponent<EnemyProjectile>(out var ep))
            {
                Destroy(ep.gameObject);
            }
        }

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.sortingOrder = 15;

            var art = Resources.Load<Sprite>("Sprites/bolt");
            if (art != null)
            {
                sr.sprite = art;
                sr.color = beamColor;
                return;
            }

            // Fallback super bolt texture
            var tex = new Texture2D(12, 8);
            var px = new Color[96];
            for (var i = 0; i < px.Length; i++) px[i] = beamColor;
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 12, 8), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
