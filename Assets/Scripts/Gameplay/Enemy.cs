using UnityEngine;
using KANYE.Systems;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed = 2.8f;
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private float radius = 0.4f;
        [SerializeField] private float despawnX = -11f;

        private Rigidbody2D body;

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

            hitBox.isTrigger = false;
            hitBox.radius = radius;
            EnsureVisible();
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + Vector2.left * speed * Time.fixedDeltaTime);
            if (body.position.x < despawnX)
                Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            health.TakeDamage(1);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);

            Destroy(projectile.gameObject);
            Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 1;
            spriteRenderer.sprite = MakeSprite();
        }

        private static Sprite MakeSprite()
        {
            const int width = 16;
            const int height = 10;
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[width * height];
            var body = new Color(0.85f, 0.18f, 0.22f, 1f);
            var dark = new Color(0.35f, 0.05f, 0.08f, 1f);
            var window = new Color(1f, 0.85f, 0.45f, 1f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var inHull = y >= 3 && y <= 6 && x >= 1 && x <= 13;
                    var inDome = y >= 5 && y <= 8 && x >= 5 && x <= 10;
                    if (!inHull && !inDome)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    var edge = y == 3 || y == 6 || x == 1 || x == 13 || y == 8;
                    var cockpit = y >= 6 && y <= 7 && x >= 6 && x <= 8;
                    pixels[y * width + x] = cockpit ? window : edge ? dark : body;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
