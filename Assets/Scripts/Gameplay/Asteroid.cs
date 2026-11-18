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
        [SerializeField] private int maxHealth = 3;

        private int currentHealth = 3;
        private Vector2 moveDirection = Vector2.left;

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

            body.MovePosition(body.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
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

        public void Configure(float speed, int contactDamage, float hitRadius, int albums = 1, int hp = 0)
        {
            moveSpeed = speed;
            damage = contactDamage;
            radius = hitRadius;
            albumsOnHit = albums;

            if (hp > 0)
            {
                maxHealth = hp;
                currentHealth = hp;
            }
            else
            {
                maxHealth = albums >= 2 ? 7 : 3;
                currentHealth = maxHealth;
            }

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

            // Off-screen protection
            if (transform.position.x > 8.5f)
                return;

            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            var dmg = Mathf.Max(1, projectile.Damage);
            currentHealth -= dmg;
            FloatingCombatText.ShowDamage(transform.position, dmg, false);

            EffectsManager.PlayImpact(projectile.transform.position, Vector2.left, new Color(1f, 0.85f, 0.3f));
            projectile.HandleImpact(transform.position);

            if (currentHealth > 0)
            {
                StartCoroutine(HitFlashRoutine());
                Audio.AudioManager.PlayHit();
                return;
            }

            // Asteroid fully shattered
            EffectsManager.PlayExplosion(transform.position, color, isMechanical: false);
            CameraShake.Shake(albumsOnHit >= 2 ? 0.28f : 0.16f, 0.2f);

            GameRules.NotifyAsteroidDestroyed(albumsOnHit);
            TutorialHints.NotifyAsteroidDestroyed();

            // Large asteroid splits into 2 small angular fragments!
            if (albumsOnHit >= 2)
            {
                SpawnFragment(transform.position + Vector3.up * 0.25f, new Vector2(-1f, 0.32f).normalized, moveSpeed * 1.25f);
                SpawnFragment(transform.position - Vector3.up * 0.25f, new Vector2(-1f, -0.32f).normalized, moveSpeed * 1.25f);

                if (Random.value < 0.18f)
                {
                    var drop = new GameObject("NaniteRepair", typeof(SpriteRenderer), typeof(CircleCollider2D));
                    drop.transform.position = transform.position;
                    drop.AddComponent<NaniteRepair>();
                }
            }

            Destroy(gameObject);
        }

        private static void SpawnFragment(Vector3 pos, Vector2 dir, float spd)
        {
            var fragObj = new GameObject("AsteroidFragment", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            fragObj.transform.position = pos;
            var ast = fragObj.AddComponent<Asteroid>();
            ast.moveDirection = dir;
            ast.Configure(spd, 1, 0.42f, albums: 1, hp: 2);
            fragObj.transform.localScale = Vector3.one * 0.65f;
        }

        private System.Collections.IEnumerator HitFlashRoutine()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            var orig = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.color = orig;
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
