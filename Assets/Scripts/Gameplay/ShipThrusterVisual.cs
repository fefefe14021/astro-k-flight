// Assets/Scripts/Gameplay/ShipThrusterVisual.cs
// Namespace: KANYE.Gameplay
// GameObject: Player (UFO Flying Saucer)
// Rear-mounted mechanical engine nozzle, glowing combustion throat, dynamic exhaust flame, and anti-grav ring.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(ShipInput))]
    public class ShipThrusterVisual : MonoBehaviour
    {
        private ShipInput shipInput;

        private Transform nozzleTransform;
        private SpriteRenderer nozzleRenderer;
        private Transform nozzleGlowTransform;
        private SpriteRenderer nozzleGlowRenderer;

        private Transform flameTransform;
        private SpriteRenderer flameRenderer;

        private Transform antiGravTransform;
        private SpriteRenderer antiGravRenderer;

        private float flameFlickerTimer;
        private float recoilPunch;
        private float particleTimer;
        private ShipSkin currentSkin;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
            CreateEngineNozzle();
            CreateFlame();
            CreateAntiGravGlow();
            ApplySkinVisuals();
        }

        private void OnEnable()
        {
            ApplySkinVisuals();
        }

        public void ApplySkinVisuals()
        {
            var skinId = SaveManager.Instance != null ? SaveManager.Instance.Data.equippedSkinId : SaveManager.DefaultSkinId;
            currentSkin = ShipSkinCatalog.Find(skinId);

            if (nozzleRenderer != null && currentSkin != null)
            {
                var nozzleSpr = Resources.Load<Sprite>(currentSkin.NozzleSprite);
                if (nozzleSpr == null)
                    nozzleSpr = Resources.Load<Sprite>("Sprites/engine_nozzle");

                if (nozzleSpr != null)
                    nozzleRenderer.sprite = nozzleSpr;
            }

            if (nozzleGlowRenderer != null && currentSkin != null)
            {
                nozzleGlowRenderer.color = currentSkin.ThrusterColor;
            }

            if (flameRenderer != null && currentSkin != null)
            {
                var spr = Resources.Load<Sprite>(currentSkin.ThrusterSprite);
                if (spr == null)
                    spr = Resources.Load<Sprite>("Sprites/thruster");

                if (spr != null)
                    flameRenderer.sprite = spr;

                flameRenderer.color = currentSkin.ThrusterColor;
            }

            if (antiGravRenderer != null && currentSkin != null)
            {
                antiGravRenderer.color = currentSkin.AntiGravColor;
            }
        }

        private void CreateEngineNozzle()
        {
            var existing = transform.Find("EngineNozzle");
            if (existing != null)
            {
                nozzleTransform = existing;
                nozzleRenderer = existing.GetComponent<SpriteRenderer>();
            }
            else
            {
                var nozzleObject = new GameObject("EngineNozzle");
                nozzleTransform = nozzleObject.transform;
                nozzleTransform.SetParent(transform, false);

                // Anchored to the rear rim of the flying saucer (-X)
                // Pivot is {x: 1, y: 0.5} (the right mounting bracket edge).
                // Positioning at (-0.62f, -0.083f, 0f) mounts brackets directly onto the saucer hull.
                nozzleTransform.localPosition = new Vector3(-0.62f, -0.083f, 0f);
                nozzleTransform.localRotation = Quaternion.identity;
                nozzleTransform.localScale = Vector3.one;

                nozzleRenderer = nozzleObject.AddComponent<SpriteRenderer>();
                PlaceholderShipVisual.UseSpriteMaterial(nozzleRenderer);
                nozzleRenderer.sortingOrder = 11; // Renders over the saucer rim (sortingOrder 10)
            }

            var defaultSpr = Resources.Load<Sprite>("Sprites/engine_nozzle");
            if (defaultSpr != null && nozzleRenderer != null && nozzleRenderer.sprite == null)
                nozzleRenderer.sprite = defaultSpr;

            // Inner combustion throat glow
            var existingGlow = nozzleTransform.Find("NozzleGlow");
            if (existingGlow != null)
            {
                nozzleGlowTransform = existingGlow;
                nozzleGlowRenderer = existingGlow.GetComponent<SpriteRenderer>();
            }
            else
            {
                var glowObj = new GameObject("NozzleGlow");
                nozzleGlowTransform = glowObj.transform;
                nozzleGlowTransform.SetParent(nozzleTransform, false);

                // Deep inside the nozzle bell
                nozzleGlowTransform.localPosition = new Vector3(-0.25f, 0f, 0f);
                nozzleGlowTransform.localRotation = Quaternion.identity;
                nozzleGlowTransform.localScale = new Vector3(1.15f, 1.15f, 1f);

                nozzleGlowRenderer = glowObj.AddComponent<SpriteRenderer>();
                var glowSpr = Resources.Load<Sprite>("Sprites/engine_glow");
                if (glowSpr != null)
                    nozzleGlowRenderer.sprite = glowSpr;

                PlaceholderShipVisual.UseSpriteMaterial(nozzleGlowRenderer);
                nozzleGlowRenderer.sortingOrder = 10;
            }
        }

        private void CreateFlame()
        {
            var parentTransform = nozzleTransform != null ? nozzleTransform : transform;
            var existing = parentTransform.Find("ThrusterFlame");
            if (existing == null)
                existing = transform.Find("ThrusterFlame");

            if (existing != null)
            {
                flameTransform = existing;
                flameRenderer = existing.GetComponent<SpriteRenderer>();
            }
            else
            {
                var flameObject = new GameObject("ThrusterFlame");
                flameTransform = flameObject.transform;
                flameTransform.SetParent(parentTransform, false);

                flameRenderer = flameObject.AddComponent<SpriteRenderer>();
                PlaceholderShipVisual.UseSpriteMaterial(flameRenderer);
                flameRenderer.sortingOrder = 9; // Inside / behind the outer nozzle bell
            }

            if (flameTransform.parent != nozzleTransform && nozzleTransform != null)
            {
                flameTransform.SetParent(nozzleTransform, false);
            }

            // Anchored inside the nozzle throat
            flameTransform.localPosition = new Vector3(-0.28f, 0f, 0f);
            flameTransform.localRotation = Quaternion.identity;

            var thrusterSprite = Resources.Load<Sprite>("Sprites/thruster_ion");
            if (thrusterSprite == null)
                thrusterSprite = Resources.Load<Sprite>("Sprites/thruster");

            if (thrusterSprite != null)
                flameRenderer.sprite = thrusterSprite;
        }

        private void CreateAntiGravGlow()
        {
            var existing = transform.Find("AntiGravGlow");
            if (existing != null)
            {
                antiGravTransform = existing;
                antiGravRenderer = existing.GetComponent<SpriteRenderer>();
                return;
            }

            var glowObject = new GameObject("AntiGravGlow");
            antiGravTransform = glowObject.transform;
            antiGravTransform.SetParent(transform, false);
            antiGravTransform.localPosition = new Vector3(0f, -0.28f, 0f);

            antiGravRenderer = glowObject.AddComponent<SpriteRenderer>();
            var spark = Resources.Load<Sprite>("Sprites/spark");
            if (spark != null)
                antiGravRenderer.sprite = spark;

            PlaceholderShipVisual.UseSpriteMaterial(antiGravRenderer);
            antiGravRenderer.color = new Color(0.2f, 0.85f, 1f, 0.7f);
            antiGravRenderer.sortingOrder = 8;
        }

        public void TriggerRecoil()
        {
            recoilPunch = 0.32f;
        }

        private void Update()
        {
            if (currentSkin == null)
                ApplySkinVisuals();

            var move = shipInput != null ? shipInput.Move : Vector2.zero;
            var isThrusting = move.x > 0.1f;

            // 1. Dynamic Flame Animation & Hot Burn scaling
            flameFlickerTimer += Time.deltaTime * 28f;
            var flicker = Mathf.Sin(flameFlickerTimer) * 0.14f + (Random.value * 0.16f - 0.08f);

            var thrustIntensity = isThrusting ? (1.35f + move.x * 0.45f) : 0.95f;
            var scaleX = (1.0f + flicker + recoilPunch * 0.85f) * thrustIntensity;
            var scaleY = 0.92f + flicker * 0.25f;

            if (flameTransform != null)
            {
                flameTransform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            if (flameRenderer != null && currentSkin != null)
            {
                var baseCol = currentSkin.ThrusterColor;
                var hotCol = Color.Lerp(baseCol, Color.white, 0.55f);
                flameRenderer.color = isThrusting ? hotCol : baseCol;
            }

            // 2. Interior Nozzle Combustion Throat Glow
            if (nozzleGlowTransform != null && nozzleGlowRenderer != null && currentSkin != null)
            {
                var glowPulse = 0.85f + Mathf.Sin(Time.time * 18f) * 0.15f;
                var targetScale = isThrusting ? (1.45f * glowPulse) : (1.1f * glowPulse);
                nozzleGlowTransform.localScale = new Vector3(targetScale, targetScale, 1f);

                var glowCol = isThrusting ? Color.Lerp(currentSkin.ThrusterColor, Color.white, 0.75f) : currentSkin.ThrusterColor;
                glowCol.a = isThrusting ? 0.95f : 0.45f;
                nozzleGlowRenderer.color = glowCol;
            }

            // 3. Anti-gravity ventral glow pulsation
            if (antiGravTransform != null)
            {
                var pulse = 1.0f + Mathf.Sin(Time.time * 9f) * 0.22f;
                antiGravTransform.localScale = new Vector3(pulse * 1.5f, pulse * 0.55f, 1f);
            }

            // 4. Custom Per-Skin Particle Emissions
            particleTimer += Time.deltaTime;
            var interval = isThrusting ? 0.032f : 0.065f;
            if (particleTimer >= interval && currentSkin != null && flameTransform != null)
            {
                particleTimer = 0f;
                var tipPos = flameTransform.position - flameTransform.right * (scaleX * 0.45f) + (Vector3)Random.insideUnitCircle * 0.06f;
                var pVel = new Vector2(-4.0f - Random.value * 2.5f, Random.Range(-0.5f, 0.5f));
                EffectsManager.EmitExhaustParticle(tipPos, currentSkin.ParticleColor, pVel, currentSkin.ParticleType);

                // Ventral anti-grav hover motes
                if (Random.value > 0.65f && antiGravTransform != null)
                {
                    var ventralPos = antiGravTransform.position + (Vector3)new Vector2(Random.Range(-0.35f, 0.35f), -0.05f);
                    EffectsManager.EmitExhaustParticle(ventralPos, currentSkin.AntiGravColor, new Vector2(-1.2f, -0.8f), currentSkin.ParticleType);
                }
            }

            // 5. Controlled flight pitch & subtle aiming challenge (clamped between -8 and +8 deg)
            var targetTilt = 0f;
            if (move.y > 0.1f)
                targetTilt = 6.5f;   // Pitching up slightly directs fire upward
            else if (move.y < -0.1f)
                targetTilt = -6.5f;  // Pitching down slightly directs fire downward

            // Gun recoil kick pushes nose slightly
            targetTilt -= recoilPunch * 6.5f;
            targetTilt = Mathf.Clamp(targetTilt, -8f, 8f);

            var currentZ = transform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f; // Map into [-180, 180] range for safe clamping

            var newZ = Mathf.MoveTowards(currentZ, targetTilt, Time.deltaTime * 140f);
            transform.localRotation = Quaternion.Euler(0f, 0f, newZ);

            // 6. Gun recoil dampening
            if (recoilPunch > 0f)
            {
                recoilPunch = Mathf.MoveTowards(recoilPunch, 0f, Time.deltaTime * 3.0f);
            }
        }
    }
}
