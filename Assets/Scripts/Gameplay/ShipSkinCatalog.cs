// Assets/Scripts/Gameplay/ShipSkinCatalog.cs
// Namespace: KANYE.Gameplay
// Custom Flying Saucer hulls with visible cockpit pilot variants.

using KANYE.Services;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class ShipSkin
    {
        public string Id;
        public string NameRu;
        public string NameEn;
        public string CategoryRu;
        public string CategoryEn;
        public int Price;
        public string SpriteName;
        public string DescriptionRu;
        public string DescriptionEn;
        public Color ThemeColor;
        public Color Color => ThemeColor;

        // Backwards compatibility properties
        public string Title => NameEn;
        public string Subtitle => NameRu;
        public string Description => DescriptionRu;

        // Fully localized title, subtitle, and description
        public string LocalizedTitle => Localization.CurrentLanguage == GameLanguage.Russian ? NameRu : NameEn;
        public string LocalizedSubtitle => Localization.CurrentLanguage == GameLanguage.Russian ? CategoryRu : CategoryEn;
        public string LocalizedDescription => Localization.CurrentLanguage == GameLanguage.Russian ? DescriptionRu : (string.IsNullOrEmpty(DescriptionEn) ? DescriptionRu : DescriptionEn);

        // Custom Thruster Flame & Particle System parameters
        public string ThrusterSprite;
        public Color ThrusterColor;
        public Color AntiGravColor;
        public Color ParticleColor;
        public string ParticleType;
        public int RequiredMissionIndex;

        public ShipSkin(
            string id,
            string nameRu,
            string nameEn,
            string categoryRu,
            string categoryEn,
            int price,
            string spriteName,
            string descriptionRu,
            Color themeColor,
            string thrusterSprite = "Sprites/thruster_ion",
            Color? thrusterColor = null,
            Color? antiGravColor = null,
            Color? particleColor = null,
            string particleType = "ion",
            int requiredMissionIndex = 0,
            string descriptionEn = null)
        {
            Id = id;
            NameRu = nameRu;
            NameEn = nameEn;
            CategoryRu = categoryRu;
            CategoryEn = categoryEn;
            Price = price;
            SpriteName = spriteName;
            DescriptionRu = descriptionRu;
            ThemeColor = themeColor;
            ThrusterSprite = thrusterSprite;
            ThrusterColor = thrusterColor ?? themeColor;
            AntiGravColor = antiGravColor ?? new Color(themeColor.r, themeColor.g, themeColor.b, 0.7f);
            ParticleColor = particleColor ?? themeColor;
            ParticleType = particleType;
            RequiredMissionIndex = requiredMissionIndex;
            DescriptionEn = descriptionEn;
        }
    }

    public static class ShipSkinCatalog
    {
        public static readonly ShipSkin[] All =
        {
            new ShipSkin(
                "default",
                "Классический хром 1957",
                "Retro Chrome 1957",
                "Оригинальная тарелка Зигги",
                "Classic Chrome Saucer",
                0,
                "Sprites/ship",
                "Классическая летающая тарелка из зеркального хрома с бирюзовым стеклянным куполом. В кабине — отважный Капитан Зигги у штурвала.",
                new Color(0.3f, 0.85f, 1f, 1f),
                "Sprites/thruster_ion",
                new Color(0.3f, 0.85f, 1f, 1f),
                new Color(0.2f, 0.8f, 1f, 0.7f),
                new Color(0.5f, 0.9f, 1f, 0.95f),
                "ion",
                0,
                "Classic flying saucer crafted from mirror-polished chrome with a reinforced quartz canopy. Brave Captain Ziggy himself takes the helm."),

            new ShipSkin(
                "cyberpunk",
                "Кибер-диск 2077",
                "Neon 2077 Cyber-Disc",
                "Ночной перехватчик Синдиката",
                "Syndicate Cyber Interceptor",
                160,
                "Sprites/ship_cyberpunk",
                "Неоновый ночной перехватчик с темно-фиолетовым композитным диском, неоново-розовыми огнями и пилотом в кибер-визоре.",
                new Color(1f, 0.2f, 0.75f, 1f),
                "Sprites/thruster_cyber",
                new Color(1f, 0.25f, 0.8f, 1f),
                new Color(1f, 0.15f, 0.7f, 0.75f),
                new Color(0.95f, 0.3f, 1f, 0.95f),
                "cyber",
                3,
                "High-tech night interceptor engineered from carbon-composite alloy with neon-pink edge trim and a cyber-visored pilot."),

            new ShipSkin(
                "gold",
                "Золотой VIP-шаттл",
                "Solar Flare VIP",
                "Престижный золотой корпус",
                "Prestige 24K Solar Shuttle",
                420,
                "Sprites/ship_gold",
                "Роскошная тарелка из 24-каратного сплава со сверкающим янтарным куполом и пилотом в стильных темных очках.",
                new Color(1f, 0.85f, 0.2f, 1f),
                "Sprites/thruster_gold",
                new Color(1f, 0.82f, 0.2f, 1f),
                new Color(1f, 0.75f, 0.15f, 0.8f),
                new Color(1f, 0.95f, 0.45f, 0.95f),
                "solar",
                6,
                "Prestige saucer plated in 24-karat solar alloy, featuring an amber sun-shield dome and a pilot rocking cool sunglasses."),

            new ShipSkin(
                "bio",
                "Биоморфный симбиот",
                "Bio-Alien Symbiote",
                "Живой органический экзоскелет",
                "Living Alien Exoskeleton",
                750,
                "Sprites/ship_bio",
                "Живой органический экзоскелет из изумрудного хитина и пульсирующих био-жил. В кабине управляет инопланетный симбиот.",
                new Color(0.3f, 1f, 0.45f, 1f),
                "Sprites/thruster_bio",
                new Color(0.35f, 1f, 0.45f, 1f),
                new Color(0.25f, 0.95f, 0.35f, 0.8f),
                new Color(0.45f, 1f, 0.55f, 0.95f),
                "bio",
                8,
                "Living organic bioship composed of emerald chitin plates and pulsating plasma veins, steered by an extraterrestrial symbiote."),

            new ShipSkin(
                "void",
                "Обсидиановый жнец",
                "Void Obsidian Reaper",
                "Секретный стелс-аппарат",
                "Covert Stealth Saucer",
                1100,
                "Sprites/ship_void",
                "Сверхсекретная стелс-тарелка из матового обсидиана с багровым антигравитационным полем и светящимися алыми глазами пилота.",
                new Color(1f, 0.22f, 0.28f, 1f),
                "Sprites/thruster_void",
                new Color(1f, 0.18f, 0.25f, 1f),
                new Color(0.95f, 0.1f, 0.22f, 0.85f),
                new Color(1f, 0.35f, 0.35f, 0.95f),
                "void",
                10,
                "Covert stealth saucer fashioned from matte void obsidian with a blood-crimson antigravity field and piercing red pilot optics."),

            new ShipSkin(
                "steampunk",
                "Латунный аэронавт 1899",
                "Brass Aeronaut 1899",
                "Паро-ионный ретро-аппарат",
                "Victorian Steam Aeronaut",
                1600,
                "Sprites/ship_steampunk",
                "Винтажный викторианский аппарат на паро-ионной тяге с медными заклепками, янтарным стеклом и пилотом в авиаторских очках.",
                new Color(0.95f, 0.65f, 0.3f, 1f),
                "Sprites/thruster_steam",
                new Color(1f, 0.6f, 0.2f, 1f),
                new Color(0.95f, 0.7f, 0.3f, 0.75f),
                new Color(0.95f, 0.92f, 0.85f, 0.9f),
                "steam",
                11,
                "Victorian retro-futuristic craft powered by steam-ion combustion, featuring copper rivets, amber gauge lenses, and flight goggles.")
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
