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
                "Миссия 1. Неоновый коридор",
                "Редкий поток обломков у доков.",
                "Astro K медленно выходит из доков Neon City. Обломки редкие и небыстрые. Собери шесть Album: торопиться некуда, коридор длинный.",
                "Коридор открыт. Дальше темнеет вода.",
                6, 8, 2.4f, 1.35f, 1, 0f, 0f, 0f, 0),
            new MissionDefinition(
                "Миссия 2. Глубокая вода",
                "Обломки чаще, сверху падают ракеты.",
                "Под городом тянется тёмный слой. Обломков больше, а между ними сверху срываются ракеты. Нужно восемь Album. Ракету можно сбить, пока она не дошла до корабля.",
                "Водный слой пройден. Впереди лёд.",
                8, 14, 1.15f, 2.4f, 1, 0.28f, 2.8f, 3.1f, 1),
            new MissionDefinition(
                "Миссия 3. Ледяной предел",
                "Плотное поле и залпы ракет.",
                "У Ice Planet обломки идут стеной, ракеты падают парами и быстрее. Крупные глыбы снимают два HP. Нужно двенадцать Album.",
                "Ледяное поле позади. Hype держит курс.",
                12, 20, 0.85f, 3.1f, 1, 0.45f, 1.25f, 4.6f, 2)
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
