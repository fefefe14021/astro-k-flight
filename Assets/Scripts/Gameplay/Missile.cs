// Assets/Scripts/Gameplay/Missile.cs
// Namespace: KANYE.Gameplay
// Falling aerial hazard with auto-configuring sprite & trail.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Missile : MonoBehaviour
    {
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D body;
        private Transform trailTransform;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            var loaded = Resources.Load<Sprite>("Sprites/missile");
            if (loaded != null)
            {
                spriteRenderer.sprite = loaded;
            }
            else if (spriteRenderer.sprite == null)
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

            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 2;

            CreateTrail();

            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var hitBox = GetComponent<BoxCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<BoxCollider2D>();
            hitBox.isTrigger = false;
            hitBox.size = new Vector2(0.35f, 0.9f);
        }

        private void CreateTrail()
        {
            var trailObj = new GameObject("MissileTrail", typeof(SpriteRenderer));
            trailTransform = trailObj.transform;
            trailTransform.SetParent(transform, false);
            trailTransform.localPosition = new Vector3(0f, 0.55f, 0f); // Top tail of falling missile
            trailTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            var sr = trailObj.GetComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>("Sprites/thruster");
            if (sprite != null)
                sr.sprite = sprite;
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.color = new Color(1f, 0.5f, 0.1f, 0.9f);
            sr.sortingOrder = 1;
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

            if (trailTransform != null)
            {
                var flicker = 0.8f + Mathf.Sin(Time.time * 35f) * 0.25f;
                trailTransform.localScale = new Vector3(flicker * 0.9f, flicker * 0.6f, 1f);
            }

            if (body.position.y < -8f)
                Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent<PlayerHealth>(out var health))
                return;

            EffectsManager.PlayExplosion(transform.position, new Color(1f, 0.4f, 0.15f), isMechanical: true);
            CameraShake.Shake(0.35f, 0.35f);

            health.TakeDamage(damage);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Projectile>(out var projectile))
                return;

            EffectsManager.PlayImpact(projectile.transform.position, Vector2.up, Color.yellow);
            EffectsManager.PlayExplosion(transform.position, new Color(1f, 0.45f, 0.1f), isMechanical: true);
            CameraShake.Shake(0.2f, 0.2f);

            GameRules.NotifyMissileDestroyed();
            Destroy(projectile.gameObject);
            Destroy(gameObject);
        }
    }
}
