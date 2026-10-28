using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(ShipInput))]
    public class ShipShooter : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Vector2 fireDirection = Vector2.right;
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0.8f, 0f);
        [SerializeField] private float fireInterval = 0.16f;

        private ShipInput shipInput;
        private float cooldown;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
        }

        private void LateUpdate()
        {
            if (cooldown > 0f)
                cooldown -= Time.deltaTime;

            if (!shipInput.FireHeld || cooldown > 0f)
                return;

            Shoot();
            cooldown = fireInterval;
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
