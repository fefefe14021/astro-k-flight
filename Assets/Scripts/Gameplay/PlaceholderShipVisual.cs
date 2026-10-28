using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderShipVisual : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(0.2f, 0.85f, 1f, 1f);
        [SerializeField] private Vector2 sizeWorldUnits = new Vector2(1.2f, 1.4f);

        private void Awake()
        {
            if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 20f)
                transform.position = Vector3.zero;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            var ship = Resources.Load<Sprite>("Sprites/ship");
            if (ship != null)
                spriteRenderer.sprite = ship;
            else if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = CreateFallbackSprite();

            UseSpriteMaterial(spriteRenderer);
            transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        }

        internal static void UseSpriteMaterial(SpriteRenderer spriteRenderer)
        {
            var shaderName = spriteRenderer.sharedMaterial != null && spriteRenderer.sharedMaterial.shader != null
                ? spriteRenderer.sharedMaterial.shader.name
                : string.Empty;
            if (shaderName == "Sprites/Default")
                return;

            var spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
                spriteRenderer.sharedMaterial = new Material(spriteShader);
        }

        private Sprite CreateFallbackSprite()
        {
            var texture = new Texture2D(4, 2);
            var pixels = new Color[8];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            var pixelsPerUnit = 4f / sizeWorldUnits.x;
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 2f), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

    }
}
