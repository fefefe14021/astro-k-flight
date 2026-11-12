// Assets/Scripts/Gameplay/Projectile.cs
// Namespace: KANYE.Gameplay
// GameObject: Projectile prefab or dynamically created bullet

using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private float hitRadius = 0.22f;
        [SerializeField] private Vector2 sizeWorldUnits = new Vector2(0.35f, 0.15f);
        [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 1f);

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
            EnsureVisible();
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
            lifetime -= Time.fixedDeltaTime;
            if (lifetime <= 0f || transform.position.x > 14f)
                Destroy(gameObject);
        }

        private void EnsureVisible()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            PlaceholderShipVisual.UseSpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 8;

            var art = Resources.Load<Sprite>("Sprites/bolt");
            if (art != null)
            {
                spriteRenderer.sprite = art;
                return;
            }

            if (spriteRenderer.sprite != null)
                return;

            var texture = new Texture2D(4, 2);
            var pixels = new Color[8];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            var pixelsPerUnit = 4f / sizeWorldUnits.x;
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 2f), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
