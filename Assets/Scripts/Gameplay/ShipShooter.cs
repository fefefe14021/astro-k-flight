// Assets/Scripts/Gameplay/ShipShooter.cs
// Namespace: KANYE.Gameplay
// Manages primary weapon arsenal, EX special rockets, and Super Art mega beam.

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
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0.68f, 0.02f);

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
            var launchDir = (Vector2)transform.right;
            var origin = (Vector2)(transform.position + transform.rotation * (Vector3)muzzleOffset);
            var exObj = new GameObject("EXRocket", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            exObj.transform.position = origin;
            var rocket = exObj.AddComponent<EXMissile>();
            rocket.Launch(launchDir);

            AudioManager.PlayEXShoot();
            EffectsManager.PlayMuzzleFlash(origin);
            CameraShake.Shake(0.35f, 0.25f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }

        private void FireSuperBeam()
        {
            var launchDir = (Vector2)transform.right;
            var origin = (Vector2)(transform.position + transform.rotation * (Vector3)muzzleOffset);
            var superObj = new GameObject("SuperBeam", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            superObj.transform.position = origin;
            var beam = superObj.AddComponent<SuperProjectile>();
            beam.Launch(launchDir);

            AudioManager.PlaySuperBeam();
            EffectsManager.PlayMuzzleFlash(origin);
            CameraShake.HitStop(0.06f);
            CameraShake.Shake(0.6f, 0.5f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }

        private void Shoot()
        {
            var weaponId = SaveManager.Instance != null ? SaveManager.Instance.Data.equippedWeaponId : "twin_blaster";
            var weapon = WeaponCatalog.Find(weaponId);

            // Muzzle points according to the tilted saucer's nose!
            var baseDirection = (Vector2)transform.right;
            var origin = (Vector2)(transform.position + transform.rotation * (Vector3)muzzleOffset);

            switch (weapon.Type)
            {
                case WeaponType.TwinBlaster:
                    var upOffset = (Vector2)(transform.up * 0.16f);
                    SpawnProjectile(origin + upOffset, baseDirection, weapon);
                    SpawnProjectile(origin - upOffset, baseDirection, weapon);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.035f, 0.1f);
                    break;

                case WeaponType.ScatterShotgun:
                    float[] angles = { -15f, -7.5f, 0f, 7.5f, 15f };
                    for (var i = 0; i < angles.Length; i++)
                    {
                        var rot = Quaternion.Euler(0f, 0f, angles[i]);
                        var dir = (Vector2)(rot * (Vector3)baseDirection);
                        SpawnProjectile(origin, dir, weapon);
                    }
                    AudioManager.PlayEXShoot();
                    CameraShake.Shake(0.18f, 0.15f);
                    break;

                case WeaponType.HomingMissiles:
                    SpawnProjectile(origin + (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, 12f) * (Vector3)baseDirection), weapon);
                    SpawnProjectile(origin - (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, -12f) * (Vector3)baseDirection), weapon);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.05f, 0.12f);
                    break;

                case WeaponType.HeavyRailgun:
                    SpawnProjectile(origin, baseDirection, weapon);
                    AudioManager.PlaySuperBeam();
                    CameraShake.Shake(0.22f, 0.2f);
                    break;

                case WeaponType.SawDisc:
                    SpawnProjectile(origin, baseDirection, weapon);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.08f, 0.14f);
                    break;

                case WeaponType.PlasmaMortar:
                    SpawnProjectile(origin, baseDirection, weapon);
                    AudioManager.PlayEXShoot();
                    CameraShake.Shake(0.2f, 0.18f);
                    break;

                default:
                    SpawnProjectile(origin, baseDirection, weapon);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.035f, 0.1f);
                    break;
            }

            EffectsManager.PlayMuzzleFlash(origin);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
            cooldown = weapon.FireInterval;
        }

        private static void SpawnProjectile(Vector2 pos, Vector2 dir, WeaponDefinition weapon)
        {
            var projObj = new GameObject("Projectile", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            projObj.transform.position = pos;
            var p = projObj.AddComponent<Projectile>();
            p.Configure(weapon, dir);
        }
    }
}
