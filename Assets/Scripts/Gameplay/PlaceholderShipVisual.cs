// Assets/Scripts/Gameplay/PlaceholderShipVisual.cs
// Namespace: KANYE.Gameplay
// GameObject: Player

using UnityEngine;

namespace KANYE.Gameplay
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderShipVisual : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;
        private static Sprite cachedShipSprite;

        private void Awake()
        {
            EnsureShipSetup();
        }

        private void OnEnable()
        {
            EnsureShipSetup();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            EnsureShipSetup();
        }
#endif

        public void EnsureShipSetup()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            spriteRenderer.sortingOrder = 10;

            // Load sleek delta starfighter facing RIGHT
            var loaded = Resources.Load<Sprite>("Sprites/ship");
            if (loaded != null && loaded.rect.width > 2)
            {
                spriteRenderer.sprite = loaded;
            }
            else
            {
                if (cachedShipSprite == null)
                    cachedShipSprite = CreateFallbackShipSprite();

                spriteRenderer.sprite = cachedShipSprite;
            }

            UseSpriteMaterial(spriteRenderer);

            if (Application.isPlaying)
            {
                // Ship naturally faces RIGHT (+X) at identity rotation
                transform.rotation = Quaternion.identity;
                ShipSkinCatalog.Apply(spriteRenderer);

                if (GetComponent<ShipThrusterVisual>() == null)
                    gameObject.AddComponent<ShipThrusterVisual>();
            }
        }

        internal static void UseSpriteMaterial(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null) return;

            var shaderName = spriteRenderer.sharedMaterial != null && spriteRenderer.sharedMaterial.shader != null
                ? spriteRenderer.sharedMaterial.shader.name
                : string.Empty;

            if (shaderName == "Sprites/Default")
                return;

            var spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
                spriteRenderer.sharedMaterial = new Material(spriteShader);
        }

        /// <summary>
        /// Generates a sleek delta-wing starfighter sprite facing RIGHT (+X).
        /// </summary>
        public static Sprite CreateFallbackShipSprite()
        {
            var path = System.IO.Path.Combine(Application.dataPath, "Resources/Sprites/ship.png");
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var bytes = System.IO.File.ReadAllBytes(path);
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (ImageConversion.LoadImage(tex, bytes))
                    {
                        tex.filterMode = FilterMode.Point;
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 48f);
                    }
                }
                catch
                {
                    // Fall through to procedural generation
                }
            }

            // Procedural sleek delta starfighter facing RIGHT (64x50)
            const int w = 64;
            const int h = 50;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            var hull = new Color(0f, 0.15f, 0.44f, 1f);
            var glass = new Color(0.98f, 0.99f, 1f, 1f);
            var wing = new Color(0.14f, 0.4f, 0.72f, 1f);

            for (var x = 0; x < w; x++)
            {
                var normX = x / (float)w;
                var halfH = Mathf.Lerp(24f, 2f, normX * normX);
                for (var y = 0; y < h; y++)
                {
                    var dist = Mathf.Abs(y - 24.5f);
                    if (dist <= halfH)
                    {
                        var isCockpit = normX >= 0.5f && normX <= 0.8f && dist <= 3f;
                        var isWingEdge = dist >= halfH - 3f;
                        pixels[y * w + x] = isCockpit ? glass : isWingEdge ? wing : hull;
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 48f);
        }
    }
}
