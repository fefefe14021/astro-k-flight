// Assets/Scripts/Gameplay/ShipThrusterVisual.cs
// Namespace: KANYE.Gameplay
// GameObject: Player

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
        private float flameFlickerTimer;
        private float recoilPunch;

        private void Awake()
        {
            shipInput = GetComponent<ShipInput>();
            CreateFlame();
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

            // Exhaust at the rear (-X) of the ship
            flameTransform.localPosition = new Vector3(-0.72f, 0f, 0f);
            flameTransform.localRotation = Quaternion.identity;

            flameRenderer = flameObject.AddComponent<SpriteRenderer>();
            var thrusterSprite = Resources.Load<Sprite>("Sprites/thruster");
            if (thrusterSprite != null)
                flameRenderer.sprite = thrusterSprite;

            PlaceholderShipVisual.UseSpriteMaterial(flameRenderer);
            flameRenderer.sortingOrder = 9; // right under ship sprite
        }

        public void TriggerRecoil()
        {
            recoilPunch = 0.22f;
        }

        private void Update()
        {
            var move = shipInput != null ? shipInput.Move : Vector2.zero;

            // 1. Thruster flame animation and scaling
            flameFlickerTimer += Time.deltaTime * 24f;
            var flicker = Mathf.Sin(flameFlickerTimer) * 0.12f + (Random.value * 0.15f - 0.075f);

            var thrustIntensity = Mathf.Clamp01(1f + move.x * 0.5f);
            var scaleX = (1.0f + flicker + recoilPunch * 0.6f) * thrustIntensity;
            var scaleY = 1.0f + flicker * 0.4f;

            if (flameTransform != null)
            {
                flameTransform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            if (flameRenderer != null)
            {
                // Hot cyan/white when boosting forward, warmer cyan/blue when idle
                var c = move.x > 0.1f 
                    ? new Color(0.9f, 0.98f, 1f, 0.95f) 
                    : new Color(0.3f, 0.75f, 1f, 0.85f);
                flameRenderer.color = c;
            }

            // 2. Dynamic flight pitch banking around 0 degrees
            var targetTilt = 0f;
            if (move.y > 0.1f)
                targetTilt = 7f; // Nose pitches up (+7 deg)
            else if (move.y < -0.1f)
                targetTilt = -7f; // Nose pitches down (-7 deg)

            var currentZ = transform.localEulerAngles.z;
            var newZ = Mathf.MoveTowardsAngle(currentZ, targetTilt, Time.deltaTime * 180f);
            transform.localRotation = Quaternion.Euler(0f, 0f, newZ);

            // 3. Gun recoil dampening
            if (recoilPunch > 0f)
            {
                recoilPunch = Mathf.MoveTowards(recoilPunch, 0f, Time.deltaTime * 2.5f);
            }
        }
    }
}
