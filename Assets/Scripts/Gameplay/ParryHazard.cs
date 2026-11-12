// Assets/Scripts/Gameplay/ParryHazard.cs
// Namespace: KANYE.Gameplay
// Cuphead-style pink parryable hazard/projectile.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ParryHazard : MonoBehaviour
    {
        [SerializeField] private float speed = 4.5f;
        [SerializeField] private Vector2 direction = Vector2.left;
        [SerializeField] private float hitRadius = 0.35f;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D hitBox;
        private Rigidbody2D body;
        private float initialY;
        private float hoverSeed;

        public void Launch(Vector2 moveDirection, float moveSpeed = 4.5f)
        {
            if (moveDirection.sqrMagnitude > 0.0001f)
                direction = moveDirection.normalized;

            speed = moveSpeed;
        }

        private void Awake()
        {
            hoverSeed = Random.Range(0f, 10f);
            initialY = transform.position.y;

            body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            hitBox = GetComponent<CircleCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<CircleCollider2D>();

            hitBox.isTrigger = true;
            hitBox.radius = hitRadius;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 9;

            var art = Resources.Load<Sprite>("Sprites/parry_orb");
            if (art != null)
                spriteRenderer.sprite = art;
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            var bob = Mathf.Sin((Time.time + hoverSeed) * 5f) * 0.15f;
            var nextPos = body.position + direction * speed * Time.fixedDeltaTime;
            nextPos.y = initialY + bob;
            body.MovePosition(nextPos);

            // Pulsing glowing pink effect
            var pulse = 0.95f + Mathf.Sin(Time.time * 16f) * 0.2f;
            transform.localScale = new Vector3(pulse, pulse, 1f);

            if (transform.position.x < -13f || transform.position.x > 15f || Mathf.Abs(transform.position.y) > 8f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player collision
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                if (health.IsDead) return;

                if (GameRules.IsParryUnlocked)
                {
                    ExecuteParry(health);
                }
                else
                {
                    health.TakeDamage(1);
                    Destroy(gameObject);
                }
                return;
            }

            // Super beam automatically destroys and converts
            if (other.TryGetComponent<SuperProjectile>(out _))
            {
                EffectsManager.PlayParry(transform.position);
                GameRules.NotifyParry();
                Destroy(gameObject);
            }
        }

        public void ExecuteParry(PlayerHealth health)
        {
            // 1. Visceral Cuphead feedback
            EffectsManager.PlayParry(transform.position);
            CameraShake.Shake(0.35f, 0.25f);
            CameraShake.HitStop(0.08f); // 80ms freeze frame

            // 2. Grant Hype meter (+30 Hype!)
            var hype = Object.FindAnyObjectByType<HypeState>();
            hype?.AddHype(30f);

            // 3. Register stats & brief shield
            GameRules.NotifyParry();
            health?.GrantParryInvulnerability(0.75f);

            // 4. Destroy hazard
            Destroy(gameObject);
        }
    }
}
