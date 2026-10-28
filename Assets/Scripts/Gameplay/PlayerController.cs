using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ShipInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Настройки скорости")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Границы экрана (чтобы корабль не улетал)")]
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;
        [SerializeField] private float minY = -4f;
        [SerializeField] private float maxY = 4f;

        private Rigidbody2D rb;
        private ShipInput shipInput;
        private PlayerHealth health;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            shipInput = GetComponent<ShipInput>();
            if (GetComponent<PlayerHealth>() == null)
                gameObject.AddComponent<PlayerHealth>();
            health = GetComponent<PlayerHealth>();
            rb.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            if (health != null && health.IsDead)
                return;

            Vector2 newPosition = rb.position + shipInput.Move * moveSpeed * Time.fixedDeltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            rb.MovePosition(newPosition);
        }
    }
}
