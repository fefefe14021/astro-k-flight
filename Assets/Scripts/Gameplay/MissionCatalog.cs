// Assets/Scripts/Gameplay/MissionCatalog.cs
// Namespace: KANYE.Gameplay

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
                "Сектор 1. Неоновый рубеж (Neon Orbit)",
                "Прорыв блокады доков Neon City.",
                "Капитан Зигги выводит Astro-Saucer из грузовых доков Neon City. " +
                "Повсюду патрулируют сторожевые дроны Синдиката и дрейфуют обломки. " +
                "Собери 6 Альбомов Hype Core, парируй розовые сферы и сокруши стража сектора — Vanguard Dread-Drone!",
                "Командный дрон уничтожен! Сектор освобожден. Впереди темнеет Океанический рубеж.",
                6, 8, 2.3f, 1.35f, 1, 0f, 0f, 0f, 0),

            new MissionDefinition(
                "Сектор 2. Океанский разлом (Oceanic Trench)",
                "Штормовая атмосфера и торпедные залпы.",
                "Густые метановые облака и штормовые волны планеты-океана. " +
                "Сверху сквозь бурю срываются баллистические ракеты Синдиката. " +
                "Собери 8 Альбомов и вымани на бой суб-орбитальный крейсер Leviathan!",
                "Крейсер расколот пополам! Атмосфера позади, тарелка входит в зону аномального холода.",
                8, 14, 1.35f, 2.2f, 1, 0.28f, 2.8f, 3.1f, 1),

            new MissionDefinition(
                "Сектор 3. Ледяной предел (Cryo Nebula)",
                "Замерзшие астероиды и бронебойные залпы.",
                "Криогенная туманность сковала космос ледяным панцирем. " +
                "Метеориты раскалываются на острые осколки, наносящие двойной урон. " +
                "Собери 12 Альбомов и уничтожь ледяного колосса — Cryo-Titan Dreadnought!",
                "Дредноут рассыпался ледяной пылью! Корабль пересекает горизонт Бездны.",
                12, 20, 0.95f, 3.0f, 1, 0.42f, 1.5f, 4.2f, 2),

            new MissionDefinition(
                "Сектор 4. Бездна сингулярности (Void Abyss)",
                "Гравитационные аномалии и стелс-перехватчики.",
                "Искаженное пространство вблизи черных дыр. " +
                "Гравитация швыряет тяжелые обломки, а орудия ведут беглый веерный огонь. " +
                "Собери 15 Альбомов и одолей призрачного жнеца — Void Reaper!",
                "Жнец повержен! Пространство стабилизировалось. Впереди сияет цитадель Fortress Prime!",
                15, 26, 0.8f, 3.8f, 1, 0.55f, 1.2f, 5.0f, 2),

            new MissionDefinition(
                "Сектор 5. Цитадель Синдиката (Fortress Prime)",
                "Финальная битва за свободу Вселенной.",
                "Главный защитный пояс Цитадели. " +
                "Массированный заградительный огонь турелей, залпы торпед и элитные звенья дронов. " +
                "Собери 20 последних Альбомов, используй весь арсенал и сокруши верховный флагман — Fortress Prime!",
                "ПОБЕДА! Цитадель Синдиката пала! Все 50 виниловых реликвий возвращены. Золотая Эра Звука спасена!",
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
                return "Ас Гравитации";
            if (kills >= 80)
                return "Ветеран Hypeverse";
            if (kills >= 40)
                return "Штурман Зигги";
            if (kills >= 15)
                return "Кадет Тарелки";
            if (kills >= 5)
                return "Новичок";
            return "Пилот";
        }
    }
}
