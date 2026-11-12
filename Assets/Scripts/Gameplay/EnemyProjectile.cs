// Assets/Scripts/Gameplay/EnemyProjectile.cs
// Namespace: KANYE.Gameplay
// GameObject: Dynamic Enemy Projectile

using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 6.5f;
        [SerializeField] private float hitRadius = 0.22f;
        [SerializeField] private Color color = new Color(1f, 0.25f, 0.15f, 1f);

        private Rigidbody2D body;
        private Vector2 direction = Vector2.left;

        public void Launch(Vector2 newDirection, float projectileSpeed = 6.5f)
        {
            if (newDirection.sqrMagnitude > 0.0001f)
                direction = newDirection.normalized;

            speed = projectileSpeed;
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

            if (transform.position.x < -12f || transform.position.x > 14f || Mathf.Abs(transform.position.y) > 7f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Collided with player
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                EffectsManager.PlayImpact(transform.position, direction, color);
                CameraShake.Shake(0.35f, 0.25f);
                playerHealth.TakeDamage(1);
                Destroy(gameObject);
                return;
            }

            // Can be intercepted by player's projectiles
            if (other.TryGetComponent<Projectile>(out var projectile))
            {
                EffectsManager.PlayImpact(transform.position, Vector2.up, Color.yellow);
                Destroy(projectile.gameObject);
                Destroy(gameObject);
                return;
            }

            if (other.TryGetComponent<SuperProjectile>(out _))
            {
                EffectsManager.PlayImpact(transform.position, Vector2.right, Color.cyan);
                Destroy(gameObject);
            }
        }

        private void EnsureVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.sortingOrder = 8;

            var art = Resources.Load<Sprite>("Sprites/bolt");
            if (art != null)
            {
                sr.sprite = art;
                sr.color = color;
                return;
            }

            // Fallback plasma bolt
            var tex = new Texture2D(6, 4);
            var px = new Color[24];
            for (var i = 0; i < px.Length; i++) px[i] = color;
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 6, 4), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
