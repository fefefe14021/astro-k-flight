using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(ShipInput))]
    public class ShipShooter : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Vector2 fireDirection = Vector2.right;
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0.8f, 0f);

        private ShipInput shipInput;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
        }

        private void OnEnable()
        {
            shipInput.FirePressed += Shoot;
        }

        private void OnDisable()
        {
            if (shipInput != null)
                shipInput.FirePressed -= Shoot;
        }

        private void Shoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("ShipShooter: не назначен префаб снаряда.", this);
                return;
            }

            var direction = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
            var origin = (Vector2)transform.position + muzzleOffset;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction);
        }
    }
}
