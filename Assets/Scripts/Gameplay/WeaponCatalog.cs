// Assets/Scripts/Gameplay/WeaponCatalog.cs
// Namespace: KANYE.Gameplay
// Diverse armory catalog inspired by Cuphead and Broforce.

using KANYE.Services;
using UnityEngine;

namespace KANYE.Gameplay
{
    public enum WeaponType
    {
        TwinBlaster,
        ScatterShotgun,
        HomingMissiles,
        HeavyRailgun,
        SawDisc,
        PlasmaMortar
    }

    public class WeaponDefinition
    {
        public string Id;
        public string NameRu;
        public string NameEn;
        public string CategoryRu;
        public string CategoryEn;
        public int Price;
        public string DescriptionRu;
        public string DescriptionEn;
        public int Damage;
        public float FireInterval;
        public WeaponType Type;
        public string ProjectileSprite;
        public Color ThemeColor;
        public int RequiredMissionIndex;
        public int UpgradeCostLv2;
        public int UpgradeCostLv3;

        // Backwards compatibility properties
        public string Title => NameRu;
        public string Subtitle => NameEn;
        public string Description => DescriptionRu;

        // Fully localized title, subtitle, and description
        public string LocalizedTitle => Localization.CurrentLanguage == GameLanguage.Russian ? NameRu : NameEn;
        public string LocalizedSubtitle => Localization.CurrentLanguage == GameLanguage.Russian ? CategoryRu : CategoryEn;
        public string LocalizedDescription => Localization.CurrentLanguage == GameLanguage.Russian ? DescriptionRu : (string.IsNullOrEmpty(DescriptionEn) ? DescriptionRu : DescriptionEn);

        public WeaponDefinition(
            string id,
            string nameRu,
            string nameEn,
            string categoryRu,
            string categoryEn,
            int price,
            string descriptionRu,
            int damage,
            float fireInterval,
            WeaponType type,
            string projectileSprite,
            Color themeColor,
            int requiredMissionIndex = 0,
            int upgradeCostLv2 = 75,
            int upgradeCostLv3 = 160,
            string descriptionEn = null)
        {
            Id = id;
            NameRu = nameRu;
            NameEn = nameEn;
            CategoryRu = categoryRu;
            CategoryEn = categoryEn;
            Price = price;
            DescriptionRu = descriptionRu;
            DescriptionEn = descriptionEn;
            Damage = damage;
            FireInterval = fireInterval;
            Type = type;
            ProjectileSprite = projectileSprite;
            ThemeColor = themeColor;
            RequiredMissionIndex = requiredMissionIndex;
            UpgradeCostLv2 = upgradeCostLv2;
            UpgradeCostLv3 = upgradeCostLv3;
        }
    }

