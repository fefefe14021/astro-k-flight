// Assets/Scripts/Gameplay/SpaceBackdrop.cs
// Namespace: KANYE.Gameplay
// GameObject: SpaceBackdrop

using UnityEngine;

namespace KANYE.Gameplay
{
    public class SpaceBackdrop : MonoBehaviour
    {
        private void Awake()
        {
            var camera = Camera.main;
            if (camera != null)
                camera.backgroundColor = new Color(0.015f, 0.018f, 0.045f, 1f);

            var height = camera != null ? camera.orthographicSize * 2f : 10f;
            var width = camera != null ? height * camera.aspect : 18f;
            var bounds = new Rect(-width * 0.75f, -height * 0.75f, width * 1.5f, height * 1.5f);

            CreateNebulae(bounds);
            CreateStarLayer("StarsFar", bounds, 80, 0.15f, 1, new Color(0.5f, 0.65f, 0.9f, 0.65f), -35, false);
            CreateStarLayer("StarsMid", bounds, 40, 0.45f, 2, new Color(0.85f, 0.9f, 1f, 0.85f), -25, true);
            CreateStarLayer("StarsNear", bounds, 20, 0.85f, 3, new Color(1f, 0.95f, 0.85f, 0.95f), -15, true);
            
            // Terraria/Broforce style high-speed space streaks in foreground
            CreateSpeedStreakLayer(bounds, 18, 5.5f, -5);

            // Celestial bodies with glowing rims
            CreatePlanet(bounds, new Vector3(bounds.xMax - 3.2f, 2.5f, 0f), 2.2f, new Color(0.42f, 0.22f, 0.65f), 0.06f, -30, true);
            CreatePlanet(bounds, new Vector3(bounds.xMin + 4.5f, -2.6f, 0f), 1.1f, new Color(0.2f, 0.65f, 0.75f), 0.14f, -20, false);
        }

        private void CreateNebulae(Rect bounds)
        {
            // Cyberpunk cosmic haze: rich purple & cyan clouds
            CreateNebulaPatch("NebulaViolet", new Vector3(-bounds.width * 0.2f, bounds.height * 0.25f, 0f),
                new Vector3(bounds.width * 0.85f, bounds.height * 0.65f, 1f),
                new Color(0.25f, 0.08f, 0.42f, 0.28f), -42);

            CreateNebulaPatch("NebulaCyan", new Vector3(bounds.width * 0.25f, -bounds.height * 0.2f, 0f),
                new Vector3(bounds.width * 0.9f, bounds.height * 0.55f, 1f),
                new Color(0.05f, 0.28f, 0.48f, 0.24f), -41);
        }

        private void CreateNebulaPatch(string name, Vector3 pos, Vector3 scale, Color color, int order)
        {
            var nebula = new GameObject(name);
            nebula.transform.SetParent(transform, false);
            var renderer = nebula.AddComponent<SpriteRenderer>();
            renderer.sprite = MakeNebulaSprite(64, color);
            renderer.sortingOrder = order;
            nebula.transform.localScale = scale;
            nebula.transform.position = pos;
        }

        private void CreateStarLayer(string name, Rect bounds, int count, float speed, int pixelSize, Color color, int sortingOrder, bool canTwinkle)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(transform, false);
            var scroller = layer.AddComponent<StarScroller>();
            scroller.speed = speed;
            scroller.minX = bounds.xMin;
            scroller.maxX = bounds.xMax;

            var dotSprite = MakeStarSprite(pixelSize, color);
            var crossSprite = pixelSize >= 2 ? MakeCrossStarSprite(color) : dotSprite;
            var random = new System.Random(name.GetHashCode());

            for (var i = 0; i < count; i++)
            {
                var star = new GameObject("Star");
                star.transform.SetParent(layer.transform, false);
                var x = bounds.xMin + (float)random.NextDouble() * bounds.width;
                var y = bounds.yMin + (float)random.NextDouble() * bounds.height;
                star.transform.position = new Vector3(x, y, 0f);

                var renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = (pixelSize >= 2 && random.NextDouble() > 0.6) ? crossSprite : dotSprite;
                renderer.sortingOrder = sortingOrder;

                if (canTwinkle && random.NextDouble() > 0.5)
                {
                    var tw = star.AddComponent<StarTwinkle>();
                    tw.speed = 1.5f + (float)random.NextDouble() * 3.5f;
                    tw.baseAlpha = color.a;
                }
            }
        }

