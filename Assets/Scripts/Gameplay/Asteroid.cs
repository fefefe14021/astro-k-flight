using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Asteroid : MonoBehaviour
    {
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private float radius = 0.55f;
        [SerializeField] private Vector2 sizeWorldUnits = new Vector2(1.2f, 1.2f);
        [SerializeField] private Color color = new Color(0.75f, 0.72f, 0.66f, 1f);

        private void OnEnable()
        {
            EnsureBody();
            EnsureVisible();
        }

        private void Awake()
        {
            EnsureBody();
            EnsureVisible();
        }

        private void EnsureBody()
        {
            if (!Application.isPlaying)
                return;

            var body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var hitBox = GetComponent<CircleCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<CircleCollider2D>();

            hitBox.isTrigger = false;
            hitBox.radius = radius;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Application.isPlaying)
                return;

            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);

            Destroy(projectile.gameObject);
            Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite != null)
                return;

            var texture = new Texture2D(2, 2);
            var pixels = new Color[4];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            var pixelsPerUnit = 2f / sizeWorldUnits.x;
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