    public static class WeaponCatalog
    {
        public static readonly WeaponDefinition[] All =
        {
            new WeaponDefinition(
                "twin_blaster",
                "Сдвоенный плазмомет",
                "Twin Plasma Blaster",
                "Скорострельный плазмомет",
                "Rapid Twin Blaster",
                0,
                "Базовое скорострельное оружие Зигги. Двойные лазерные импульсы с высоким темпом стрельбы и стабильной кучностью.",
                1,
                0.14f,
                WeaponType.TwinBlaster,
                "Sprites/bolt",
                new Color(0.2f, 0.9f, 1f, 1f),
                0, 75, 160,
                "Ziggy's standard rapid-fire weapon. High-cadence twin laser pulses delivering pinpoint accuracy and sustained firepower."),

            new WeaponDefinition(
                "scatter_shotgun",
                "Дробовик «Громобой»",
                "Scatter Cannon",
                "Дробовик веерного залпа",
                "Wide-Spread Shotgun",
                120,
                "Broforce & Cuphead Spread стиль! Залп из 5 раскаленных дробинок веером. Колоссальный урон в упор по крупным целям и боссам!",
                1,
                0.32f,
                WeaponType.ScatterShotgun,
                "Sprites/proj_spread",
                new Color(1f, 0.55f, 0.15f, 1f),
                1, 140, 260,
                "Cuphead & Broforce Spread style! Discharges 5 superheated plasma pellets in a wide fan. Immense point-blank burst damage."),

            new WeaponDefinition(
                "homing_missiles",
                "Микроракеты «Оса»",
                "Homing Swarm",
                "Самонаводящиеся микроракеты",
                "Guided Micro-Missiles",
                260,
                "Cuphead Chaser! Залпы умных микроракет, огибающих препятствия и самостоятельно выслеживающих ближайших дронов и босса.",
                1,
                0.22f,
                WeaponType.HomingMissiles,
                "Sprites/proj_homing",
                new Color(0.4f, 1f, 0.5f, 1f),
                3, 240, 420,
                "Cuphead Chaser style! Autonomous micro-guided missiles that curve around obstacles and actively hunt down nearest hostiles."),

            new WeaponDefinition(
                "heavy_railgun",
                "Рельсотрон «Квазар»",
                "Hyper Railgun",
                "Бронебойный тахионный луч",
                "Armor-Piercing Beam",
                480,
                "Бронебойный сверхсветовой заряд. Пробивает астероиды и врагов насквозь единым лучом высокой плотности!",
                3,
                0.38f,
                WeaponType.HeavyRailgun,
                "Sprites/proj_rail",
                new Color(0.95f, 0.85f, 0.2f, 1f),
                6, 380, 650,
                "Armor-piercing tachyon beam. Slices cleanly through dense asteroid fields and enemy formations in a single concentrated shot."),

            new WeaponDefinition(
                "saw_disc",
                "Пиломет «Циркулярка»",
                "Buzzsaw Roundabout",
                "Рикошетный дискомет",
                "Boomerang Buzzsaws",
                720,
                "Cuphead Roundabout! Выстреливает зубчатые пилы-диски, рикошетящие от границ космоса и наносящие повторный урон при обратном полете.",
                2,
                0.28f,
                WeaponType.SawDisc,
                "Sprites/proj_saw",
                new Color(1f, 0.25f, 0.25f, 1f),
                9, 520, 880,
                "Cuphead Roundabout style! Launches spinning buzzsaws that boomerang back from space boundaries, striking targets again on return."),

            new WeaponDefinition(
                "plasma_mortar",
                "Мортира «Лоббер»",
                "Plasma Lobber",
                "Тяжелая осадная мортира",
                "Heavy AoE Mortar",
                1050,
                "Тяжелые гравитационные шары нестабильной плазмы, детонирующие при столкновении разрушительным взрывом по площади (AoE).",
                4,
                0.48f,
                WeaponType.PlasmaMortar,
                "Sprites/proj_mortar",
                new Color(0.7f, 0.35f, 1f, 1f),
                11, 780, 1350,
                "Heavy unstable plasma spheres that detonate upon impact, generating a catastrophic area-of-effect (AoE) shockwave.")
        };

        public static WeaponDefinition Find(string id)
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

        public static int GetDamage(string weaponId, int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            switch (weaponId)
            {
                case "twin_blaster":
                    return level;
                case "scatter_shotgun":
                    return level;
                case "homing_missiles":
                    return level >= 3 ? 2 : 1;
                case "heavy_railgun":
                    return level == 1 ? 3 : (level == 2 ? 6 : 10);
                case "saw_disc":
                    return level == 1 ? 2 : (level == 2 ? 4 : 7);
                case "plasma_mortar":
                    return level == 1 ? 4 : (level == 2 ? 8 : 14);
                default:
                    return level;
            }
        }

