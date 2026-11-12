using KANYE.Systems;
using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Asteroid : MonoBehaviour
    {
        [SerializeField] private int albumsOnHit = 1;
        [SerializeField] private int damage = 1;
        [SerializeField] private float moveSpeed = 2.5f;
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

        private Rigidbody2D body;
        private float spinSpeed;

        private void Start()
        {
            spinSpeed = Random.Range(-55f, 55f);
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying || body == null)
                return;

            body.MovePosition(body.position + Vector2.left * moveSpeed * Time.fixedDeltaTime);
            transform.Rotate(0f, 0f, spinSpeed * Time.fixedDeltaTime);

            if (body.position.x < -12f)
            {
                if (TutorialHints.IsTutorialActive)
                    TutorialHints.NotifyAsteroidMissed();
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!Application.isPlaying)
                return;

            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            if (TutorialHints.IsTutorialActive)
            {
                EffectsManager.PlayImpact(transform.position, Vector2.left, Color.yellow);
                CameraShake.Shake(0.2f, 0.15f);
                TutorialHints.NotifyAsteroidCollided();
                Destroy(gameObject);
                return;
            }

            EffectsManager.PlayExplosion(transform.position, color, isMechanical: false);
            CameraShake.Shake(0.35f, 0.35f);
            health.TakeDamage(damage);
            Destroy(gameObject);
        }

        public void Configure(float speed, int contactDamage, float hitRadius, int albums = 1)
        {
            moveSpeed = speed;
            damage = contactDamage;
            radius = hitRadius;
            albumsOnHit = albums;
            var hitBox = GetComponent<CircleCollider2D>();
            if (hitBox != null)
                hitBox.radius = radius;
        }

        private void EnsureBody()
        {
            if (!Application.isPlaying)
                return;

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
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Application.isPlaying)
                return;

            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            EffectsManager.PlayImpact(projectile.transform.position, Vector2.left, new Color(1f, 0.85f, 0.3f));
            EffectsManager.PlayExplosion(transform.position, color, isMechanical: false);
            CameraShake.Shake(0.18f, 0.2f);

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);
            TutorialHints.NotifyAsteroidDestroyed();

            projectile.HandleImpact(transform.position);

            if (albumsOnHit >= 2 && Random.value < 0.12f)
            {
                var drop = new GameObject("NaniteRepair", typeof(SpriteRenderer), typeof(CircleCollider2D));
                drop.transform.position = transform.position;
                drop.AddComponent<NaniteRepair>();
            }

            Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);

            var art = Resources.Load<Sprite>("Sprites/asteroid");
            if (art != null)
            {
                spriteRenderer.sprite = art;
                return;
            }

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
