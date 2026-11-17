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
            const int size = 64;
            const int border = 14;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var bgTop = new Color(0.04f, 0.08f, 0.15f, 0.96f);       // Deep dark cyber navy
            var bgBot = new Color(0.02f, 0.04f, 0.08f, 0.98f);
            var borderColor = new Color(0f, 0.9f, 1f, 1f);           // Neon cyber cyan
            var innerBevel = new Color(0.08f, 0.24f, 0.4f, 0.8f);     // Bevel

            for (var y = 0; y < size; y++)
            {
                var t = (float)y / size;
                var rowBg = Color.Lerp(bgBot, bgTop, t);

                for (var x = 0; x < size; x++)
                {
                    // Chamfer corners (cut 5px)
                    var isCorner = (x < 5 && y < 5 && (x + y < 5)) ||
                                   (x >= size - 5 && y < 5 && ((size - 1 - x) + y < 5)) ||
                                   (x < 5 && y >= size - 5 && (x + (size - 1 - y) < 5)) ||
                                   (x >= size - 5 && y >= size - 5 && ((size - 1 - x) + (size - 1 - y) < 5));
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (x <= 1 || x >= size - 2 || y <= 1 || y >= size - 2)
                        tex.SetPixel(x, y, borderColor);
                    else if (x == 2 || x == size - 3 || y == 2 || y == size - 3)
                        tex.SetPixel(x, y, innerBevel);
                    else
                        tex.SetPixel(x, y, rowBg);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite CreateProceduralSubPanel()
        {
            const int size = 48;
            const int border = 12;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var bgTop = new Color(0.05f, 0.09f, 0.16f, 0.92f);
            var bgBot = new Color(0.03f, 0.05f, 0.10f, 0.95f);
            var borderColor = new Color(0.12f, 0.26f, 0.42f, 1f);
            var accentCyan = new Color(0f, 0.9f, 1f, 1f);

            for (var y = 0; y < size; y++)
            {
                var t = (float)y / size;
                var rowBg = Color.Lerp(bgBot, bgTop, t);

                for (var x = 0; x < size; x++)
                {
                    var isCorner = (x < 4 && y < 4 && (x + y < 4)) ||
                                   (x >= size - 4 && y < 4 && ((size - 1 - x) + y < 4)) ||
                                   (x < 4 && y >= size - 4 && (x + (size - 1 - y) < 4)) ||
                                   (x >= size - 4 && y >= size - 4 && ((size - 1 - x) + (size - 1 - y) < 4));
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                        tex.SetPixel(x, y, borderColor);
                    else if (y == size - 2 && (x < 10 || x > size - 11))
                        tex.SetPixel(x, y, accentCyan); // Tech corner highlight
                    else
                        tex.SetPixel(x, y, rowBg);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite CreateProceduralButtonPrimary()
        {
            const int size = 64;
            const int border = 14;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var gradTop = new Color(1f, 0.62f, 0f, 1f);         // Vivid Amber
            var gradBot = new Color(0.82f, 0.28f, 0f, 1f);       // Deep Orange
            var topHighlight = new Color(1f, 0.88f, 0.5f, 1f);   // Gold Highlight
            var bottomShadow = new Color(0.4f, 0.1f, 0f, 1f);
            var borderCol = new Color(1f, 0.55f, 0.05f, 1f);

            for (var y = 0; y < size; y++)
            {
                var t = (float)y / size;
                var rowFill = Color.Lerp(gradBot, gradTop, t);

                for (var x = 0; x < size; x++)
                {
                    var isCorner = (x < 5 && y < 5 && (x + y < 5)) ||
                                   (x >= size - 5 && y < 5 && ((size - 1 - x) + y < 5)) ||
                                   (x < 5 && y >= size - 5 && (x + (size - 1 - y) < 5)) ||
                                   (x >= size - 5 && y >= size - 5 && ((size - 1 - x) + (size - 1 - y) < 5));
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y >= size - 2)
                        tex.SetPixel(x, y, topHighlight);
                    else if (y <= 1)
                        tex.SetPixel(x, y, bottomShadow);
                    else if (x <= 1 || x >= size - 2)
                        tex.SetPixel(x, y, borderCol);
                    else
                        tex.SetPixel(x, y, rowFill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite CreateProceduralButtonSecondary()
        {
            const int size = 64;
            const int border = 14;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var gradTop = new Color(0.06f, 0.12f, 0.22f, 0.96f);
            var gradBot = new Color(0.02f, 0.05f, 0.10f, 0.98f);
            var topHighlight = new Color(0.2f, 0.85f, 1f, 1f);
            var borderCol = new Color(0f, 0.9f, 1f, 1f);

            for (var y = 0; y < size; y++)
            {
                var t = (float)y / size;
                var rowFill = Color.Lerp(gradBot, gradTop, t);

                for (var x = 0; x < size; x++)
                {
                    var isCorner = (x < 5 && y < 5 && (x + y < 5)) ||
                                   (x >= size - 5 && y < 5 && ((size - 1 - x) + y < 5)) ||
                                   (x < 5 && y >= size - 5 && (x + (size - 1 - y) < 5)) ||
                                   (x >= size - 5 && y >= size - 5 && ((size - 1 - x) + (size - 1 - y) < 5));
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y >= size - 2)
                        tex.SetPixel(x, y, topHighlight);
                    else if (x <= 1 || x >= size - 2 || y <= 1)
                        tex.SetPixel(x, y, borderCol);
                    else
                        tex.SetPixel(x, y, rowFill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite CreateProceduralButtonDanger()
        {
            const int size = 64;
            const int border = 14;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var gradTop = new Color(0.22f, 0.05f, 0.08f, 0.96f);
            var gradBot = new Color(0.08f, 0.02f, 0.03f, 0.98f);
            var topHighlight = new Color(1f, 0.45f, 0.45f, 1f);
            var borderCol = new Color(1f, 0.25f, 0.25f, 1f);

            for (var y = 0; y < size; y++)
            {
                var t = (float)y / size;
                var rowFill = Color.Lerp(gradBot, gradTop, t);

                for (var x = 0; x < size; x++)
                {
                    var isCorner = (x < 5 && y < 5 && (x + y < 5)) ||
                                   (x >= size - 5 && y < 5 && ((size - 1 - x) + y < 5)) ||
                                   (x < 5 && y >= size - 5 && (x + (size - 1 - y) < 5)) ||
                                   (x >= size - 5 && y >= size - 5 && ((size - 1 - x) + (size - 1 - y) < 5));
                    if (isCorner)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (y >= size - 2)
                        tex.SetPixel(x, y, topHighlight);
                    else if (x <= 1 || x >= size - 2 || y <= 1)
                        tex.SetPixel(x, y, borderCol);
                    else
                        tex.SetPixel(x, y, rowFill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }
    }
}
