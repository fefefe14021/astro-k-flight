// Assets/Scripts/Gameplay/LoreCatalog.cs
// Namespace: KANYE.Gameplay
// Deep universe lore, character dossiers, syndicate archives, and captain log entries.

using KANYE.Services;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class LoreEntry
    {
        public string Id;
        public string Title;
        public string Category;
        public string Content;

        public string TitleEn;
        public string CategoryEn;
        public string ContentEn;

        public string LocalizedTitle => Localization.CurrentLanguage == GameLanguage.Russian ? Title : (!string.IsNullOrEmpty(TitleEn) ? TitleEn : Title);
        public string LocalizedCategory => Localization.CurrentLanguage == GameLanguage.Russian ? Category : (!string.IsNullOrEmpty(CategoryEn) ? CategoryEn : Category);
        public string LocalizedContent => Localization.CurrentLanguage == GameLanguage.Russian ? Content : (!string.IsNullOrEmpty(ContentEn) ? ContentEn : Content);

        public LoreEntry(string id, string title, string category, string content, string titleEn = null, string categoryEn = null, string contentEn = null)
        {
            Id = id;
            Title = title;
            Category = category;
            Content = content;
            TitleEn = titleEn;
            CategoryEn = categoryEn;
            ContentEn = contentEn;
        }
    }

    public class MissionDialogue
    {
        public string Speaker;
        public string Header;
        public string[] IntroLines;
        public string BossWarning;
        public string TutorialIntro;

        public string SpeakerEn;
        public string HeaderEn;
        public string[] IntroLinesEn;
        public string BossWarningEn;
        public string TutorialIntroEn;

        public string LocalizedSpeaker => Localization.CurrentLanguage == GameLanguage.Russian ? Speaker : (!string.IsNullOrEmpty(SpeakerEn) ? SpeakerEn : Speaker);
        public string LocalizedHeader => Localization.CurrentLanguage == GameLanguage.Russian ? Header : (!string.IsNullOrEmpty(HeaderEn) ? HeaderEn : Header);
        public string[] LocalizedIntroLines => Localization.CurrentLanguage == GameLanguage.Russian ? IntroLines : (IntroLinesEn != null && IntroLinesEn.Length > 0 ? IntroLinesEn : IntroLines);
        public string LocalizedBossWarning => Localization.CurrentLanguage == GameLanguage.Russian ? BossWarning : (!string.IsNullOrEmpty(BossWarningEn) ? BossWarningEn : BossWarning);
        public string LocalizedTutorialIntro => Localization.CurrentLanguage == GameLanguage.Russian ? TutorialIntro : (!string.IsNullOrEmpty(TutorialIntroEn) ? TutorialIntroEn : TutorialIntro);

        public MissionDialogue(
            string speaker, string header, string[] introLines, string bossWarning, string tutorialIntro = null,
            string speakerEn = null, string headerEn = null, string[] introLinesEn = null, string bossWarningEn = null, string tutorialIntroEn = null)
        {
            Speaker = speaker;
            Header = header;
            IntroLines = introLines;
            BossWarning = bossWarning;
            TutorialIntro = tutorialIntro;
            SpeakerEn = speakerEn;
            HeaderEn = headerEn;
            IntroLinesEn = introLinesEn;
            BossWarningEn = bossWarningEn;
            TutorialIntroEn = tutorialIntroEn;
        }
    }

    public static class LoreCatalog
    {
        public static readonly MissionDialogue[] Dialogues =
        {
            // СЕКТОР 1: НЕОНОВЫЙ РУБЕЖ (Neon Orbit)
            // Миссия 1
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // НЕОНОВЫЙ КОРРИДОР",
                new[]
                {
                    "Зигги, дружище, приём! Это Барни с радиобазы. Шлем плотно сидит? Отлично!",
                    "Синдикат еще не понял, что происходит — в небе лишь легкие дроны и обломки.",
                    "Твоя тарелка заправлена виниловой энергией. Освойся со штурвалом, сбей астероиды и пройди 48 км маршрута! Погнали!"
                },
                null,
                "Зигги! Я только что заменил ионные приводы на нашей тарелке! Проверь штурвал на WASD и пальни вон в тот тренировочный булыжник на [ПРОБЕЛ]! Если разобьешь — включится гипердвигатель!",
                "Navigator Barney",
                "📡 COMMS // NEON CORRIDOR",
                new[]
                {
                    "Ziggy, buddy, do you read me? This is Barney from radio base. Helmet tight? Awesome!",
                    "The Syndicate hasn't realized what's happening yet — only light drones and debris ahead.",
                    "Your saucer is fueled with vinyl energy. Get used to flight controls, shoot down asteroids, and clear the 48 km route! Let's go!"
                },
                null,
                "Ziggy! I just replaced the ionic thrusters on our saucer! Test the steering with WASD and blast that training boulder with [SPACE]! If you break it, the hyperdrive activates!"),

            // Миссия 2 (Открытие парирования)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // АСТЕРОИДНЫЙ ЗАСЛОН",
                new[]
                {
                    "Зигги, внимание! Синдикат выставил блокаду и сбросил ритмические мины!",
                    "Видишь эти пульсирующие РОЗОВЫЕ сферы? Врезайся в них прямо в лоб на встречном курсе!",
                    "Ты спарируешь их и получишь щит неуязвимости! Опробуй ПАРИРОВАНИЕ!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // ASTEROID BARRIER",
                new[]
                {
                    "Ziggy, heads up! The Syndicate has deployed a blockade and dropped rhythm mines!",
                    "See those pulsing PINK spheres? Ram right into them head-on!",
                    "You will PARRY them and gain invulnerability! Try out PARRYING!"
                },
                null),

            // Миссия 3 (Босс Сектора 1)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ОРБИТАЛЬНЫЙ ШЛЮЗ",
                new[]
                {
                    "Мы подобрались к выходу из сектора, но гипер-шлюз перекрыт сторожевым флагманом «Авангарда»!",
                    "Пройди 70 км, чтобы перегрузить глушители и вызвать дред-дрон на бой!"
                },
                "⚠️ Зигги, радары зашкаливают! Сторожевой «Авангард» на курсе перехвата! Когда полетят его розовые залпы — ПАРИРУЙ их на встречном курсе и бей в ответ!",
                null,
                "Navigator Barney",
                "📡 COMMS // ORBITAL GATE",
                new[]
                {
                    "We're approaching the sector exit, but the hyper-gate is guarded by Vanguard's flagship!",
                    "Travel 70 km to overload their jammers and force the dread-drone into battle!"
                },
                "⚠️ Ziggy, radar readings are off the charts! The Vanguard guard is intercepting! When pink volleys launch — PARRY them head-on and return fire!"),

            // СЕКТОР 2: ОКЕАНСКИЙ РАЗЛОМ (Oceanic Trench)
            // Миссия 4 (Открытие EX-Атак [Q])
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // МЕТАНОВЫЙ ШТОРМ",
                new[]
                {
                    "Отличный прыжок, Зигги! Мы в атмосфере планеты-океана. Сверху сквозь бурю падают баллистические торпеды!",
                    "Я откалибровал вспомогательный ракетный отсек: когда на шкале Ярости есть хотя бы одна карта, жми [Q]!",
                    "Вылетит бронебойная EX-ракета с колоссальным уроном! Опробуй EX-атаки в шторме!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // METHANE STORM",
                new[]
                {
                    "Great jump, Ziggy! We're in the ocean planet's atmosphere. Ballistic torpedoes are raining from above!",
                    "I calibrated the auxiliary missile bay: whenever you have at least one card on your Hype meter, hit [Q]!",
                    "An armor-piercing EX-rocket will launch with massive damage! Test out EX attacks in the storm!"
                },
                null),

            // Миссия 5 (Перехватчики и Прокачка оружия)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ЭСКАДРИЛЬЯ ВЕРФЕЙ",
                new[]
                {
                    "Синдикат бросил в бой маневренные эскадрильи перехватчиков! Одиночные лазеры едва успевают.",
                    "Помни про Магазин в ангаре: ты можешь прокачать плазмомет до 2-го уровня (удвоив урон) либо купить веерный дробовик или микроракеты!",
                    "Пройди 100 км и расчисти путь к докам!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // SHIPYARD SQUADRON",
                new[]
                {
                    "The Syndicate dispatched agile interceptor swarms! Single lasers can barely keep up.",
                    "Remember the Hangar Shop: you can upgrade the plasma blaster to Lv.2 or purchase a scatter shotgun or micro-missiles!",
                    "Travel 100 km and clear the path to the dry docks!"
                },
                null),

            // Миссия 6 (Босс Сектора 2)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ГЛУБОКОВОДНЫЙ РУБЕЖ",
                new[]
                {
                    "Мы у главного сухого дока. Где-то под волнами прячется флагманский крейсер «Левиафан».",
                    "Пройди 80 км — выманим эту махину на поверхность!"
                },
                "🚨 Внимание! С верфей сорвался тяжелый крейсер «Левиафан»! Он поливает вертикальными торпедами — следи за верхней кромкой экрана и концентрируй огонь!",
                null,
                "Navigator Barney",
                "📡 COMMS // DEEP TRENCH",
                new[]
                {
                    "We're at the main dry dock. Flagship cruiser Leviathan is lurking somewhere beneath the waves.",
                    "Travel 80 km to lure that behemoth to the surface!"
                },
                "🚨 Warning! Heavy cruiser Leviathan has launched! Watch the top of the screen for torpedoes and focus your fire!"),

            // СЕКТОР 3: ПОЯС ТИТАНОВ (Cryo Nebula)
            // Миссия 7 (Открытие Super Art [E])
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // КРИОГЕННЫЙ ПОЯС",
                new[]
                {
                    "Держись за кварцевый купол, Зигги! Тут летают ледяные глыбы размером с торговый центр!",
                    "Но у меня для тебя сюрприз: гравитационный конденсатор заряжен на 100%!",
                    "Заполни шкалу Ярости и жми [E] — Мега-луч Сингулярности превратит любой ледяной затор в пар!",
                    "Пройди 105 км во льдах!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // CRYO BELT",
                new[]
                {
                    "Hold tight to your quartz dome, Ziggy! Ice boulders the size of city blocks ahead!",
                    "Good news though: the gravitational capacitor is 100% charged!",
                    "Max out your Hype meter and press [E] — the Mega-Laser will vaporize any ice jam!",
                    "Travel 105 km through the frozen belt!"
                },
                null),

            // Миссия 8 (Бронебойные цели)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ЛЕДЯНОЙ ШКВАЛ",
                new[]
                {
                    "Лед становится плотнее брони крейсера! Обычные выстрелы рикошетят от ледяных монолитов.",
                    "Используй тяжелую мортиру «Лоббер», рельсотрон или пушки 2-3 уровня прокачки, чтобы дробить лед с одного залпа!",
                    "Пройди 120 км и вскрой проход к ядру!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // ICE SQUALL",
                new[]
                {
                    "The ice is getting denser than cruiser plating! Standard lasers bounce off.",
                    "Use heavy lobber mortars, railguns, or Lv.2-3 weapons to shatter the ice in one barrage!",
                    "Travel 120 km and breach the path to the core!"
                },
                null),

            // Миссия 9 (Босс Сектора 3)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ЯДРО ТИТАНОВ",
                new[]
                {
                    "Мы подошли к логову ледяного колосса Синдиката. 95 км — и дредноут выйдет на перехват!"
                },
                "⚠️ Матерь божья! «Крио-Титан» на курсе перехвата! Его броня покрыта космическим льдом — бей прямо в реакторное ядро и применяй Супер-луч на [E]!",
                null,
                "Navigator Barney",
                "📡 COMMS // TITAN CORE",
                new[]
                {
                    "We're approaching the icy colossus lair. 95 km until the dreadnought intercepts!"
                },
                "⚠️ Holy smoke! Cryo-Titan is intercepting! Its armor is coated in cosmic ice — fire directly into its core and unleash Super-Laser on [E]!"),

            // СЕКТОР 4: ТУМАННОСТЬ ЗАБЫТЫХ БИТОВ (Void Abyss)
            // Миссия 10 (Гравитационный разлом)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ТУМАННОСТЬ БИТОВ",
                new[]
                {
                    "Слышишь эти басы, Зигги? Древние золотые матрицы Hype Core резонируют в туманности!",
                    "Черные дыры искривляют пространство — рикошетный пиломет «Циркулярка» или самонаведение здесь творят чудеса!",
                    "Пройди 125 км в гравитационном разломе!"
                },
                null,
                null,
                "Navigator Barney",
                "📡 COMMS // BEAT NEBULA",
                new[]
                {
                    "Hear that heavy bass, Ziggy? The ancient gold Hype Core matrices are resonating!",
                    "Black holes are warping space — bouncing buzzsaw blades or homing missiles will work wonders!",
                    "Travel 125 km through the gravitational rift!"
                },
                null),

            // Миссия 11 (Босс Сектора 4)
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ГОРИЗОНТ СОБЫТИЙ",
                new[]
                {
                    "Штаб Синдиката уже на сканерах, но путь преграждает фантомный перехватчик «Жнец». 110 км — и бой начнется!"
                },
                "🚨 Призрачные сигналы на радаре! Фантомный «Жнец» меняет фазы атак и растворяется в пространстве! Держи дистанцию и встречай залпами!",
                null,
                "Navigator Barney",
                "📡 COMMS // EVENT HORIZON",
                new[]
                {
                    "Syndicate headquarters are in range, but phantom interceptor Reaper is blocking us. 110 km to engagement!"
                },
                "🚨 Phantom signatures detected! Reaper phases between dimensions! Keep distance and unleash focused volleys!"),

            // СЕКТОР 5: ЦИТАДЕЛЬ СИНДИКАТА (ФИНАЛ)
            // Миссия 12
            new MissionDialogue(
                "Штурман Барни",
                "📡 СВЯЗЬ // ЦИТАДЕЛЬ СИНДИКАТА (ФИНАЛ)",
                new[]
                {
                    "Зигги... мы в самом эпицентре Синдиката. Гравитационное ядро искажает само пространство и время!",
                    "Либо мы возвращаем музыку, душу и свободу всей галактике прямо сейчас, либо нас разорвет на кварки!",
                    "Вся надежда на тебя и твою летающую тарелку! ПОКАЖИ ИМ НАСТОЯЩИЙ HYPE!"
                },
                "⚡ ГЛАВНАЯ ЦИТАДЕЛЬ СИНДИКАТА АКТИВИРОВАНА! ЖМИ НА ВСЕ КНОПКИ, ЗИГГИ! ЭТО РЕШАЮЩАЯ БИТВА ЗА ВСЕЛЕННУЮ!",
                null,
                "Navigator Barney",
                "📡 COMMS // CITADEL PRIME (FINALE)",
                new[]
                {
                    "Ziggy... we're at the very epicenter of the Syndicate. The gravity core is distorting spacetime itself!",
                    "Either we restore music, soul, and freedom to the whole galaxy right now, or get vaporized!",
                    "All hope rests on you and your saucer! SHOW THEM TRUE HYPE!"
                },
                "⚡ SYNDICATE FLAGSHIP CITADEL ONLINE! FIRE ALL SYSTEMS, ZIGGY! THIS IS THE ULTIMATE BATTLE FOR THE UNIVERSE!")
        };

        public static MissionDialogue GetDialogue(int sectorIndex)
        {
            var idx = Mathf.Clamp(sectorIndex, 0, Dialogues.Length - 1);
            return Dialogues[idx];
        }

        public static readonly LoreEntry[] Entries =
        {
            new LoreEntry(
                "origin",
                "Звездный Пилот Зигги",
                "ПЕРСОНАЖИ",
                "Капитан Зигги — последний вольный астропилот системы Hypeverse. " +
                "Вместо гигантских военных крейсеров он управляет ультра-маневренной летающей тарелкой «Astro-Saucer X-1». " +
                "За прозрачным куполом из армированного кварца Зигги лично держит штурвал, рассчитывая баллистику лазеров " +
                "по наклону корпуса. Когда Синдикат похитил Великие Виниловые Реликвии, Зигги запустил ионный реактор и отправился в погоню!",
                "Star Pilot Ziggy",
                "CHARACTERS",
                "Captain Ziggy is the last free-flying astropilot in the Hypeverse system. " +
                "Instead of clumsy battleships, he pilots the ultra-nimble 'Astro-Saucer X-1'. " +
                "Behind a reinforced quartz dome, Ziggy steers manually, aiming lasers by banking the ship. " +
                "When the Syndicate stole the Great Vinyl Relics, Ziggy fired up his ionic reactor and gave chase!"),

            new LoreEntry(
                "syndicate",
                "Синдикат «Авангард»",
                "ФРАКЦИИ",
                "Межзвездная военно-промышленная мегакорпорация «VANGUARD CORP». " +
                "Они монополизировали поставки энергии в пяти секторах и заменили живую музыку галактики " +
                "холодным машинным гулом фабрик. Похитив 50 виниловых матриц Hype Core, Синдикат запер их в ядрах " +
                "своих пяти флагманских боссов, намереваясь переписать сознание всей галактики.",
                "Vanguard Syndicate",
                "FACTIONS",
                "Interstellar industrial-military megacorporation 'VANGUARD CORP'. " +
                "They monopolized energy grids across five sectors, replacing the galaxy's vibrant music with cold factory drone. " +
                "Having stolen 50 Hype Core vinyl matrices, they locked them within five flagship bosses to rewrite free will."),

            new LoreEntry(
                "hype_core",
                "Реликвии Hype Core & Парирование",
                "ТЕХНОЛОГИИ",
                "Древние золотые винилы Hype Core излучают чистую творческую энергию космоса. " +
                "Синдикат пытался нейтрализовать их, выбрасывая в космос розовую нестабильную антиматерию. " +
                "Зигги модифицировал корпус тарелки Cuphead-поглотителем: касание розовых сфер в вираже " +
                "вызывает квантовый хит-стоп (стоп-кадр 80 мс), мгновенно заряжая шкалу Ярости для EX-ракет и Мега-Лазера!",
                "Hype Core Relics & Parrying",
                "TECHNOLOGY",
                "Ancient golden Hype Core vinyls emit the universe's pure creative resonance. " +
                "The Syndicate attempted to neutralize them by venting volatile pink antimatter hazards. " +
                "Ziggy modified his saucer hull with a Cuphead absorber: touching pink hazards triggers an 80ms quantum hit-stop, instantly fueling the Hype meter for EX-Rockets and Mega-Lasers!"),

            new LoreEntry(
                "boss_1",
                "Досье: Vanguard Dread-Drone",
                "БОССЫ СЕКТОРОВ",
                "Командный сторожевой дрон Сектора 1 (Неоновый коридор). " +
                "Оснащен сдвоенными лазерными турелями и роторными пусковыми установками розовых сфер антиматерии. " +
                "Патрулирует транспортные доки Neon City, перехватывая любые беглые корабли.",
                "Dossier: Vanguard Dread-Drone",
                "SECTOR BOSSES",
                "Command sentinel drone of Sector 1 (Neon Corridor). " +
                "Equipped with twin laser turrets and rotary launchers firing pink antimatter spheres. " +
                "Patrols Neon City transit docks, eliminating any escaping vessels."),

            new LoreEntry(
                "boss_2",
                "Досье: Leviathan Torpedo Cruiser",
                "БОССЫ СЕКТОРОВ",
                "Тяжелый суб-орбитальный крейсер Сектора 2 (Океанский рубеж). " +
                "Скрывается под плотной метановой пеленой океанических глубин. " +
                "Атакует непрерывными вертикальными залпами тяжелых баллистических торпед и веерными гидро-лазерами.",
                "Dossier: Leviathan Torpedo Cruiser",
                "SECTOR BOSSES",
                "Heavy sub-orbital cruiser of Sector 2 (Oceanic Trench). " +
                "Submerged beneath dense methane oceans, surfacing to unleash relentless vertical ballistic torpedoes and hydro-lasers."),

            new LoreEntry(
                "boss_3",
                "Досье: Cryo-Titan Dreadnought",
                "БОССЫ СЕКТОРОВ",
                "Сверхтяжелый дредноут Сектора 3 (Ледяной предел). " +
                "Покрыт броней из абсолютного космического льда толщиной в 4 метра. " +
                "При переходе в ярость (<40% HP) включает форсаж и выбрасывает перекрестные кольца замораживающей плазмы.",
                "Dossier: Cryo-Titan Dreadnought",
                "SECTOR BOSSES",
                "Super-heavy dreadnought of Sector 3 (Cryo Nebula). " +
                "Reinforced with 4-meter thick cosmic absolute-zero ice plating. " +
                "When enraged (<40% HP), engages full overdrive with interlocking rings of freezing plasma."),

            new LoreEntry(
                "boss_4",
                "Досье: Void Reaper",
                "БОССЫ СЕКТОРОВ",
                "Фантомный перехватчик-жнец Сектора 4 (Глубокий космос). " +
                "Использует двигатель искривления пространства, маневрируя на границе сингулярности. " +
                "Выпускает плотные веера высокоскоростных снарядов, требуя от пилота филигранного чувства наклона корпуса.",
                "Dossier: Void Reaper",
                "SECTOR BOSSES",
                "Phantom reaper interceptor of Sector 4 (Void Abyss). " +
                "Employs a dimensional warp drive on the event horizon of a black hole, launching dense bullet-hell spreads."),

            new LoreEntry(
                "boss_5",
                "Досье: Fortress Prime",
                "БОССЫ СЕКТОРОВ",
                "Флагманская космическая цитадель Синдиката (Домашняя База). " +
                "Колоссальный летающий форт со 160 единицами бронестойкости. " +
                "В его центральном реакторе заточены последние похищенные Альбомы. " +
                "Победа над ним уничтожит командную сеть Синдиката и вернет галактике свободу и звук!",
                "Dossier: Fortress Prime",
                "SECTOR BOSSES",
                "Flagship cosmic citadel of the Syndicate (Home Base). " +
                "A colossal dread-fortress with 160 hull points holding the last stolen Vinyl Relics. " +
                "Destroying it dismantles the Syndicate command network and frees the universe!")
        };
    }
}
