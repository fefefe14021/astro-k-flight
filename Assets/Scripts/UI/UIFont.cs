using UnityEngine;

namespace KANYE.UI
{
    public static class UIFont
    {
        private static Font s_cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (s_cachedFont != null)
                    return s_cachedFont;

                // 1. Primary: load font asset bundled in Resources (guaranteed to be included in WebGL build)
                s_cachedFont = Resources.Load<Font>("Fonts/GameFont");
                if (s_cachedFont != null)
                    return s_cachedFont;

                s_cachedFont = Resources.Load<Font>("GameFont");
                if (s_cachedFont != null)
                    return s_cachedFont;

                // 2. Fallbacks for Unity Editor or environments with built-in fonts
                s_cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (s_cachedFont != null)
                    return s_cachedFont;

                s_cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (s_cachedFont != null)
                    return s_cachedFont;

                // 3. Fallback search across loaded font objects
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0)
                {
                    s_cachedFont = fonts[0];
                    return s_cachedFont;
                }

                return null;
            }
        }
    }
}
