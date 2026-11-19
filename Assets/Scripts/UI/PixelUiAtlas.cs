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
        private static Sprite iconHeartFullSprite;
        private static Sprite iconHeartEmptySprite;
        private static Sprite iconCreditSprite;
        private static Sprite iconStarCoreSprite;
        private static Sprite iconChestStdSprite;
        private static Sprite iconChestEpicSprite;

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

        public static Sprite IconHeartFull
        {
            get
            {
                if (iconHeartFullSprite != null) return iconHeartFullSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_heart_full");
                if (res != null) return iconHeartFullSprite = res;
                return iconHeartFullSprite = CreateProceduralIconHeart(true);
            }
        }

        public static Sprite IconHeartEmpty
        {
            get
            {
                if (iconHeartEmptySprite != null) return iconHeartEmptySprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_heart_empty");
                if (res != null) return iconHeartEmptySprite = res;
                return iconHeartEmptySprite = CreateProceduralIconHeart(false);
            }
        }

        public static Sprite IconCredit
        {
            get
            {
                if (iconCreditSprite != null) return iconCreditSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_credit");
                if (res != null) return iconCreditSprite = res;
                return iconCreditSprite = CreateProceduralIconCredit();
            }
        }

        public static Sprite IconStarCore
        {
            get
            {
                if (iconStarCoreSprite != null) return iconStarCoreSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_star_core");
                if (res != null) return iconStarCoreSprite = res;
                return iconStarCoreSprite = CreateProceduralIconStarCore();
            }
        }

        public static Sprite IconChestStandard
        {
            get
            {
                if (iconChestStdSprite != null) return iconChestStdSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_chest_std");
                if (res != null) return iconChestStdSprite = res;
                return iconChestStdSprite = CreateProceduralIconChestStd();
            }
        }

        public static Sprite IconChestEpic
        {
            get
            {
                if (iconChestEpicSprite != null) return iconChestEpicSprite;
                var res = Resources.Load<Sprite>("Sprites/UI/icon_chest_epic");
                if (res != null) return iconChestEpicSprite = res;
                return iconChestEpicSprite = CreateProceduralIconChestEpic();
            }
        }

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

        private static Sprite CreateProceduralIconHeart(bool full)
        {
            const int s = 16;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            string[] rows = new string[]
            {
                "................",
                "...xxxx...xxxx..",
                "..xxxxxx.xxxxxx.",
                ".xxxxxxxxxxxxxx.",
                ".xxxxxxxxxxxxxx.",
                ".xxxxxxxxxxxxxx.",
                ".xxxxxxxxxxxxxx.",
                "..xxxxxxxxxxxx..",
                "...xxxxxxxxxx...",
                "....xxxxxxxx....",
                ".....xxxxxx.....",
                "......xxxx......",
                ".......xx.......",
                "................",
                "................",
                "................"
            };

            for (var y = 0; y < s; y++)
            {
                var rowStr = rows[s - 1 - y];
                for (var x = 0; x < s; x++)
                {
                    if (rowStr[x] == '.')
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        var isBorder = (x <= 1 || x >= 14 || y <= 3 || y >= 14 ||
                            (x == 7 || x == 8) && y >= 13);

                        if (full)
                        {
                            if (isBorder)
                            {
                                tex.SetPixel(x, y, new Color(0.32f, 0.04f, 0.12f, 1f));
                            }
                            else if (x >= 3 && x <= 5 && y >= 11 && y <= 13)
                            {
                                tex.SetPixel(x, y, new Color(1f, 0.78f, 0.88f, 1f));
                            }
                            else if (x >= 4 && x <= 6 && y >= 9 && y <= 10)
                            {
                                tex.SetPixel(x, y, new Color(1f, 0.35f, 0.52f, 1f));
                            }
                            else
                            {
                                tex.SetPixel(x, y, new Color(1f, 0.14f, 0.32f, 1f));
                            }
                        }
                        else
                        {
                            if (isBorder)
                            {
                                tex.SetPixel(x, y, new Color(0.24f, 0.36f, 0.50f, 0.95f));
                            }
                            else
                            {
                                tex.SetPixel(x, y, new Color(0.06f, 0.10f, 0.18f, 0.75f));
                            }
                        }
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite CreateProceduralIconCredit()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(16f, 16f));
                    if (dist > 14f) tex.SetPixel(x, y, Color.clear);
                    else if (dist > 12.5f) tex.SetPixel(x, y, new Color(0.4f, 0.28f, 0.05f, 1f));
                    else if (dist > 10.5f) tex.SetPixel(x, y, new Color(1f, 0.85f, 0.2f, 1f));
                    else if (dist > 8f) tex.SetPixel(x, y, new Color(0.95f, 0.72f, 0.12f, 1f));
                    else
                    {
                        var isC = (x >= 12 && x <= 14 && y >= 11 && y <= 21) ||
                                  (x >= 14 && x <= 19 && (y >= 19 && y <= 21 || y >= 11 && y <= 13)) ||
                                  (x == 16 && (y == 9 || y == 10 || y == 22 || y == 23));
                        tex.SetPixel(x, y, isC ? new Color(1f, 0.95f, 0.7f, 1f) : new Color(0.85f, 0.6f, 0.08f, 1f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite CreateProceduralIconStarCore()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var cx = 15.5f; var cy = 15.5f;
            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    var dx = Mathf.Abs(x + 0.5f - cx);
                    var dy = Mathf.Abs(y + 0.5f - cy);
                    var starDist = dx + dy * 2.5f;
                    var starDist2 = dx * 2.5f + dy;
                    var minDist = Mathf.Min(starDist, starDist2);
                    var euclid = Mathf.Sqrt(dx * dx + dy * dy);

                    if (euclid <= 3f)
                        tex.SetPixel(x, y, Color.white);
                    else if (minDist <= 11f)
                        tex.SetPixel(x, y, Color.Lerp(new Color(0f, 0.95f, 1f, 1f), new Color(0.85f, 0.3f, 1f, 1f), minDist / 11f));
                    else if (euclid <= 12f)
                        tex.SetPixel(x, y, new Color(0.5f, 0.15f, 0.85f, Mathf.Clamp01(1f - (euclid / 12f))));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite CreateProceduralIconChestStd()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    if (x < 3 || x > 28 || y < 5 || y > 26)
                        tex.SetPixel(x, y, Color.clear);
                    else if (x == 3 || x == 28 || y == 5 || y == 26)
                        tex.SetPixel(x, y, new Color(0.08f, 0.14f, 0.22f, 1f));
                    else if (y >= 14 && y <= 17)
                        tex.SetPixel(x, y, (x >= 14 && x <= 17) ? new Color(0f, 1f, 0.95f, 1f) : new Color(0.12f, 0.22f, 0.35f, 1f));
                    else if (x <= 7 || x >= 24)
                        tex.SetPixel(x, y, new Color(0.28f, 0.42f, 0.58f, 1f));
                    else
                        tex.SetPixel(x, y, new Color(0.16f, 0.26f, 0.38f, 1f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite CreateProceduralIconChestEpic()
        {
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (var y = 0; y < s; y++)
            {
                for (var x = 0; x < s; x++)
                {
                    if (x < 2 || x > 29 || y < 4 || y > 27)
                        tex.SetPixel(x, y, Color.clear);
                    else if (x == 2 || x == 29 || y == 4 || y == 27)
                        tex.SetPixel(x, y, new Color(0.35f, 0.2f, 0.05f, 1f));
                    else if (y >= 13 && y <= 18)
                        tex.SetPixel(x, y, (x >= 13 && x <= 18) ? new Color(1f, 0.4f, 0.9f, 1f) : new Color(0.6f, 0.4f, 0.1f, 1f));
                    else if (x <= 7 || x >= 24 || y >= 22)
                        tex.SetPixel(x, y, new Color(1f, 0.85f, 0.3f, 1f));
                    else
                        tex.SetPixel(x, y, new Color(0.85f, 0.65f, 0.18f, 1f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