        private void CreateSpeedStreakLayer(Rect bounds, int count, float speed, int sortingOrder)
        {
            var layer = new GameObject("SpeedStreaks");
            layer.transform.SetParent(transform, false);
            var scroller = layer.AddComponent<StarScroller>();
            scroller.speed = speed;
            scroller.minX = bounds.xMin - 2f;
            scroller.maxX = bounds.xMax + 2f;

            var streakSprite = MakeStreakSprite();
            var random = new System.Random(999);

            for (var i = 0; i < count; i++)
            {
                var streak = new GameObject("Streak");
                streak.transform.SetParent(layer.transform, false);
                var x = bounds.xMin + (float)random.NextDouble() * bounds.width;
                var y = bounds.yMin + (float)random.NextDouble() * bounds.height;
                streak.transform.position = new Vector3(x, y, 0f);

                var renderer = streak.AddComponent<SpriteRenderer>();
                renderer.sprite = streakSprite;
                renderer.sortingOrder = sortingOrder;
                var alpha = 0.25f + (float)random.NextDouble() * 0.45f;
                renderer.color = new Color(0.85f, 0.95f, 1f, alpha);
            }
        }

        private void CreatePlanet(Rect bounds, Vector3 position, float size, Color color, float speed, int sortingOrder, bool hasRing)
        {
            var planet = new GameObject("Planet");
            planet.transform.SetParent(transform, false);
            planet.transform.position = position;
            planet.transform.localScale = Vector3.one * size;
            var renderer = planet.AddComponent<SpriteRenderer>();
            renderer.sprite = MakePlanetSprite(color, hasRing);
            renderer.sortingOrder = sortingOrder;

            var scroller = planet.AddComponent<StarScroller>();
            scroller.speed = speed;
            scroller.minX = bounds.xMin - size * 1.5f;
            scroller.maxX = bounds.xMax + size * 1.5f;
            scroller.wrapSingle = true;
        }

        private static Sprite MakeStreakSprite()
        {
            const int width = 14;
            const int height = 1;
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            for (var x = 0; x < width; x++)
            {
                // Tapered streak from x=0 (tail) to x=13 (head)
                var alpha = (float)x / (width - 1);
                texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha * alpha));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 16f);
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

        private static Sprite MakeCrossStarSprite(Color color)
        {
            const int size = 5;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isCenter = (x == 2 && y == 2);
                    var isRay = (x == 2 || y == 2) && (x != 0 && x != 4 && y != 0 && y != 4);
                    if (isCenter)
                        texture.SetPixel(x, y, Color.white);
                    else if (isRay)
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * 0.7f));
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 24f);
        }

        private static Sprite MakeNebulaSprite(int size, Color color)
        {
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Bilinear;
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / center;
                    var dy = (y - center) / center;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01(1f - dist);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha) * color.a;
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite MakePlanetSprite(Color color, bool hasRing)
        {
            const int size = 32;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;
            var center = (size - 1) * 0.5f;
            var radius = size * 0.38f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distSq = dx * dx + dy * dy;

                    // Ring
                    if (hasRing && Mathf.Abs(dy - dx * 0.25f) <= 1.4f && Mathf.Abs(dx) >= radius * 0.6f && Mathf.Abs(dx) <= radius * 1.35f)
                    {
                        var ringColor = new Color(0.9f, 0.8f, 0.6f, 0.85f);
                        texture.SetPixel(x, y, ringColor);
                        continue;
                    }

                    if (distSq > radius * radius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Planet body with atmospheric rim and crater shading
                    var rim = Mathf.Sqrt(distSq) / radius;
                    var light = (dx - dy * 0.4f) / radius;
                    
                    Color pixelColor;
                    if (rim > 0.88f)
                    {
                        // Atmospheric halo
                        pixelColor = Color.Lerp(color, Color.cyan, 0.6f);
                    }
                    else if (light > 0.2f)
                    {
                        // Lit side
                        pixelColor = Color.Lerp(color, Color.white, 0.35f);
                    }
                    else if (light < -0.3f)
                    {
                        // Shadow side
                        pixelColor = color * 0.45f;
                    }
                    else
                    {
                        pixelColor = color;
                    }

                    // Craters / surface stripes
                    if ((x + y * 2) % 11 == 0 && distSq < radius * radius * 0.7f)
                    {
                        pixelColor *= 0.65f;
                    }

                    pixelColor.a = 1f;
                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    public class StarTwinkle : MonoBehaviour
    {
        public float speed = 2f;
        public float baseAlpha = 1f;
        private SpriteRenderer sr;
        private float seed;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            seed = Random.Range(0f, 10f);
        }

        private void Update()
        {
            if (sr == null) return;
            var alpha = baseAlpha * (0.6f + Mathf.Sin((Time.time + seed) * speed) * 0.4f);
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
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
