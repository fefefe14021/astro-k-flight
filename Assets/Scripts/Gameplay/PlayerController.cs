// Assets/Scripts/Gameplay/PlayerController.cs
// Namespace: KANYE.Gameplay
// Tight, responsive arcade flight controls.

using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ShipInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Flight Dynamics")]
        [SerializeField] private float moveSpeed = 6.5f;
        [SerializeField] private float acceleration = 28f;

        [Header("Screen Boundaries")]
        [SerializeField] private float minX = -8.2f;
        [SerializeField] private float maxX = 8.2f;
        [SerializeField] private float minY = -4.3f;
        [SerializeField] private float maxY = 4.3f;

        private Rigidbody2D rb;
        private ShipInput shipInput;
        private PlayerHealth health;
        private Vector2 currentVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            shipInput = GetComponent<ShipInput>();

            if (GetComponent<PlayerHealth>() == null)
                gameObject.AddComponent<PlayerHealth>();

            health = GetComponent<PlayerHealth>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            if (health != null && health.IsDead)
                return;

            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
            {
                currentVelocity = Vector2.zero;
                return;
            }

            var targetVelocity = (shipInput != null ? shipInput.Move : Vector2.zero) * moveSpeed;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            var nextPos = rb.position + currentVelocity * Time.fixedDeltaTime;
            nextPos.x = Mathf.Clamp(nextPos.x, minX, maxX);
            nextPos.y = Mathf.Clamp(nextPos.y, minY, maxY);

            rb.MovePosition(nextPos);
        }
    }
}
