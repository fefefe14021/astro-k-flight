// Assets/Scripts/UI/PixelUiAtlas.cs
// Namespace: KANYE.UI

using UnityEngine;

namespace KANYE.UI
{
    /// <summary>
    /// Provides pixel-perfect 9-sliced retro arcade UI sprites and textures.
    /// Ensures that UI frames, buttons, and panels NEVER distort or stretch.
    /// </summary>
    public static class PixelUiAtlas
    {
        private static Sprite panelSprite;
        private static Sprite subPanelSprite;
        private static Sprite buttonPrimarySprite;
        private static Sprite buttonSecondarySprite;
        private static Sprite buttonDangerSprite;
        private static Sprite whitePixel;

        public static Sprite Panel
        {
            get
            {
                if (panelSprite != null) return panelSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/pixel_panel");
                if (res != null && res.border.sqrMagnitude > 0)
                    return panelSprite = res;
                return panelSprite = CreateProceduralPanel();
            }
        }

        public static Sprite SubPanel
        {
            get
            {
                if (subPanelSprite != null) return subPanelSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/pixel_subpanel");
                if (res != null && res.border.sqrMagnitude > 0)
                    return subPanelSprite = res;
                return subPanelSprite = CreateProceduralSubPanel();
            }
        }

        public static Sprite ButtonPrimary
        {
            get
            {
                if (buttonPrimarySprite != null) return buttonPrimarySprite;
                var res = Resources.Load<Sprite>("Sprites/UI/pixel_btn_primary");
                if (res != null && res.border.sqrMagnitude > 0)
                    return buttonPrimarySprite = res;
                return buttonPrimarySprite = CreateProceduralButtonPrimary();
            }
        }

        public static Sprite ButtonSecondary
        {
            get
            {
                if (buttonSecondarySprite != null) return buttonSecondarySprite;
                var res = Resources.Load<Sprite>("Sprites/UI/pixel_btn_secondary");
                if (res != null && res.border.sqrMagnitude > 0)
                    return buttonSecondarySprite = res;
                return buttonSecondarySprite = CreateProceduralButtonSecondary();
            }
        }

        public static Sprite ButtonDanger
        {
            get
            {
                if (buttonDangerSprite != null) return buttonDangerSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/pixel_btn_danger");
                if (res != null && res.border.sqrMagnitude > 0)
                    return buttonDangerSprite = res;
                return buttonDangerSprite = CreateProceduralButtonDanger();
            }
        }

        public static Sprite White
        {
            get
            {
                if (whitePixel != null) return whitePixel;
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                for (var y = 0; y < 2; y++)
                    for (var x = 0; x < 2; x++)
                        tex.SetPixel(x, y, Color.white);
                tex.Apply();
                return whitePixel = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
            }
        }

        public static Sprite Logo => Resources.Load<Sprite>("Sprites/UI/menu_logo");
        public static Sprite Background => Resources.Load<Sprite>("Sprites/UI/menu_background");
        public static Sprite Ship => Resources.Load<Sprite>("Sprites/ship");

        private static Sprite CreateProceduralPanel()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var bg = new Color(0.04f, 0.07f, 0.12f, 0.94f);        // Deep dark navy
            var border = new Color(0.22f, 0.88f, 1f, 1f);          // Neon cyber cyan
            var inner = new Color(0.08f, 0.24f, 0.38f, 1f);        // Inner bevel
            var corner = new Color(0.6f, 0.96f, 1f, 1f);           // Highlighted corner

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    // Chamfer corners
                    var isCorner = (x < 2 && y < 2 && x + y < 2) ||
                                   (x >= size - 2 && y < 2 && (size - 1 - x) + y < 2) ||
                                   (x < 2 && y >= size - 2 && x + (size - 1 - y) < 2) ||
                                   (x >= size - 2 && y >= size - 2 && (size - 1 - x) + (size - 1 - y) < 2);
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                        tex.SetPixel(x, y, border);
                    else if ((x == 1 && y == 1) || (x == size - 2 && y == 1) || (x == 1 && y == size - 2) || (x == size - 2 && y == size - 2))
                        tex.SetPixel(x, y, corner);
                    else if (x == 1 || x == size - 2 || y == 1 || y == size - 2)
                        tex.SetPixel(x, y, inner);
                    else
                        tex.SetPixel(x, y, bg);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        private static Sprite CreateProceduralSubPanel()
        {
            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var bg = new Color(0.05f, 0.08f, 0.14f, 0.85f);
            var border = new Color(0.18f, 0.35f, 0.52f, 0.75f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                        tex.SetPixel(x, y, border);
                    else
                        tex.SetPixel(x, y, bg);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(4, 4, 4, 4));
        }

        private static Sprite CreateProceduralButtonPrimary()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var topHighlight = new Color(1f, 0.86f, 0.32f, 1f);
            var fill = new Color(0.92f, 0.44f, 0.05f, 1f);
            var bottomShadow = new Color(0.55f, 0.16f, 0.02f, 1f);
            var border = new Color(1f, 0.58f, 0.08f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if ((x == 0 || x == size - 1) && (y == 0 || y == size - 1))
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y == size - 1)
                        tex.SetPixel(x, y, topHighlight);
                    else if (y == 0)
                        tex.SetPixel(x, y, bottomShadow);
                    else if (x == 0 || x == size - 1)
                        tex.SetPixel(x, y, border);
                    else if (y == size - 2)
                        tex.SetPixel(x, y, new Color(1f, 0.72f, 0.2f, 1f));
                    else if (y == 1)
                        tex.SetPixel(x, y, new Color(0.68f, 0.22f, 0.04f, 1f));
                    else
                        tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        private static Sprite CreateProceduralButtonSecondary()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var topHighlight = new Color(0.28f, 0.85f, 1f, 1f);
            var fill = new Color(0.09f, 0.22f, 0.36f, 1f);
            var bottomShadow = new Color(0.04f, 0.09f, 0.16f, 1f);
            var border = new Color(0.18f, 0.5f, 0.75f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if ((x == 0 || x == size - 1) && (y == 0 || y == size - 1))
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y == size - 1)
                        tex.SetPixel(x, y, topHighlight);
                    else if (y == 0)
                        tex.SetPixel(x, y, bottomShadow);
                    else if (x == 0 || x == size - 1)
                        tex.SetPixel(x, y, border);
                    else if (y == size - 2)
                        tex.SetPixel(x, y, new Color(0.2f, 0.62f, 0.85f, 1f));
                    else if (y == 1)
                        tex.SetPixel(x, y, new Color(0.06f, 0.14f, 0.24f, 1f));
                    else
                        tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        private static Sprite CreateProceduralButtonDanger()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var topHighlight = new Color(1f, 0.42f, 0.52f, 1f);
            var fill = new Color(0.62f, 0.1f, 0.18f, 1f);
            var bottomShadow = new Color(0.32f, 0.04f, 0.08f, 1f);
            var border = new Color(0.85f, 0.18f, 0.28f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if ((x == 0 || x == size - 1) && (y == 0 || y == size - 1))
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y == size - 1)
                        tex.SetPixel(x, y, topHighlight);
                    else if (y == 0)
                        tex.SetPixel(x, y, bottomShadow);
                    else if (x == 0 || x == size - 1)
                        tex.SetPixel(x, y, border);
                    else if (y == size - 2)
                        tex.SetPixel(x, y, new Color(0.88f, 0.24f, 0.35f, 1f));
                    else if (y == 1)
                        tex.SetPixel(x, y, new Color(0.42f, 0.06f, 0.12f, 1f));
                    else
                        tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }
    }
}
