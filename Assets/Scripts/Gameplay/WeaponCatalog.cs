// Assets/Scripts/Gameplay/WeaponCatalog.cs
// Namespace: KANYE.Gameplay
// Diverse armory catalog inspired by Cuphead and Broforce.

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
        public string Title;
        public string Subtitle;
        public int Price;
        public string Description;
        public int Damage;
        public float FireInterval;
        public WeaponType Type;
        public string ProjectileSprite;
        public Color ThemeColor;

        public WeaponDefinition(
            string id,
            string title,
            string subtitle,
            int price,
            string description,
            int damage,
            float fireInterval,
            WeaponType type,
            string projectileSprite,
            Color themeColor)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            Price = price;
            Description = description;
            Damage = damage;
            FireInterval = fireInterval;
            Type = type;
            ProjectileSprite = projectileSprite;
            ThemeColor = themeColor;
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
                0,
                "Базовое скорострельное оружие Зигги. Двойные лазерные импульсы с высоким темпом стрельбы и стабильной кучностью.",
                1,
                0.14f,
                WeaponType.TwinBlaster,
                "Sprites/bolt",
                new Color(0.2f, 0.9f, 1f, 1f)),

            new WeaponDefinition(
                "scatter_shotgun",
                "Дробовик «Громобой»",
                "Scatter Cannon",
                45,
                "Broforce & Cuphead Spread стиль! Залп из 5 раскаленных дробинок веером. Колоссальный урон в упор по крупным целям и боссам!",
                1,
                0.32f,
                WeaponType.ScatterShotgun,
                "Sprites/proj_spread",
                new Color(1f, 0.55f, 0.15f, 1f)),

            new WeaponDefinition(
                "homing_missiles",
                "Микроракеты «Оса»",
                "Homing Swarm",
                80,
                "Cuphead Chaser! Залпы умных микроракет, огибающих препятствия и самостоятельно выслеживающих ближайших дронов и босса.",
                1,
                0.22f,
                WeaponType.HomingMissiles,
                "Sprites/proj_homing",
                new Color(0.4f, 1f, 0.5f, 1f)),

            new WeaponDefinition(
                "heavy_railgun",
                "Рельсотрон «Квазар»",
                "Hyper Railgun",
                120,
                "Бронебойный сверхсветовой заряд. Пробивает астероиды и врагов насквозь единым лучом высокой плотности!",
                3,
                0.38f,
                WeaponType.HeavyRailgun,
                "Sprites/proj_rail",
                new Color(0.95f, 0.85f, 0.2f, 1f)),

            new WeaponDefinition(
                "saw_disc",
                "Пиломет «Циркулярка»",
                "Buzzsaw Roundabout",
                160,
                "Cuphead Roundabout! Выстреливает зубчатые пилы-диски, рикошетящие от границ космоса и наносящие повторный урон при обратном полете.",
                2,
                0.28f,
                WeaponType.SawDisc,
                "Sprites/proj_saw",
                new Color(1f, 0.25f, 0.25f, 1f)),

            new WeaponDefinition(
                "plasma_mortar",
                "Мортира «Лоббер»",
                "Plasma Lobber",
                210,
                "Тяжелые гравитационные шары нестабильной плазмы, детонирующие при столкновении разрушительным взрывом по площади (AoE).",
                4,
                0.48f,
                WeaponType.PlasmaMortar,
                "Sprites/proj_mortar",
                new Color(0.7f, 0.35f, 1f, 1f))
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
    }
}
