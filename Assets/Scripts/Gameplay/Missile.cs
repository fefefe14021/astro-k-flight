using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class Missile : MonoBehaviour
    {
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D body;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null)
            {
                var texture = new Texture2D(1, 3);
                var pixels = new Color[3];
                for (var i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color(1f, 0.25f, 0.2f, 1f);
                texture.SetPixels(pixels);
                texture.filterMode = FilterMode.Point;
                texture.Apply();
                spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 3f), new Vector2(0.5f, 0.5f), 4f);
            }

            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var hitBox = GetComponent<BoxCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<BoxCollider2D>();
            hitBox.isTrigger = false;
            hitBox.size = new Vector2(0.28f, 0.8f);
        }

        public void Configure(float fallSpeed, int contactDamage)
        {
            speed = fallSpeed;
            damage = contactDamage;
        }

        private void FixedUpdate()
        {
            if (body == null)
                return;

            body.MovePosition(body.position + Vector2.down * speed * Time.fixedDeltaTime);
            if (body.position.y < -8f)
                Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            health.TakeDamage(damage);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            GameRules.NotifyMissileDestroyed();
            Destroy(projectile.gameObject);
            Destroy(gameObject);
        }
    }
}
