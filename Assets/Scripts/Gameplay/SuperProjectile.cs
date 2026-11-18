// Assets/Scripts/Gameplay/SuperProjectile.cs
// Namespace: KANYE.Gameplay
// Authentic 16-bit Pixel-Art Crimson Mega-Laser Beam with High-Speed Tiled Plasma Waves,
// Stepped Confinement Prongs, Crackling Arc Lightning, and Total Screen Obliteration.

using System.Collections.Generic;
using KANYE.Audio;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class SuperProjectile : MonoBehaviour
    {
        [Header("Beam Parameters")]
        [SerializeField] private float duration = 2.0f;
        [SerializeField] private float beamLength = 24f;
        [SerializeField] private int tickDamage = 7;
        [SerializeField] private float tickInterval = 0.07f;

        private Transform shipTransform;
        private Vector2 muzzleOffset = new Vector2(0.68f, 0.02f);
        private float lifeTimer;
        private float damageTimer;
        private float sparkTimer;
        private float scrollOffset;
        private float lightningTimer;
        private int currentLightningFrame;

        // Visual Components
        private SpriteRenderer beamRenderer;
        private Transform beamTransform;
        private SpriteRenderer emitterRenderer;
        private Transform emitterTransform;
        private SpriteRenderer impactRenderer;
        private Transform impactTransform;
        private readonly List<SpriteRenderer> lightningArcs = new List<SpriteRenderer>(4);

        // Procedural Pixel Art Sprites
        private static Sprite pixelBeamSprite;
        private static Sprite pixelEmitterSprite;
        private static Sprite[] pixelLightningSprites;
        private static Sprite pixelImpactSprite;

        public static SuperProjectile UnleashMegaLaser(Transform ship, Vector2 muzzle)
        {
            var go = new GameObject("PixelMegaLaser", typeof(SuperProjectile), typeof(BoxCollider2D), typeof(Rigidbody2D));
            var laser = go.GetComponent<SuperProjectile>();
            laser.Initialize(ship, muzzle);
            return laser;
        }

        public void Initialize(Transform ship, Vector2 muzzle)
        {
            shipTransform = ship;
            muzzleOffset = muzzle;

            transform.SetParent(ship, false);
            transform.localPosition = (Vector3)muzzleOffset;
            transform.localRotation = Quaternion.identity;

            SetupPhysics();
            EnsurePixelSprites();
            BuildPixelVisuals();

            lifeTimer = duration;
            damageTimer = 0f;

            // Grand visceral feedback
            CameraShake.HitStop(0.08f);
            CameraShake.Shake(0.55f, 0.35f);
            AudioManager.PlaySuperBeam();
            EffectsManager.PlayMuzzleFlash(transform.position, new Color(1f, 0.15f, 0.2f), 3.4f);
        }

        private void SetupPhysics()
        {
            var body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var box = GetComponent<BoxCollider2D>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider2D>();

            box.isTrigger = true;
            box.size = new Vector2(beamLength, 1.05f);
            box.offset = new Vector2(beamLength * 0.5f, 0f);
        }

        #region Procedural Pixel Art Generation

        private static void EnsurePixelSprites()
        {
            if (pixelBeamSprite != null)
                return;

            pixelBeamSprite = GeneratePixelBeamSprite();
            pixelEmitterSprite = GeneratePixelEmitterSprite();
            pixelLightningSprites = GeneratePixelLightningSprites();
            pixelImpactSprite = GeneratePixelImpactSprite();
        }

        private static Sprite GeneratePixelBeamSprite()
        {
            const int w = 64;
            const int h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            var px = new Color[w * h];

            // 10-tier calibrated laser palette
            var cBorderDark = new Color(0.20f, 0.00f, 0.04f, 1f); // #330008
            var cBorderRed  = new Color(0.45f, 0.00f, 0.08f, 1f); // #730014
            var cGlowDark   = new Color(0.72f, 0.02f, 0.12f, 1f); // #B8051E
            var cGlowBright = new Color(0.92f, 0.10f, 0.14f, 1f); // #EB1A24
            var cMantleRed  = new Color(1.00f, 0.22f, 0.16f, 1f); // #FF3829
            var cMantleOran = new Color(1.00f, 0.44f, 0.12f, 1f); // #FF701F
            var cEnergyOran = new Color(1.00f, 0.68f, 0.16f, 1f); // #FFAE29
            var cEnergyYell = new Color(1.00f, 0.88f, 0.28f, 1f); // #FFE047
            var cCoreWarm   = new Color(1.00f, 0.96f, 0.72f, 1f); // #FFF5B8
            var cCoreWhite  = new Color(1.00f, 1.00f, 1.00f, 1f); // #FFFFFF

            for (var y = 0; y < h; y++)
            {
                var distFromCenter = Mathf.Abs(y - 7.5f);

                for (var x = 0; x < w; x++)
                {
                    var waveNode = Mathf.Abs((x % 16) - 8) / 8f;
                    var dither = ((x + y) % 2 == 0);

                    Color col;

                    if (distFromCenter >= 7.0f)
                    {
                        col = dither ? cBorderDark : Color.clear;
                    }
                    else if (distFromCenter >= 5.8f)
                    {
                        col = dither ? cBorderRed : cGlowDark;
                    }
                    else if (distFromCenter >= 4.6f)
                    {
                        col = (waveNode > 0.6f && dither) ? cGlowBright : cGlowDark;
                    }
                    else if (distFromCenter >= 3.6f)
                    {
                        col = cGlowBright;
                    }
                    else if (distFromCenter >= 2.6f)
                    {
                        col = (waveNode > 0.4f) ? cMantleOran : cMantleRed;
                    }
                    else if (distFromCenter >= 1.6f)
                    {
                        col = (waveNode > 0.5f) ? cEnergyYell : cEnergyOran;
                    }
                    else if (distFromCenter >= 0.8f)
                    {
                        col = cCoreWarm;
                    }
                    else
                    {
                        col = ((x % 8) < 3) ? cCoreWhite : cCoreWarm;
                    }

                    px[y * w + x] = col;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }

        private static Sprite GeneratePixelEmitterSprite()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[s * s];

            var center = new Vector2(s * 0.5f, s * 0.5f);
            var cSteelDark  = new Color(0.14f, 0.16f, 0.22f, 1f);
            var cSteelLight = new Color(0.35f, 0.40f, 0.52f, 1f);
            var cCopper     = new Color(0.85f, 0.45f, 0.18f, 1f);
            var cCoreGlow   = new Color(1.00f, 0.20f, 0.25f, 1f);
            var cCoreHot    = new Color(1.00f, 0.90f, 0.50f, 1f);

            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    Color c = Color.clear;

                    if (d <= 14.5f && d >= 11.5f)
                    {
                        // Mechanical outer clamp teeth (every 45 deg)
                        var angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                        var tooth = Mathf.Abs(angle % 45f) < 18f;
                        c = tooth ? cSteelLight : cSteelDark;
                    }
                    else if (d < 11.5f && d >= 8.5f)
                    {
                        // Copper magnetic induction coil
                        c = ((x + y) % 2 == 0) ? cCopper : cSteelDark;
                    }
                    else if (d < 8.5f && d >= 4.5f)
                    {
                        // Blazing plasma chamber
                        c = cCoreGlow;
                    }
                    else if (d < 4.5f)
                    {
                        // White-hot focal emitter lens
                        c = cCoreHot;
                    }

                    px[y * s + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }

        private static Sprite[] GeneratePixelLightningSprites()
        {
            var sprites = new Sprite[3];
            const int w = 32;
            const int h = 16;

            for (var f = 0; f < 3; f++)
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                var px = new Color[w * h];

                var seedY = 8 + (f * 2);
                var currentY = seedY;

                for (var x = 0; x < w; x++)
                {
                    // Stepped jagged 1-pixel arc
                    var step = (x % 4 == 0) ? ((x + f * 5) % 3 - 1) : 0;
                    currentY = Mathf.Clamp(currentY + step, 2, h - 3);

                    var idx = currentY * w + x;
                    px[idx] = new Color(1f, 0.95f, 0.6f, 1f); // Hot yellow-white spark

                    // Subtle electric halo pixel
                    if (currentY + 1 < h)
                        px[(currentY + 1) * w + x] = new Color(1f, 0.3f, 0.2f, 0.75f);
                    if (currentY - 1 >= 0)
                        px[(currentY - 1) * w + x] = new Color(1f, 0.3f, 0.2f, 0.75f);
                }

                tex.SetPixels(px);
                tex.Apply();
                sprites[f] = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
            }

            return sprites;
        }

        private static Sprite GeneratePixelImpactSprite()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[s * s];

            var center = new Vector2(s * 0.5f, s * 0.5f);
            var cWhite  = new Color(1f, 1f, 0.9f, 1f);
            var cYellow = new Color(1f, 0.85f, 0.2f, 1f);
            var cOrange = new Color(1f, 0.45f, 0.1f, 0.9f);
            var cRed    = new Color(0.85f, 0.1f, 0.15f, 0.8f);

            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    var dither = ((x + y) % 2 == 0);
                    Color c = Color.clear;

                    if (d <= 3.5f)
                        c = cWhite;
                    else if (d <= 7.0f)
                        c = dither ? cWhite : cYellow;
                    else if (d <= 11.0f)
                        c = dither ? cYellow : cOrange;
                    else if (d <= 15.0f && dither)
                        c = cRed;

                    px[y * s + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }

        #endregion

        #region Visual Assembly

        private void BuildPixelVisuals()
        {
            // 1. Main Tiled Pixel Beam
            var beamObj = new GameObject("PixelLaserBeam", typeof(SpriteRenderer));
            beamObj.transform.SetParent(transform, false);
            beamTransform = beamObj.transform;
            beamTransform.localPosition = Vector3.zero;

            beamRenderer = beamObj.GetComponent<SpriteRenderer>();
            beamRenderer.sprite = pixelBeamSprite;
            beamRenderer.drawMode = SpriteDrawMode.Tiled;
            beamRenderer.size = new Vector2(beamLength, 1.5f);
            PlaceholderShipVisual.UseSpriteMaterial(beamRenderer);
            beamRenderer.sortingOrder = 15;

            // 2. Pixel Emitter Nozzle Focus Ring
            var emitObj = new GameObject("PixelEmitter", typeof(SpriteRenderer));
            emitObj.transform.SetParent(transform, false);
            emitterTransform = emitObj.transform;
            emitterTransform.localPosition = Vector3.zero;

            emitterRenderer = emitObj.GetComponent<SpriteRenderer>();
            emitterRenderer.sprite = pixelEmitterSprite;
            PlaceholderShipVisual.UseSpriteMaterial(emitterRenderer);
            emitterRenderer.sortingOrder = 17;

            // 3. Pixel Arc Lightning Discharges (Upper & Lower)
            CreateLightningArc(new Vector3(4f, 0.65f, 0f), false);
            CreateLightningArc(new Vector3(11f, -0.65f, 0f), true);
            CreateLightningArc(new Vector3(17f, 0.65f, 0f), false);

            // 4. Pixel Terminal Screen Burn Impact
            var impObj = new GameObject("PixelImpact", typeof(SpriteRenderer));
            impObj.transform.SetParent(transform, false);
            impactTransform = impObj.transform;
            impactTransform.localPosition = new Vector3(beamLength - 0.5f, 0f, 0f);

            impactRenderer = impObj.GetComponent<SpriteRenderer>();
            impactRenderer.sprite = pixelImpactSprite;
            PlaceholderShipVisual.UseSpriteMaterial(impactRenderer);
            impactRenderer.sortingOrder = 16;
        }

        private void CreateLightningArc(Vector3 localPos, bool flipY)
        {
            var arcObj = new GameObject("PixelArc", typeof(SpriteRenderer));
            arcObj.transform.SetParent(transform, false);
            arcObj.transform.localPosition = localPos;

            var sr = arcObj.GetComponent<SpriteRenderer>();
            sr.sprite = pixelLightningSprites[0];
            sr.flipY = flipY;
            PlaceholderShipVisual.UseSpriteMaterial(sr);
            sr.sortingOrder = 16;
            lightningArcs.Add(sr);
        }

        #endregion

        private void LateUpdate()
        {
            if (shipTransform != null)
            {
                transform.position = shipTransform.position + shipTransform.rotation * (Vector3)muzzleOffset;
                transform.rotation = shipTransform.rotation;
            }

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                CollapseLaser();
                return;
            }

            AnimatePixelLaser();
            ProcessDestructionTicks();

            // Continuous visceral screen shake
            CameraShake.Shake(0.14f, 0.08f);

            // Flying square pixel embers
            sparkTimer += Time.deltaTime;
            if (sparkTimer >= 0.035f)
            {
                sparkTimer = 0f;
                EffectsManager.PlayImpact(transform.position, -transform.right, new Color(1f, 0.8f, 0.25f));
                EffectsManager.PlayImpact(transform.position + transform.right * (beamLength * 0.75f), Vector2.left, new Color(1f, 0.35f, 0.15f));
            }
        }

        private void AnimatePixelLaser()
        {
            // 1. High-Speed Tiled Plasma Wave Scrolling
            scrollOffset += Time.deltaTime * 24f; // 24 units/sec plasma rush!
            if (beamTransform != null)
            {
                // Subtle horizontal step scroll
                var tileShift = (scrollOffset % (64f / 16f));
                beamTransform.localPosition = new Vector3(-tileShift * 0.15f, 0f, 0f);
            }

            // 2. Stepped Pixel Height Pulsation (12px <-> 14px <-> 16px) - Sleek, calibrated concentrated lance
            var pulsePhase = Mathf.FloorToInt((Time.time * 20f) % 4);
            var heightTiers = new float[] { 0.90f, 1.05f, 1.18f, 1.05f };
            var steppedHeight = heightTiers[pulsePhase];

            if (beamRenderer != null)
            {
                beamRenderer.size = new Vector2(beamLength + 2.0f, steppedHeight);
            }

            // 3. Emitter Focus Ring Rotation (Stepped 45 deg angles)
            if (emitterTransform != null)
            {
                var stepAngle = Mathf.FloorToInt((Time.time * 12f) % 8) * 45f;
                emitterTransform.localRotation = Quaternion.Euler(0f, 0f, stepAngle);
                var emitScale = steppedHeight * 0.82f;
                emitterTransform.localScale = new Vector3(emitScale, emitScale, 1f);
            }

            // 4. Crackling Pixel Arc Lightning Animation (Cycles frames every 0.06s)
            lightningTimer += Time.deltaTime;
            if (lightningTimer >= 0.06f)
            {
                lightningTimer = 0f;
                currentLightningFrame = (currentLightningFrame + 1) % pixelLightningSprites.Length;

                for (var i = 0; i < lightningArcs.Count; i++)
                {
                    var arc = lightningArcs[i];
                    if (arc != null)
                    {
                        arc.sprite = pixelLightningSprites[(currentLightningFrame + i) % pixelLightningSprites.Length];
                        // Random jitter
                        arc.transform.localPosition = new Vector3(
                            arc.transform.localPosition.x,
                            (arc.flipY ? -1f : 1f) * (steppedHeight * 0.42f + (Random.value * 0.08f)),
                            0f
                        );
                    }
                }
            }

            // 5. Impact Head Pulsing
            if (impactTransform != null)
            {
                var impScale = 1.05f + ((pulsePhase % 2 == 0) ? 0.22f : -0.1f);
                impactTransform.localScale = new Vector3(impScale * 0.85f, impScale * 1.25f, 1f);
            }
        }

        private void ProcessDestructionTicks()
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer > 0f)
                return;

            damageTimer = tickInterval;

            var center = (Vector2)(transform.position + transform.right * (beamLength * 0.5f));
            var size = new Vector2(beamLength, 1.15f);
            var angle = transform.eulerAngles.z;

            var hits = Physics2D.OverlapBoxAll(center, size, angle);
            for (var i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;

                if (h.GetComponent<PlayerHealth>() != null ||
                    h.GetComponentInParent<PlayerHealth>() != null ||
                    h.GetComponent<PlayerController>() != null ||
                    h.GetComponent<Projectile>() != null ||
                    h.GetComponent<EXMissile>() != null ||
                    h.GetComponent<SuperProjectile>() != null)
                {
                    continue;
                }

                var hitPoint = h.ClosestPoint(transform.position);

                if (h.TryGetComponent<Enemy>(out var enemy))
                {
                    EffectsManager.PlayImpact(hitPoint, Vector2.right, new Color(1f, 0.45f, 0.15f));
                    enemy.TakeDamage(tickDamage, hitPoint);
                }
                else if (h.TryGetComponent<Asteroid>(out var asteroid))
                {
                    EffectsManager.PlayImpact(asteroid.transform.position, Vector2.right, Color.yellow);
                    EffectsManager.PlayExplosion(asteroid.transform.position, Color.yellow, isMechanical: false);
                    GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                    Destroy(asteroid.gameObject);
                }
                else if (h.TryGetComponent<Missile>(out var missile))
                {
                    EffectsManager.PlayExplosion(missile.transform.position, Color.yellow, isMechanical: true);
                    GameRules.NotifyMissileDestroyed();
                    Destroy(missile.gameObject);
                }
                else if (h.TryGetComponent<EnemyProjectile>(out var ep))
                {
                    EffectsManager.PlayImpact(ep.transform.position, Vector2.right, Color.cyan);
                    Destroy(ep.gameObject);
                }
                else if (h.TryGetComponent<ParryHazard>(out var parry))
                {
                    EffectsManager.PlayParry(parry.transform.position);
                    GameRules.NotifyParry();
                    Destroy(parry.gameObject);
                }
                else if (h.TryGetComponent<BossController>(out var boss))
                {
                    EffectsManager.PlayImpact(hitPoint, Vector2.right, new Color(1f, 0.85f, 0.3f));
                    boss.TakeDamage(tickDamage + 3);
                }
            }
        }

        private void CollapseLaser()
        {
            EffectsManager.PlayExplosion(transform.position, new Color(1f, 0.25f, 0.15f), isMechanical: false);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(tickDamage * 2, other.transform.position);
            }
            else if (other.TryGetComponent<Asteroid>(out var asteroid))
            {
                GameRules.NotifyAsteroidDestroyed(1, grantHype: false);
                Destroy(asteroid.gameObject);
            }
            else if (other.TryGetComponent<EnemyProjectile>(out var ep))
            {
                Destroy(ep.gameObject);
            }
            else if (other.TryGetComponent<Missile>(out var missile))
            {
                GameRules.NotifyMissileDestroyed();
                Destroy(missile.gameObject);
            }
        }
    }
}
