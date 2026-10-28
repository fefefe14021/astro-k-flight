using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class ShipSkin
    {
        public string Id;
        public string Title;
        public int Price;
        public Color Color;

        public ShipSkin(string id, string title, int price, Color color)
        {
            Id = id;
            Title = title;
            Price = price;
            Color = color;
        }
    }

    public static class ShipSkinCatalog
    {
        public static readonly ShipSkin[] All =
        {
            new ShipSkin("default", "Базовый корпус", 0, new Color(0.2f, 0.85f, 1f, 1f)),
            new ShipSkin("ember", "Угольный след", 30, new Color(1f, 0.45f, 0.2f, 1f)),
            new ShipSkin("violet", "Сигнальный", 50, new Color(0.62f, 0.35f, 1f, 1f))
        };

        public static ShipSkin Find(string id)
        {
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].Id == id)
                    return All[i];
            }

            return All[0];
        }

        public static void Apply(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
                return;

            var id = SaveManager.Instance != null ? SaveManager.Instance.Data.equippedSkinId : SaveManager.DefaultSkinId;
            spriteRenderer.color = Find(id).Color;
        }
    }
}
