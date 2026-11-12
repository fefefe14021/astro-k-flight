// Assets/Scripts/Gameplay/ShipThrusterVisual.cs
// Namespace: KANYE.Gameplay
// GameObject: Player (UFO Flying Saucer)

using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(ShipInput))]
    public class ShipThrusterVisual : MonoBehaviour
    {
        private ShipInput shipInput;
        private SpriteRenderer flameRenderer;
        private Transform flameTransform;
        private SpriteRenderer antiGravRenderer;
        private Transform antiGravTransform;
        private float flameFlickerTimer;
        private float recoilPunch;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
            CreateFlame();
            CreateAntiGravGlow();
        }

        private void CreateFlame()
        {
            var existing = transform.Find("ThrusterFlame");
            if (existing != null)
            {
                flameTransform = existing;
                flameRenderer = existing.GetComponent<SpriteRenderer>();
                return;
            }

            var flameObject = new GameObject("ThrusterFlame");
            flameTransform = flameObject.transform;
            flameTransform.SetParent(transform, false);

            // Rear exhaust nozzle of the saucer (-X)
            flameTransform.localPosition = new Vector3(-0.65f, 0.02f, 0f);
            flameTransform.localRotation = Quaternion.identity;

            flameRenderer = flameObject.AddComponent<SpriteRenderer>();
            var thrusterSprite = Resources.Load<Sprite>("Sprites/thruster");
            if (thrusterSprite != null)
                flameRenderer.sprite = thrusterSprite;

            PlaceholderShipVisual.UseSpriteMaterial(flameRenderer);
            flameRenderer.sortingOrder = 9;
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
            antiGravRenderer.color = new Color(0.2f, 0.85f, 1f, 0.6f);
            antiGravRenderer.sortingOrder = 8;
        }

        public void TriggerRecoil()
        {
            recoilPunch = 0.28f;
        }

        private void Update()
        {
            var move = shipInput != null ? shipInput.Move : Vector2.zero;

            // 1. Thruster flame animation and scaling
            flameFlickerTimer += Time.deltaTime * 24f;
            var flicker = Mathf.Sin(flameFlickerTimer) * 0.12f + (Random.value * 0.15f - 0.075f);

            var thrustIntensity = Mathf.Clamp01(0.8f + move.x * 0.5f);
            var scaleX = (0.95f + flicker + recoilPunch * 0.7f) * thrustIntensity;
            var scaleY = 0.85f + flicker * 0.35f;

            if (flameTransform != null)
            {
                flameTransform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            if (flameRenderer != null)
            {
                var c = move.x > 0.1f
                    ? new Color(0.95f, 0.98f, 1f, 0.95f)
                    : new Color(0.3f, 0.8f, 1f, 0.85f);
                flameRenderer.color = c;
            }

            // 2. Anti-gravity ventral glow pulsation
            if (antiGravTransform != null)
            {
                var pulse = 0.9f + Mathf.Sin(Time.time * 8f) * 0.25f;
                antiGravTransform.localScale = new Vector3(pulse * 1.4f, pulse * 0.6f, 1f);
            }

            // 3. Controlled flight pitch & subtle aiming challenge (clamped between -7 and +7 deg)
            var targetTilt = 0f;
            if (move.y > 0.1f)
                targetTilt = 6.5f;   // Pitching up slightly directs fire upward
            else if (move.y < -0.1f)
                targetTilt = -6.5f;  // Pitching down slightly directs fire downward

            // Gun recoil kick pushes nose slightly
            targetTilt -= recoilPunch * 6.0f;
            targetTilt = Mathf.Clamp(targetTilt, -8f, 8f);

            var currentZ = transform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f; // Map into [-180, 180] range for safe clamping

            var newZ = Mathf.MoveTowards(currentZ, targetTilt, Time.deltaTime * 140f);
            transform.localRotation = Quaternion.Euler(0f, 0f, newZ);

            // 4. Gun recoil dampening
            if (recoilPunch > 0f)
            {
                recoilPunch = Mathf.MoveTowards(recoilPunch, 0f, Time.deltaTime * 2.8f);
            }
        }
    }
}
