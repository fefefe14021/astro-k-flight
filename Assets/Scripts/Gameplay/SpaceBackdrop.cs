using UnityEngine;

namespace KANYE.Gameplay
{
    public class SpaceBackdrop : MonoBehaviour
    {
        private void Awake()
        {
            var camera = Camera.main;
            if (camera != null)
                camera.backgroundColor = new Color(0.015f, 0.02f, 0.06f, 1f);

            var height = camera != null ? camera.orthographicSize * 2f : 10f;
            var width = camera != null ? height * camera.aspect : 18f;
            var bounds = new Rect(-width * 0.7f, -height * 0.7f, width * 1.4f, height * 1.4f);

            CreateDust(bounds);
            CreateStarLayer("StarsFar", bounds, 90, 0.12f, 1, new Color(0.45f, 0.55f, 0.75f, 0.7f), -30);
            CreateStarLayer("StarsMid", bounds, 48, 0.28f, 2, new Color(0.75f, 0.82f, 0.95f, 0.9f), -20);
            CreateStarLayer("StarsNear", bounds, 22, 0.55f, 3, Color.white, -10);
            CreatePlanet(bounds, new Vector3(bounds.xMax - 2.2f, 2.4f, 0f), 1.6f, new Color(0.35f, 0.28f, 0.55f), 0.08f, -15);
            CreatePlanet(bounds, new Vector3(bounds.xMin + 3.5f, -2.8f, 0f), 0.7f, new Color(0.85f, 0.45f, 0.28f), 0.18f, -12);
        }

        private void CreateDust(Rect bounds)
        {
            var dust = new GameObject("Dust");
            dust.transform.SetParent(transform, false);
            var renderer = dust.AddComponent<SpriteRenderer>();
            renderer.sprite = MakeBlobSprite(48, new Color(0.12f, 0.16f, 0.38f, 0.35f));
            renderer.sortingOrder = -40;
            dust.transform.localScale = new Vector3(bounds.width * 0.9f, bounds.height * 0.55f, 1f);
            dust.transform.position = new Vector3(0f, 0.6f, 0f);
        }

        private void CreateStarLayer(string name, Rect bounds, int count, float speed, int pixelSize, Color color, int sortingOrder)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(transform, false);
            var scroller = layer.AddComponent<StarScroller>();
            scroller.speed = speed;
            scroller.minX = bounds.xMin;
            scroller.maxX = bounds.xMax;

            var sprite = MakeStarSprite(pixelSize, color);
            var random = new System.Random(name.GetHashCode());
            for (var i = 0; i < count; i++)
            {
                var star = new GameObject("Star");
                star.transform.SetParent(layer.transform, false);
                var x = bounds.xMin + (float)random.NextDouble() * bounds.width;
                var y = bounds.yMin + (float)random.NextDouble() * bounds.height;
                star.transform.position = new Vector3(x, y, 0f);
                var renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
            }
        }

        private void CreatePlanet(Rect bounds, Vector3 position, float size, Color color, float speed, int sortingOrder)
        {
            var planet = new GameObject("Planet");
            planet.transform.SetParent(transform, false);
            planet.transform.position = position;
            planet.transform.localScale = Vector3.one * size;
            var renderer = planet.AddComponent<SpriteRenderer>();
            renderer.sprite = MakePlanetSprite(color);
            renderer.sortingOrder = sortingOrder;

            var scroller = planet.AddComponent<StarScroller>();
            scroller.speed = speed;
            scroller.minX = bounds.xMin - size;
            scroller.maxX = bounds.xMax + size;
            scroller.wrapSingle = true;
        }

        private static Sprite MakeStarSprite(int pixelSize, Color color)
        {
            var texture = new Texture2D(pixelSize, pixelSize);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[pixelSize * pixelSize];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, pixelSize, pixelSize), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite MakeBlobSprite(int size, Color color)
        {
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / center;
                    var dy = (y - center) / center;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01(1f - distance) * color.a;
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite MakePlanetSprite(Color color)
        {
            const int size = 24;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            var center = (size - 1) * 0.5f;
            var radius = size * 0.42f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    if (dx * dx + dy * dy > radius * radius)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    var shade = dx < -2f ? 0.72f : 1f;
                    var crater = (x + y) % 7 == 0 && dx * dx + dy * dy < radius * radius * 0.55f;
                    var rgb = crater ? color * 0.55f : color * shade;
                    pixels[y * size + x] = new Color(rgb.r, rgb.g, rgb.b, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    public class StarScroller : MonoBehaviour
    {
        public float speed = 0.2f;
        public float minX = -12f;
        public float maxX = 12f;
        public bool wrapSingle;

        private void Update()
        {
            var delta = Vector3.left * speed * Time.deltaTime;
            if (wrapSingle)
            {
                transform.position += delta;
                if (transform.position.x < minX)
                {
                    var position = transform.position;
                    position.x = maxX;
                    transform.position = position;
                }

                return;
            }

            transform.position += delta;
            var span = maxX - minX;
            for (var i = 0; i < transform.childCount; i++)
            {
                var star = transform.GetChild(i);
                if (star.position.x >= minX)
                    continue;

                var position = star.position;
                position.x += span;
                star.position = position;
            }
        }
    }
}
