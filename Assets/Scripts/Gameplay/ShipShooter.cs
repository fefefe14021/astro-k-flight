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

        public float CurrentHeat { get; private set; }
        public bool IsOverheated { get; private set; }

        private const float OverheatDuration = 1.35f;
        private const float HeatCooldownRate = 0.40f;
        private float overheatTimer;
        private float steamTimer;

        private Transform heatBarRoot;
        private SpriteRenderer heatBarFill;
        private SpriteRenderer heatBarBg;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
        }

        private void Start()
        {
            hypeState = FindAnyObjectByType<HypeState>();
            BuildHeatGauge();
        }

        private void BuildHeatGauge()
        {
            var gaugeObj = new GameObject("HeatGauge");
            heatBarRoot = gaugeObj.transform;
            heatBarRoot.SetParent(transform, false);
            heatBarRoot.localPosition = new Vector3(0f, -0.56f, 0f);

            var bgObj = new GameObject("Bg", typeof(SpriteRenderer));
            bgObj.transform.SetParent(heatBarRoot, false);
            heatBarBg = bgObj.GetComponent<SpriteRenderer>();
            heatBarBg.sprite = MakePixelSprite();
            heatBarBg.color = new Color(0.08f, 0.1f, 0.14f, 0.75f);
            heatBarBg.transform.localScale = new Vector3(0.92f, 0.12f, 1f);
            heatBarBg.sortingOrder = 2;

            var fillObj = new GameObject("Fill", typeof(SpriteRenderer));
            fillObj.transform.SetParent(heatBarRoot, false);
            heatBarFill = fillObj.GetComponent<SpriteRenderer>();
            heatBarFill.sprite = MakePixelSprite();
            heatBarFill.color = new Color(0.25f, 0.95f, 0.65f, 0.95f);
            heatBarFill.transform.localScale = new Vector3(0.88f, 0.08f, 1f);
            heatBarFill.sortingOrder = 3;

            heatBarRoot.gameObject.SetActive(false);
        }

        private static Sprite MakePixelSprite()
        {
            var tex = new Texture2D(2, 2);
            var px = new Color[] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }

        private void Update()
        {
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

            UpdateHeatState();

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
            {
                if (!TryFireSuper())
                {
                    AudioManager.PlayDenied();
                    HypeHud.Instance?.ShakeMeterDenied();
                }
            }
            else if (exPressed)
            {
                if (!TryFireEX())
                {
                    AudioManager.PlayDenied();
                    HypeHud.Instance?.ShakeMeterDenied();
                }
            }
        }

        private void UpdateHeatState()
        {
            if (IsOverheated)
            {
                overheatTimer -= Time.deltaTime;
                CurrentHeat = Mathf.Clamp01(overheatTimer / OverheatDuration);

                steamTimer += Time.deltaTime;
                if (steamTimer >= 0.10f)
                {
                    steamTimer = 0f;
                    var origin = (Vector2)(transform.position + transform.rotation * (Vector3)muzzleOffset);
                    EffectsManager.EmitExhaustParticle(origin, new Color(1f, 0.35f, 0.2f, 0.75f), Vector2.up * 1.5f, "steam");
                }

                if (overheatTimer <= 0f)
                {
                    IsOverheated = false;
                    CurrentHeat = 0f;
                    AudioManager.PlayClick();
                }
            }
            else
            {
                if (shipInput == null || !shipInput.FireHeld)
                {
                    CurrentHeat = Mathf.Max(0f, CurrentHeat - HeatCooldownRate * Time.deltaTime);
                }
            }

            UpdateHeatGaugeVisual();
        }

        private void UpdateHeatGaugeVisual()
        {
            if (heatBarRoot == null) return;

            // Only show gauge when heating up or overheated
            if (CurrentHeat <= 0.04f && !IsOverheated)
            {
                heatBarRoot.gameObject.SetActive(false);
                return;
            }

            heatBarRoot.gameObject.SetActive(true);
            heatBarRoot.localRotation = Quaternion.Inverse(transform.rotation); // keep horizontal

            Color heatCol;
            if (IsOverheated)
            {
                var flash = Mathf.PingPong(Time.time * 20f, 1f);
                heatCol = Color.Lerp(new Color(1f, 0.15f, 0.15f), Color.white, flash);
            }
            else if (CurrentHeat > 0.80f)
            {
                heatCol = new Color(1f, 0.22f, 0.18f);
            }
            else if (CurrentHeat > 0.50f)
            {
                heatCol = new Color(1f, 0.80f, 0.22f);
            }
            else
            {
                heatCol = new Color(0.25f, 0.95f, 0.65f);
            }

            if (heatBarFill != null)
            {
                heatBarFill.color = heatCol;
                var maxWidth = 0.88f;
                var currentWidth = maxWidth * Mathf.Clamp01(CurrentHeat);
                heatBarFill.transform.localScale = new Vector3(currentWidth, 0.08f, 1f);
                heatBarFill.transform.localPosition = new Vector3(-maxWidth * 0.5f + currentWidth * 0.5f, 0f, 0f);
            }
        }

        private void LateUpdate()
        {
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

            if (cooldown > 0f)
                cooldown -= Time.deltaTime;

            if (shipInput == null)
                return;

            if (!shipInput.FireHeld || cooldown > 0f || IsOverheated)
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
            SuperProjectile.UnleashMegaLaser(transform, muzzleOffset);
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

            // Weapon Overheat Accumulation
            var heatPerShot = weapon.Type == WeaponType.ScatterShotgun ? 0.095f :
                              weapon.Type == WeaponType.HomingMissiles ? 0.075f :
                              weapon.Type == WeaponType.HeavyRailgun ? 0.18f :
                              weapon.Type == WeaponType.SawDisc ? 0.085f :
                              weapon.Type == WeaponType.PlasmaMortar ? 0.22f :
                              0.040f; // TwinBlaster: ~3.5 seconds sustained continuous fire

            CurrentHeat += heatPerShot;
            if (CurrentHeat >= 1.0f)
            {
                CurrentHeat = 1.0f;
                IsOverheated = true;
                overheatTimer = OverheatDuration;
                CameraShake.Shake(0.18f, 0.2f);
                EffectsManager.PlayImpact(origin, Vector2.up, new Color(1f, 0.35f, 0.15f));
            }

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
