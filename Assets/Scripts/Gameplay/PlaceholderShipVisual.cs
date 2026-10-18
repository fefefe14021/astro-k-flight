using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderShipVisual : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(0.2f, 0.85f, 1f, 1f);
        [SerializeField] private Vector2 sizeWorldUnits = new Vector2(1.2f, 0.6f);

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite != null)
                return;

            var texture = new Texture2D(4, 2);
            var pixels = new Color[8];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            var pixelsPerUnit = 4f / sizeWorldUnits.x;
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 2f), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
