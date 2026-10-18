using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
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
        private Vector2 movementInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; 
        }

        private void Update()
        {
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
            
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            rb.MovePosition(newPosition);
        }
    }
}