// Assets/Scripts/Gameplay/ShipShooter.cs
// Namespace: KANYE.Gameplay
// Manages primary weapon arsenal, EX special rockets, and Super Art mega beam.

using KANYE.Audio;
using KANYE.Systems;
using KANYE.UI;
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
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

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
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

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
            EffectsManager.PlayMuzzleFlash(origin, new Color(1f, 0.65f, 0.2f), 1.8f);
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
            EffectsManager.PlayMuzzleFlash(origin, new Color(0.25f, 0.9f, 1f), 2.6f);
            CameraShake.HitStop(0.06f);
            CameraShake.Shake(0.6f, 0.5f);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
        }

        private void Shoot()
        {
            var weaponId = SaveManager.Instance != null ? SaveManager.Instance.Data.equippedWeaponId : "twin_blaster";
            var weapon = WeaponCatalog.Find(weaponId);
            var level = SaveManager.Instance != null ? SaveManager.Instance.GetWeaponLevel(weaponId) : 1;
            var damage = WeaponCatalog.GetDamage(weaponId, level);
            var fireInterval = WeaponCatalog.GetFireInterval(weaponId, level);
            var projCount = WeaponCatalog.GetProjectileCount(weaponId, level);

            if (GameRules.CurrentMode == GameMode.RogueExpedition)
            {
                damage += RogueExpeditionController.BonusDamage;
                fireInterval /= Mathf.Max(0.5f, RogueExpeditionController.FireRateMultiplier);
            }

            // Muzzle points according to the tilted saucer's nose!
            var baseDirection = (Vector2)transform.right;
            var origin = (Vector2)(transform.position + transform.rotation * (Vector3)muzzleOffset);

            switch (weapon.Type)
            {
                case WeaponType.TwinBlaster:
                    var upOffset = (Vector2)(transform.up * 0.16f);
                    SpawnProjectile(origin + upOffset, baseDirection, weapon, damage, level);
                    SpawnProjectile(origin - upOffset, baseDirection, weapon, damage, level);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.035f, 0.1f);
                    break;

                case WeaponType.ScatterShotgun:
                    float[] angles = projCount == 7 ? new[] { -18f, -12f, -6f, 0f, 6f, 12f, 18f } :
                                     projCount == 6 ? new[] { -15f, -9f, -3f, 3f, 9f, 15f } :
                                                      new[] { -15f, -7.5f, 0f, 7.5f, 15f };
                    for (var i = 0; i < angles.Length; i++)
                    {
                        var rot = Quaternion.Euler(0f, 0f, angles[i]);
                        var dir = (Vector2)(rot * (Vector3)baseDirection);
                        SpawnProjectile(origin, dir, weapon, damage, level);
                    }
                    AudioManager.PlayEXShoot();
                    CameraShake.Shake(0.18f, 0.15f);
                    break;

                case WeaponType.HomingMissiles:
                    if (projCount >= 4)
                    {
                        SpawnProjectile(origin + (Vector2)(transform.up * 0.22f), (Vector2)(Quaternion.Euler(0f, 0f, 16f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin + (Vector2)(transform.up * 0.08f), (Vector2)(Quaternion.Euler(0f, 0f, 6f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin - (Vector2)(transform.up * 0.08f), (Vector2)(Quaternion.Euler(0f, 0f, -6f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin - (Vector2)(transform.up * 0.22f), (Vector2)(Quaternion.Euler(0f, 0f, -16f) * (Vector3)baseDirection), weapon, damage, level);
                    }
                    else if (projCount == 3)
                    {
                        SpawnProjectile(origin + (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, 14f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin, baseDirection, weapon, damage, level);
                        SpawnProjectile(origin - (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, -14f) * (Vector3)baseDirection), weapon, damage, level);
                    }
                    else
                    {
                        SpawnProjectile(origin + (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, 12f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin - (Vector2)(transform.up * 0.18f), (Vector2)(Quaternion.Euler(0f, 0f, -12f) * (Vector3)baseDirection), weapon, damage, level);
                    }
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.05f, 0.12f);
                    break;

                case WeaponType.HeavyRailgun:
                    SpawnProjectile(origin, baseDirection, weapon, damage, level);
                    AudioManager.PlaySuperBeam();
                    CameraShake.Shake(0.22f, 0.2f);
                    break;

                case WeaponType.SawDisc:
                    if (projCount >= 2)
                    {
                        SpawnProjectile(origin + (Vector2)(transform.up * 0.12f), (Vector2)(Quaternion.Euler(0f, 0f, 5f) * (Vector3)baseDirection), weapon, damage, level);
                        SpawnProjectile(origin - (Vector2)(transform.up * 0.12f), (Vector2)(Quaternion.Euler(0f, 0f, -5f) * (Vector3)baseDirection), weapon, damage, level);
                    }
                    else
                    {
                        SpawnProjectile(origin, baseDirection, weapon, damage, level);
                    }
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.08f, 0.14f);
                    break;

                case WeaponType.PlasmaMortar:
                    SpawnProjectile(origin, baseDirection, weapon, damage, level);
                    AudioManager.PlayEXShoot();
                    CameraShake.Shake(0.2f, 0.18f);
                    break;

                default:
                    SpawnProjectile(origin, baseDirection, weapon, damage, level);
                    AudioManager.PlayShoot();
                    CameraShake.Shake(0.035f, 0.1f);
                    break;
            }

            var flashScale = weapon.Type == WeaponType.HeavyRailgun ? 2.0f :
                             weapon.Type == WeaponType.PlasmaMortar ? 1.7f :
                             weapon.Type == WeaponType.ScatterShotgun ? 1.8f : 1.25f;
            EffectsManager.PlayMuzzleFlash(origin, weapon.ThemeColor, flashScale);
            GetComponent<ShipThrusterVisual>()?.TriggerRecoil();
            cooldown = fireInterval;
        }

        private static void SpawnProjectile(Vector2 pos, Vector2 dir, WeaponDefinition weapon, int damage, int level)
        {
            var projObj = new GameObject("Projectile", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            projObj.transform.position = pos;
            var p = projObj.AddComponent<Projectile>();
            p.Configure(weapon, dir, damage, level);
        }
    }
}
