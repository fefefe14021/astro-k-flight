// Assets/Scripts/Gameplay/NaniteRepair.cs
// Namespace: KANYE.Gameplay
// Collectible nanite repair capsule that restores 1 life and gives clutch recovery.

using KANYE.Audio;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class NaniteRepair : MonoBehaviour
    {
        [SerializeField] private float speed = 1.4f;
        [SerializeField] private float magnetRange = 3.8f;
        [SerializeField] private float magnetSpeed = 6.5f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Transform playerTransform;
        private float hoverSeed;
        private float initialY;

        private void Awake()
        {
            hoverSeed = Random.Range(0f, 10f);
            initialY = transform.position.y;

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

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 9;

            var sprite = Resources.Load<Sprite>("Sprites/nanite_repair");
            if (sprite != null)
                spriteRenderer.sprite = sprite;

            var player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
            {
                var ph = FindAnyObjectByType<PlayerHealth>();
                if (ph != null) playerTransform = ph.transform;
            }
        }

        private void Update()
        {
            // Floating pulse
            var scale = 1f + Mathf.Sin((Time.time + hoverSeed) * 6f) * 0.15f;
            transform.localScale = new Vector3(scale, scale, 1f);

            var effectiveRange = magnetRange + (GameRules.CurrentMode == GameMode.RogueExpedition ? RogueExpeditionController.MagnetRangeBonus : 0f);
            if (playerTransform != null)
            {
                var distSq = (playerTransform.position - transform.position).sqrMagnitude;
                if (distSq < effectiveRange * effectiveRange)
                {
                    // Magnetic pull towards player
                    var dir = (playerTransform.position - transform.position).normalized;
                    transform.position += dir * (magnetSpeed * Time.deltaTime);
                    return;
                }
            }

            // Normal drift left with bobbing
            var bob = Mathf.Sin((Time.time + hoverSeed) * 3f) * 0.2f;
            transform.position = new Vector3(transform.position.x - speed * Time.deltaTime, initialY + bob, 0f);

            if (transform.position.x < -11f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerHealth>(out var health) && !other.GetComponentInParent<PlayerHealth>())
                return;

            if (health == null)
                health = other.GetComponentInParent<PlayerHealth>();

            if (health != null && !health.IsDead)
            {
                var healed = health.Heal(1);
                // Also give 5 bonus coins for catching the nanite
                SaveManager.Instance?.AddCoins(5);
                EffectsManager.PlayImpact(transform.position, Vector2.up, Color.green);
                AudioManager.PlayPowerup();
                Destroy(gameObject);
            }
        }
    }
}
