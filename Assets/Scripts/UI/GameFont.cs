using UnityEngine;

namespace KANYE.UI
{
    public static class GameFont
    {
        private static Font font;

        public static Font Get()
        {
            if (font != null)
                return font;

            // 1. Primary authentic game font (Liberation Sans Bold from original release)
            font = Resources.Load<Font>("Fonts/GameFont");
            if (font == null)
                font = Resources.Load<Font>("GameFont");
            if (font == null)
                font = Resources.Load<Font>("Fonts/NotoSans-Regular");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Ensure crisp filtering on dynamic font atlas texture
            if (font != null && font.material != null && font.material.mainTexture != null)
                font.material.mainTexture.filterMode = FilterMode.Bilinear;

            return font;
        }
    }

    public static class UIFont
    {
        public static Font DefaultFont => GameFont.Get();
    }
}
