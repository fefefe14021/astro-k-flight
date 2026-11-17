// Assets/Scripts/Systems/MetaTechCatalog.cs
// Namespace: KANYE.Systems
// Meta-Progression Tech tree for permanent Flying Saucer upgrades in the Hangar.

using System;
using UnityEngine;

namespace KANYE.Systems
{
    [Serializable]
    public struct TechCost
    {
        public int credits;
        public int cores;

        public TechCost(int credits, int cores)
        {
            this.credits = credits;
            this.cores = cores;
        }

        public void Deconstruct(out int credits, out int cores)
        {
            credits = this.credits;
            cores = this.cores;
        }
    }

    public class MetaTechDefinition
    {
        public string Id;
        public string Title;
        public string Subtitle;
        public string Description;
        public int MaxLevel;
        public string SpriteName;
        public Color ThemeColor;

        public MetaTechDefinition(string id, string title, string subtitle, string description, int maxLevel, string spriteName, Color themeColor)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            Description = description;
            MaxLevel = maxLevel;
            SpriteName = spriteName;
            ThemeColor = themeColor;
        }

        public TechCost GetCost(int currentLevel)
        {
            switch (Id)
            {
                case "hull":
                    switch (currentLevel)
                    {
                        case 1: return new TechCost(150, 0);
                        case 2: return new TechCost(350, 1);
                        case 3: return new TechCost(700, 2);
                        case 4: return new TechCost(1200, 3);
                        default: return new TechCost(0, 0);
                    }
                case "hype":
                    switch (currentLevel)
                    {
                        case 1: return new TechCost(120, 0);
                        case 2: return new TechCost(280, 1);
                        case 3: return new TechCost(600, 2);
                        case 4: return new TechCost(1000, 3);
                        default: return new TechCost(0, 0);
                    }
                case "magnet":
                    switch (currentLevel)
                    {
                        case 1: return new TechCost(100, 0);
                        case 2: return new TechCost(250, 0);
                        case 3: return new TechCost(550, 1);
                        default: return new TechCost(0, 0);
                    }
                case "parry":
                    switch (currentLevel)
                    {
                        case 1: return new TechCost(200, 1);
                        case 2: return new TechCost(500, 2);
                        default: return new TechCost(0, 0);
                    }
                default:
                    return new TechCost(100, 0);
            }
        }

        public string GetStatDescription(int level)
        {
            switch (Id)
            {
                case "hull":
                    return $"Прочность: {2 + level} HP (Макс. жизней: {2 + level})";
                case "hype":
                    var bonus = (level - 1) * 15;
                    return $"Скорость шкалы Ярости: +{bonus}% к накоплению энергии";
                case "magnet":
                    var range = 3.8f + (level - 1) * 1.5f;
                    return $"Радиус притяжения нанитов: {range:F1} м";
                case "parry":
                    var invuln = 0.75f + (level - 1) * 0.25f;
                    var hypeGain = 30 + (level - 1) * 10;
                    return $"Щит парирования: {invuln:F2}с неуязвимости | +{hypeGain} Ярости";
                default:
                    return $"Уровень {level}";
            }
        }
    }

    public static class MetaTechCatalog
    {
        public static readonly MetaTechDefinition[] All =
        {
            new MetaTechDefinition(
                "hull",
                "Усиленная титановая обшивка",
                "Броня и запас прочности",
                "Многослойная броня с нано-амортизаторами. Увеличивает максимальный запас прочности тарелки во всех режимах игры.",
                5,
                "Sprites/nanite_repair",
                new Color(0.35f, 0.95f, 0.55f, 1f)
            ),

            new MetaTechDefinition(
                "hype",
                "Квантовый генератор Ярости",
                "Зарядка EX и Мега-Лазера",
                "Усиливает катализатор энергии Hype Core. С каждым уничтоженным противником и астероидом шкала спец-атак заполняется быстрее.",
                5,
                "Sprites/spark",
                new Color(1f, 0.85f, 0.25f, 1f)
            ),

            new MetaTechDefinition(
                "magnet",
                "Гравитационный тяговый луч",
                "Автосбор ресурсов и нанитов",
                "Генерирует направленную гравитационную воронку вокруг корабля, автоматически притягивая капсулы нано-ремонта и выпавшие ресурсы с увеличенной дистанции.",
                4,
                "Sprites/proj_homing",
                new Color(0.35f, 0.8f, 1f, 1f)
            ),

            new MetaTechDefinition(
                "parry",
                "Фазовый щит парирования",
                "Отражение Cuphead-сфер",
                "Усиливает фазовое поглощение при контакте с розовыми сферами: продлевает время защитного поля и заряжает взрывной объём энергии Ярости.",
                3,
                "Sprites/parry_orb",
                new Color(1f, 0.45f, 0.85f, 1f)
            )
        };

        public static MetaTechDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return All[0];
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].Id == id) return All[i];
            }
            return All[0];
        }
    }
}
