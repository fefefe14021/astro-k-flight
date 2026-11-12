namespace KANYE.Gameplay
{
    public class MissionDefinition
    {
        public string Title;
        public string Summary;
        public string Briefing;
        public string VictoryText;
        public int AlbumsRequired;
        public int ExtraAsteroids;
        public float SpawnInterval;
        public float AsteroidSpeed;
        public int Damage;
        public float LargeChance;
        public float MissileInterval;
        public float MissileSpeed;
        public int MissileBurst;

        public MissionDefinition(
            string title,
            string summary,
            string briefing,
            string victoryText,
            int albumsRequired,
            int extraAsteroids,
            float spawnInterval,
            float asteroidSpeed,
            int damage,
            float largeChance,
            float missileInterval,
            float missileSpeed,
            int missileBurst)
        {
            Title = title;
            Summary = summary;
            Briefing = briefing;
            VictoryText = victoryText;
            AlbumsRequired = albumsRequired;
            ExtraAsteroids = extraAsteroids;
            SpawnInterval = spawnInterval;
            AsteroidSpeed = asteroidSpeed;
            Damage = damage;
            LargeChance = largeChance;
            MissileInterval = missileInterval;
            MissileSpeed = missileSpeed;
            MissileBurst = missileBurst;
        }
    }

    public static class MissionCatalog
    {
        public static int Selected { get; private set; }

        public static readonly MissionDefinition[] All =
        {
            new MissionDefinition(
                "Сектор 1. Неоновый коридор (Neon City)",
                "Редкий поток обломков у доков.",
                "Astro K медленно выходит из доков Neon City. Обломки редкие и небыстрые. Собери шесть Album: торопиться некуда, коридор длинный.",
                "Коридор открыт. Впереди шумит Океан.",
                6, 8, 2.4f, 1.35f, 1, 0f, 0f, 0f, 0),

            new MissionDefinition(
                "Сектор 2. Океанский рубеж (Ocean)",
                "Обломки чаще, сверху падают ракеты.",
                "Под городом тянется тёмный слой воды. Обломков больше, а между ними сверху срываются ракеты. Дроны открывают предупредительный огонь. Нужно восемь Album.",
                "Водный рубеж пройден. Температура резко падает.",
                8, 14, 1.35f, 2.2f, 1, 0.28f, 2.8f, 3.1f, 1),

            new MissionDefinition(
                "Сектор 3. Ледяной предел (Ice Planet)",
                "Плотное поле и залпы ракет.",
                "У Ice Planet обломки идут стеной, ракеты падают парами и быстрее. Крупные глыбы снимают два HP. Нужно двенадцать Album.",
                "Ледяное поле позади. Впереди бездна Глубокого Космоса.",
                12, 20, 0.95f, 3.0f, 1, 0.42f, 1.5f, 4.2f, 2),

            new MissionDefinition(
                "Сектор 4. Глубокий космос (Deep Space)",
                "Аномальная плотность, тяжелые метеориты.",
                "Гравитационные аномалии швыряют метеориты на высокой скорости. Орудия противников ведут непрерывный огонь. Требуется пятнадцать Album.",
                "Космос покорен. Вдали сияет маяк Домашней Базы!",
                15, 26, 0.8f, 3.8f, 1, 0.55f, 1.2f, 5.0f, 2),

            new MissionDefinition(
                "Сектор 5. Домашняя база (Home Base)",
                "Финальный штурм защитного пояса.",
                "Массированный заградительный огонь турелей и перехватчиков. Собери двадцать Album, используй супервыстрел [E] и заверши Ночной Рейс!",
                "ПОБЕДА! Корабль благополучно пристыковался к Домашней Базе. Тур завершен!",
                20, 34, 0.68f, 4.4f, 1, 0.65f, 0.95f, 5.8f, 3)
        };

        public static MissionDefinition Current => All[UnityEngine.Mathf.Clamp(Selected, 0, All.Length - 1)];

        public static void Select(int index)
        {
            Selected = UnityEngine.Mathf.Clamp(index, 0, All.Length - 1);
        }
    }

    public static class PilotRank
    {
        public static string Title(int kills)
        {
            if (kills >= 150)
                return "Ас коридора";
            if (kills >= 80)
                return "Ветеран";
            if (kills >= 40)
                return "Воин III";
            if (kills >= 15)
                return "Воин II";
            if (kills >= 5)
                return "Воин I";
            return "Пилот";
        }
    }
}