        public static float GetFireInterval(string weaponId, int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            switch (weaponId)
            {
                case "twin_blaster":
                    return level == 1 ? 0.14f : (level == 2 ? 0.115f : 0.095f);
                case "scatter_shotgun":
                    return level == 1 ? 0.32f : (level == 2 ? 0.28f : 0.24f);
                case "homing_missiles":
                    return level == 1 ? 0.22f : (level == 2 ? 0.19f : 0.16f);
                case "heavy_railgun":
                    return level == 1 ? 0.38f : (level == 2 ? 0.33f : 0.28f);
                case "saw_disc":
                    return level == 1 ? 0.28f : (level == 2 ? 0.24f : 0.20f);
                case "plasma_mortar":
                    return level == 1 ? 0.48f : (level == 2 ? 0.42f : 0.36f);
                default:
                    return 0.14f;
            }
        }

        public static int GetProjectileCount(string weaponId, int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            switch (weaponId)
            {
                case "twin_blaster":
                    return 2;
                case "scatter_shotgun":
                    return level == 1 ? 5 : (level == 2 ? 6 : 7);
                case "homing_missiles":
                    return level == 1 ? 2 : (level == 2 ? 3 : 4);
                case "heavy_railgun":
                    return 1;
                case "saw_disc":
                    return level >= 3 ? 2 : 1;
                case "plasma_mortar":
                    return 1;
                default:
                    return 1;
            }
        }

        public static int GetUpgradeCost(string weaponId, int currentLevel)
        {
            var def = Find(weaponId);
            if (def == null || currentLevel >= 3) return -1;
            return currentLevel == 1 ? def.UpgradeCostLv2 : def.UpgradeCostLv3;
        }

        public static string GetUpgradeSummary(string weaponId, int currentLevel)
        {
            var nextLvl = currentLevel + 1;
            if (nextLvl > 3) return Localization.T("★ МАКСИМАЛЬНЫЙ УРОВЕНЬ ★", "★ MAXIMUM LEVEL ★");

            var curDmg = GetDamage(weaponId, currentLevel);
            var nextDmg = GetDamage(weaponId, nextLvl);
            var curRate = 1f / GetFireInterval(weaponId, currentLevel);
            var nextRate = 1f / GetFireInterval(weaponId, nextLvl);

            switch (weaponId)
            {
                case "scatter_shotgun":
                    var curP = GetProjectileCount(weaponId, currentLevel);
                    var nextP = GetProjectileCount(weaponId, nextLvl);
                    return Localization.TFormat(
                        $"Дробь: {curP}→{nextP} шт | Урон: {curDmg}→{nextDmg} | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"Pellets: {curP}→{nextP} | DMG: {curDmg}→{nextDmg} | Rate: {curRate:0.0}→{nextRate:0.0}/s");
                case "homing_missiles":
                    var curM = GetProjectileCount(weaponId, currentLevel);
                    var nextM = GetProjectileCount(weaponId, nextLvl);
                    return Localization.TFormat(
                        $"Ракеты: {curM}→{nextM} шт | Урон: {curDmg}→{nextDmg} | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"Missiles: {curM}→{nextM} | DMG: {curDmg}→{nextDmg} | Rate: {curRate:0.0}→{nextRate:0.0}/s");
                case "heavy_railgun":
                    return Localization.TFormat(
                        $"Бронебой: {curDmg}→{nextDmg} Урона | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"Piercing: {curDmg}→{nextDmg} DMG | Rate: {curRate:0.0}→{nextRate:0.0}/s");
                case "saw_disc":
                    return Localization.TFormat(
                        $"Пилы: Урон {curDmg}→{nextDmg} | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"Buzzsaws: DMG {curDmg}→{nextDmg} | Rate: {curRate:0.0}→{nextRate:0.0}/s");
                case "plasma_mortar":
                    return Localization.TFormat(
                        $"AoE: Урон {curDmg}→{nextDmg} | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"AoE: DMG {curDmg}→{nextDmg} | Rate: {curRate:0.0}→{nextRate:0.0}/s");
                default:
                    return Localization.TFormat(
                        $"Плазмомет: Урон {curDmg}→{nextDmg} | Темп: {curRate:0.0}→{nextRate:0.0}/с",
                        $"Blaster: DMG {curDmg}→{nextDmg} | Rate: {curRate:0.0}→{nextRate:0.0}/s");
            }
        }
    }
}
