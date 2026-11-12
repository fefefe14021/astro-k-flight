// Assets/Scripts/Gameplay/ShipShooter.cs
// Namespace: KANYE.Gameplay
// Manages primary fire, EX special rockets, and Super Art mega beam.

using KANYE.Audio;
using KANYE.Systems;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KANYE.Gameplay
{
    public class ShipShooter : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Vector2 fireDirection = Vector2.right;
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0.72f, 0f);
        [SerializeField] private float fireInterval = 0.16f;

        private ShipInput shipInput;
        private HypeState hypeState;
        private float cooldown;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
        }

        private void Start()
        {
            hypeState = FindAnyObjectByType<HypeState>();
        }

        private void Update()
        {
            // Input checks for EX Move [Q] and Super Art [E]
            var superPressed = false;
            var exPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                    superPressed = true;
                if (Keyboard.current.qKey.wasPressedThisFrame)
                    exPressed = true;
            }
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                    superPressed = true;
                if (Gamepad.current.buttonWest.wasPressedThisFrame)
                    exPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.E))
                superPressed = true;
            if (Input.GetKeyDown(KeyCode.Q))
                exPressed = true;
#endif

            if (superPressed)
                TryFireSuper();
            else if (exPressed)
                TryFireEX();
        }

        private void LateUpdate()
        {
            if (cooldown > 0f)
                cooldown -= Time.deltaTime;

            if (shipInput == null)
                return;

            if (!shipInput.FireHeld || cooldown > 0f)
                return;

            Shoot();
            cooldown = fireInterval;
        }

        public bool TryFireEX()
        {
            if (hypeState == null)
                hypeState = FindAnyObjectByType<HypeState>();

            if (hypeState == null || !hypeState.CanUseEX)
                return false;

            if (!hypeState.TryUseEX())
                return false;

            FireEXRocket();
            return true;
        }

        public bool TryFireSuper()
        {
            if (hypeState == null)
                hypeState = FindAnyObjectByType<HypeState>();

            if (hypeState == null || !hypeState.CanUseSuper)
                return false;

            if (!hypeState.TryUseSuper())
                return false;

            FireSuperBeam();
            GameRules.NotifySuperUsed();
            return true;
        }

        private void FireEXRocket()
        {
            var origin = (Vector2)transform.position + muzzleOffset;
            var exObj = new GameObject("EXRocket", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            exObj.transform.position = origin;
            var rocket = exObj.AddComponent<EXMissile>();
            rocket.Launch(fireDirection);

            AudioManager.PlayEXShoot();
            EffectsManager.PlayMuzzleFlash(origin);
            CameraShake.Shake(0.35f, 0.25f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }

        private void FireSuperBeam()
        {
            var origin = (Vector2)transform.position + muzzleOffset;
            var superObj = new GameObject("SuperBeam", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            superObj.transform.position = origin;
            var beam = superObj.AddComponent<SuperProjectile>();
            beam.Launch(fireDirection);

            AudioManager.PlaySuperBeam();
            EffectsManager.PlayMuzzleFlash(origin);
            CameraShake.HitStop(0.06f);
            CameraShake.Shake(0.6f, 0.5f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }

        private void Shoot()
        {
            var direction = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
            var origin = (Vector2)transform.position + muzzleOffset;

            if (projectilePrefab != null)
            {
                var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
                projectile.Launch(direction);
            }
            else
            {
                var fallbackProj = new GameObject("Projectile", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
                fallbackProj.transform.position = origin;
                var p = fallbackProj.AddComponent<Projectile>();
                p.Launch(direction);
            }

            AudioManager.PlayShoot();
            EffectsManager.PlayMuzzleFlash(origin);
            CameraShake.Shake(0.035f, 0.1f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }
    }
}
