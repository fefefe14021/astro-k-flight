// Assets/Scripts/Gameplay/ShipSkinCatalog.cs
// Namespace: KANYE.Gameplay
// Custom Flying Saucer hulls with visible cockpit pilot variants.

using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class ShipSkin
    {
        public string Id;
        public string Title;
        public string Subtitle;
        public int Price;
        public int CoresPrice;
        public string SpriteName;
        public string Description;
        public Color ThemeColor;
        public Color Color => ThemeColor;

        // Custom Thruster Flame & Particle System parameters
        public string ThrusterSprite;
        public string NozzleSprite;
        public Color ThrusterColor;
        public Color AntiGravColor;
        public Color ParticleColor;
        public string ParticleType;
        public int RequiredMissionIndex;

        public ShipSkin(
            string id,
            string title,
            string subtitle,
            int price,
            string spriteName,
            string description,
            Color themeColor,
            string thrusterSprite = "Sprites/thruster_ion",
            Color? thrusterColor = null,
            Color? antiGravColor = null,
            Color? particleColor = null,
            string particleType = "ion",
            int requiredMissionIndex = 0,
            int coresPrice = 0,
            string nozzleSprite = null)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            Price = price;
            CoresPrice = coresPrice;
            SpriteName = spriteName;
            Description = description;
            ThemeColor = themeColor;
            ThrusterSprite = thrusterSprite;
            ThrusterColor = thrusterColor ?? themeColor;
            AntiGravColor = antiGravColor ?? new Color(themeColor.r, themeColor.g, themeColor.b, 0.7f);
            ParticleColor = particleColor ?? themeColor;
            ParticleType = particleType;
            RequiredMissionIndex = requiredMissionIndex;

            if (!string.IsNullOrEmpty(nozzleSprite))
            {
                NozzleSprite = nozzleSprite;
            }
            else
            {
                switch (id)
                {
                    case "cyberpunk": NozzleSprite = "Sprites/engine_nozzle_cyber"; break;
                    case "gold": NozzleSprite = "Sprites/engine_nozzle_gold"; break;
                    case "bio": NozzleSprite = "Sprites/engine_nozzle_bio"; break;
                    case "steampunk": NozzleSprite = "Sprites/engine_nozzle_steam"; break;
                    case "void": NozzleSprite = "Sprites/engine_nozzle_void"; break;
                    default: NozzleSprite = "Sprites/engine_nozzle"; break;
                }
            }
        }
    }

    public static class ShipSkinCatalog
    {
        public static readonly ShipSkin[] All =
        {
            new ShipSkin(
                "default",
                "Retro Chrome 1957",
                "Классический хром",
                0,
                "Sprites/ship",
                "Классическая летающая тарелка из зеркального хрома с бирюзовым стеклянным куполом. В кабине — отважный Капитан Зигги у штурвала.",
                new Color(0.3f, 0.85f, 1f, 1f),
                "Sprites/thruster_ion",
                new Color(0.3f, 0.85f, 1f, 1f),
                new Color(0.2f, 0.8f, 1f, 0.7f),
                new Color(0.5f, 0.9f, 1f, 0.95f),
                "ion"),

            new ShipSkin(
                "cyberpunk",
                "Neon 2077 Cyber-Disc",
                "Кибер-диск Синдиката",
                160,
                "Sprites/ship_cyberpunk",
                "Неоновый ночной перехватчик с темно-фиолетовым композитным диском, неоново-розовыми огнями и пилотом в кибер-визоре.",
                new Color(1f, 0.2f, 0.75f, 1f),
                "Sprites/thruster_cyber",
                new Color(1f, 0.25f, 0.8f, 1f),
                new Color(1f, 0.15f, 0.7f, 0.75f),
                new Color(0.95f, 0.3f, 1f, 0.95f),
                "cyber",
                3),

            new ShipSkin(
                "gold",
                "Solar Flare VIP",
                "Золотой VIP-шаттл",
                400,
                "Sprites/ship_gold",
                "Роскошная тарелка из 24-каратного сплава со сверкающим янтарным куполом и пилотом в стильных темных очках.",
                new Color(1f, 0.85f, 0.2f, 1f),
                "Sprites/thruster_gold",
                new Color(1f, 0.82f, 0.2f, 1f),
                new Color(1f, 0.75f, 0.15f, 0.8f),
                new Color(1f, 0.95f, 0.45f, 0.95f),
                "solar",
                6,
                1),

            new ShipSkin(
                "bio",
                "Bio-Alien Symbiote",
                "Биоморфный симбиот",
                700,
                "Sprites/ship_bio",
                "Живой органический экзоскелет из изумрудного хитина и пульсирующих био-жил. В кабине управляет инопланетный симбиот.",
                new Color(0.3f, 1f, 0.45f, 1f),
                "Sprites/thruster_bio",
                new Color(0.35f, 1f, 0.45f, 1f),
                new Color(0.25f, 0.95f, 0.35f, 0.8f),
                new Color(0.45f, 1f, 0.55f, 0.95f),
                "bio",
                8,
                2),

            new ShipSkin(
                "void",
                "Void Obsidian Reaper",
                "Обсидиановый жнец",
                1000,
                "Sprites/ship_void",
                "Сверхсекретная стелс-тарелка из матового обсидиана с багровым антигравитационным полем и светящимися алыми глазами пилота.",
                new Color(1f, 0.22f, 0.28f, 1f),
                "Sprites/thruster_void",
                new Color(1f, 0.18f, 0.25f, 1f),
                new Color(0.95f, 0.1f, 0.22f, 0.85f),
                new Color(1f, 0.35f, 0.35f, 0.95f),
                "void",
                10,
                3),

            new ShipSkin(
                "steampunk",
                "Brass Aeronaut 1899",
                "Латунный аэронавт",
                1500,
                "Sprites/ship_steampunk",
                "Винтажный викторианский аппарат на паро-ионной тяге с медными заклепками, янтарным стеклом и пилотом в авиаторских очках.",
                new Color(0.95f, 0.65f, 0.3f, 1f),
                "Sprites/thruster_steam",
                new Color(1f, 0.6f, 0.2f, 1f),
                new Color(0.95f, 0.7f, 0.3f, 0.75f),
                new Color(0.95f, 0.92f, 0.85f, 0.9f),
                "steam",
                11,
                5)
        };

        public static ShipSkin Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return All[0];

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
            var skin = Find(id);

            var sprite = Resources.Load<Sprite>(skin.SpriteName);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }
        }
    }
}
