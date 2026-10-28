# Контекст проекта Astro K Flight

Документ для людей и ИИ-ассистентов, которые продолжают эту игру. Сначала прочитай его целиком. Не добавляй механики, которых нет в задании и которые продюсер явно не подтвердил.

Дата снимка: 19 октября 2026.  
GitHub: https://github.com/fefefe14021/astro-k-flight.git  
Снаряд, астероид, Album, шкала Hype и пакет Input System входят в репозиторий вместе с этим контекстом. Друг клонирует https://github.com/fefefe14021/astro-k-flight.git и открывает корень репозитория в Unity `6000.6.6f1`.

## Роли

Продюсер в этом диалоге — тимлид. Ассистент пишет код и инструкции для Unity, но не расширяет геймплей сам.

Команда из паспорта (Практика 1, 10.09.2026, группы ПА-01 и ПА-02, команда 6):

| Участник | Роль |
|---|---|
| Звездин Дмитрий Дмитриевич | Game Designer / Producer |
| Портнов Степан Николаевич | Gameplay Developer |
| Жужугов Дмитрий Ярославович | UI/UX Developer |
| Миронов Егор Павлович | Technical Developer |

## Жёсткие правила

1. Не выдумывать механики. Если приёма нет в ТЗ и продюсер его не подтвердил, сначала спросить.
2. Если ТЗ противоречиво или нужно архитектурное решение, дать 2–3 варианта и дождаться выбора. Так уже отложено хранение данных между сценами: синглтона и DontDestroyOnLoad нет.
3. Код делать расширяемым: сервисы, события, ScriptableObject и префабы-конфиги, когда для этого есть подтверждённая механика. Идеи сверх ТЗ предлагать отдельно и не реализовывать до явного согласия.
4. Одобренные механики напоминать фиксировать в roadmap и README.
5. В каждом коде указывать путь файла, неймспейс и GameObject, на который вешается скрипт.
6. Неймспейс кода остаётся `KANYE`. Менять его нельзя, пока продюсер не выберет другой вариант. Игроку нельзя показывать имя реального человека и геополитический маршрут из исходного паспорта.
7. Лидербордов нет.
8. Платформа релиза: Яндекс Игры, WebGL. Сборка ещё не делалась, SDK рекламы не подключался.

## Задание

Жанр: 2D arcade, вид сбоку, пиксель-арт, полёт и стрельба.  
Движок: Unity 6 LTS.  
Публичное направление сеттинга: абстрактный sci-fi. Рабочее имя в обсуждении — «Astro K: Flight Home». В коде и неймспейсе по-прежнему `KANYE`.

Исходное имя паспорта «KANYE: FLIGHT TO RUSSIA» и маршрут США → Атлантика → Арктика → подлёт к России → Россия в игре не используются. Структура пяти этапов сохраняется, названия для игрока:

1. Neon City
2. Ocean
3. Ice Planet
4. Deep Space
5. Home Base

Эти этапы как контент ещё не сделаны. Сейчас есть одна сцена `Gameplay` без смены биома.

### Циклы

Core loop: лететь → стрелять и уклоняться → собирать ресурсы → улучшать корабль → пережить волну → двигаться дальше.

Gameplay loop одной партии: начать этап → уклоняться и уничтожать помехи → собирать Albums и Energy, заполняя Hype → использовать Hype Mode и улучшения → завершить этап, получить награду, перейти дальше.

Meta loop: достижения, коллекция скинов, ежедневная награда, магазин. Лидерборд выключен.

### Монетизация, только план

- Rewarded: одно воскрешение после поражения, буст перед стартом, удвоение награды после сессии.
- Interstitial: после завершения этапа.
- Магазин: косметика, скины корабля, визуальные эффекты. Не обязательные для прохождения предметы.

### Аудитория

Возраст 11–30, casual, сессия 5–10 минут.

### FTUE, пять шагов

1. Кнопка Play начинает первый этап.
2. Короткая подсказка: движение и кнопка стрельбы.
3. Игрок двигается, стреляет и уничтожает астероид.
4. Первый Album и видимое заполнение шкалы Hype.
5. Дальше обычная волна и основной цикл.

Шаги 1–4 частично есть в сцене `Gameplay`, но это ещё не обучение: нет подсказок, нет отдельного первого этапа Neon City, нет перехода «обучение закончилось». Шаг 5 не начат.

## Два каталога

Их нельзя путать.

| Путь | Что это |
|---|---|
| `C:\Users\dmitr\astro-k-flight` | Git-репозиторий, его и пушат на GitHub |
| `C:\Users\dmitr\AsteroidShooter` | Локальный Unity-проект, в котором продюсер нажимает Play |

Продюсер выбрал работать в `AsteroidShooter`. Новые скрипты пишутся в репозиторий и копируются в `AsteroidShooter`. Сцена, собранная руками в редакторе, сохраняется в `AsteroidShooter` и копируется обратно в репозиторий.

В репозитории лежат неотслеживаемые каталоги `Packages.localstub` и `ProjectSettings.localstub`. Это временные копии. В коммит их не добавлять.

## Настройки проекта в репозитории

Файл `ProjectSettings/ProjectVersion.txt`:

- Unity `6000.6.6f1` (`d20c9ac777da`)
- `companyName`: DefaultCompany
- `productName`: astro-k-flight
- `activeInputHandler`: `2` (старый Input Manager и новая Input System вместе)

Сцены в Build Settings, по порядку: `Assets/Scenes/Bootstrap.unity`, `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Gameplay.unity`.

`Packages/manifest.json` содержит `com.unity.inputsystem` `1.20.1`, `com.unity.ugui` `2.6.0` и модули движка. Для Unity `6000.6` нужна именно `1.20.1`: более старый Input System даёт ошибки компиляции `CS0619`. Код корабля использует `UnityEngine.InputSystem`.

`.gitignore` — стандартный Unity-шаблон. В конец дополнительно добавлены правила без ведущего слэша: `[Ll]ibrary/`, `[Tt]emp/`, `[Oo]bj/`, `[Bb]uild/`.

README в корне описывает, как открыть проект в Unity Hub. Роадмап игры там не ведётся.

## Принятые решения

1. Неймспейс и идентификаторы в коде остаются `KANYE`. Цензура касается только текста, который видит игрок.
2. Ввод сразу через Unity Input System: клавиатура и экранные стик плюс кнопка огня. Один выстрел на одно нажатие. Удержание кнопки серию не даёт. Отдельной задержки между выстрелами нет.
3. Идём по шагам. Состояние забега между сценами не выбирали. `HypeState` живёт только внутри `Gameplay` и не помечен DontDestroyOnLoad.
4. Снаряд летит вправо (`Vector2.right`), от дула со смещением `(0.8, 0)`. Скорость 12, время жизни 2 секунды. Префаб: `Assets/Prefabs/Projectile.prefab`.
5. Один астероид. Попадание уничтожает и астероид, и снаряд. За попадание даётся 1 Album и 25 Hype из 100. Шкала заполняется плавно.
6. Корабль в сохранённой сцене стоял в `(1282, 732)` и был вне камеры. Его вернули в `(0, 0, 0)`. Камера в `(0, 0, -10)`, ортографическая, size 5.

## Что уже сделано

### В git, коммиты

- `42a5f7b` — пустой репозиторий с Unity `.gitignore` и README.
- `7acb62f` — папки ассетов из `AsteroidShooter`: сцены `Bootstrap`, `MainMenu`, `Gameplay`, скрипты меню и движение на старом Input.
- `a044aae` — Input System: `PlayerControls`, `ShipInput`, движение корабля, заглушка спрайта, экранные кнопки.
- `3e52561` — чужой коммит: настройки Unity Hub и урезанный `Packages/manifest.json`.
- `7d9b25c` — сцена `Gameplay`: объекты `Player` и `TouchControls`.

### Геймплейный срез, который должен быть в том же коммите, что и эта правка контекста

Файлы сверены с `AsteroidShooter`: содержимое совпадает.

- `Assets/Scripts/Gameplay/ShipShooter.cs` — выстрел по `FirePressed`.
- `Assets/Scripts/Gameplay/Projectile.cs` — кинематический снаряд, триггер, полёт через `Rigidbody2D.MovePosition`.
- `Assets/Prefabs/Projectile.prefab`.
- `Assets/Scripts/Gameplay/Asteroid.cs` — серый квадрат, при попадании снаряда выдаёт Album и уничтожается.
- `Assets/Scripts/Gameplay/HypeState.cs` — Album и Hype текущей сцены.
- `Assets/Scripts/UI/HypeHud.cs` — надпись `Album N` и полоска `Hype`.
- `Assets/Scripts/Gameplay/GameplayBootstrap.cs` — при входе в сцену `Gameplay` создаёт `HypeState` и астероид в `(4, 0, 0)`, если их ещё нет.
- `Assets/Scripts/Editor/GameplaySceneRepair.cs` — если в редакторе открыта `Gameplay` и астероида нет, добавляет его и сохраняет сцену.
- Обновлённая `Assets/Scenes/Gameplay.unity` с объектом `Asteroid`.

Почему астероид мог не появиться у продюсера: объект был записан в файл сцены, но открытый редактор держал старую копию в памяти, а спрайт создаётся кодом при включении объекта. Поэтому добавлен запасной спавн при запуске сцены.

### Скрипты и куда они повешены

| Файл | Неймспейс | Куда вешать |
|---|---|---|
| `Assets/Resources/PlayerControls.inputactions` | — | `ShipInput` загружает сам из Resources |
| `Assets/Scripts/Services/SceneLoader.cs` | `KANYE.Services` | Объект в главном меню, рядом с кнопкой Play |
| `Assets/Scripts/UI/MainMenuUI.cs` | `KANYE.UI` | Canvas главного меню. Кнопка Play вызывает `SceneLoader.LoadGameplay` |
| `Assets/Scripts/Gameplay/ShipInput.cs` | `KANYE.Gameplay` | `Player` |
| `Assets/Scripts/Gameplay/PlayerController.cs` | `KANYE.Gameplay` | `Player`. Двигает `Rigidbody2D` внутри границ экрана |
| `Assets/Scripts/Gameplay/PlaceholderShipVisual.cs` | `KANYE.Gameplay` | `Player`, пока нет настоящего спрайта |
| `Assets/Scripts/Gameplay/ShipShooter.cs` | `KANYE.Gameplay` | `Player` |
| `Assets/Scripts/Gameplay/Projectile.cs` | `KANYE.Gameplay` | Префаб `Projectile` |
| `Assets/Scripts/Gameplay/Asteroid.cs` | `KANYE.Gameplay` | Объект `Asteroid` |
| `Assets/Scripts/Gameplay/HypeState.cs` | `KANYE.Gameplay` | Объект `HypeState`, создаётся кодом |
| `Assets/Scripts/UI/TouchFlightControls.cs` | `KANYE.UI` | `TouchControls`. Стик слева снизу, огонь справа снизу |
| `Assets/Scripts/UI/HypeHud.cs` | `KANYE.UI` | Объект `HypeHud`, создаётся кодом |
| `Assets/Scripts/Editor/GameplaySceneRepair.cs` | `KANYE.EditorTools` | Только редактор, на объект не вешается |

Клавиатура: WASD и стрелки, огонь — пробел. Экранные элементы пишут в `<Gamepad>/leftStick` и `<Gamepad>/buttonSouth`. Мышь к выстрелу не привязана, чтобы стик не стрелял при перетаскивании.

`Assets/Art` и `Assets/Audio` по сути пустые: в git у них только `.meta`. В `Assets/Resources` лежат `PlayerControls`, `GameRulesConfig` и ачивки.

`Bootstrap` почти пустой: камера, без загрузочного сервиса. Меню переводит в `Gameplay` напрямую.

## Срез участника 1, ветка feature/systems-and-docs

Ветки `dev` в репозитории нет. Общая рабочая ветка: `feature/systems-and-docs`. В `main` этот срез не влит.

Участник 1 делает код систем. Участник 2 делает визуал меню, панель ачивок и три документа. Панели Shop, Achievements и Settings не заведены отдельными сценами.

Сохранения: `SaveManager` — синглтон с `DontDestroyOnLoad`. Данные пишутся в `PlayerPrefs` ключом `KANYE.SaveData` через `JsonUtility`. На WebGL Unity кладёт PlayerPrefs в хранилище браузера. В сохранении лежат монеты, прогресс ачивок, экипированный скин и список открытых скинов. Скин по умолчанию — `default`. Шкала Hype и Album между запусками не сохраняются: это состояние одного забега.

Ачивки: ScriptableObject `AchievementData` в `Assets/Resources/Achievements`. `AchievementManager.AddProgress(id, amount)` и `UnlockAchievement(id)`. Событие `Unlocked` нужно участнику 2 для всплывающего окна. Готовые id:

| id | Условие | Кто вызывает |
|---|---|---|
| `kill_asteroids_1` | 1 уничтоженный астероид | само, из `GameRules` |
| `kill_asteroids_100` | 100 астероидов | само |
| `albums_1` | 1 Album | само |
| `albums_50` | 50 Album | само |
| `hype_full` | шкала Hype дошла до 100 | само |
| `stage_neon_city` | вход в сцену `Gameplay` | само |
| `stage_ocean` | этап Ocean | `GameRules.NotifyStageReached("stage_ocean")`, когда появятся этапы |
| `stage_ice_planet` | этап Ice Planet | так же |
| `stage_deep_space` | этап Deep Space | так же |
| `stage_home_base` | этап Home Base | так же |

Правила забега: один сбитый астероид даёт 10 монет, 1 Album и 25 Hype, как и раньше. Число монет лежит в `Assets/Resources/GameRulesConfig.asset`.

Меню: `SceneLoader.LoadGameplay`, `LoadMainMenu` и `Quit`. `MenuPanelSwitcher.Show("main")` переключает панели внутри одной сцены. Пока участник 2 не поставил `MenuPanelSwitcher`, в пустом `MainMenu` сами появляются кнопки «Играть» и «Выход». В `Gameplay` есть временная кнопка «Назад». Её объект называется `TemporaryBackButton`; когда появится своя кнопка, этот объект можно удалить и звать `SceneLoader.ReturnToMainMenu`.

Кнопки участника 2 вешаются на `SaveManager.Instance.AddCoins`, `AchievementManager.Instance.AddProgress` и `MenuPanelSwitcher.Show`.

## Что предстоит

Не начинать следующий пункт, пока продюсер его не назовёт. Ближайший несделанный кусок FTUE — шаг 5: после первого астероида идёт обычная волна, а не один заранее стоящий объект. До этого в ТЗ ещё нет реализации:

- подсказка управления на экране;
- скролл и автополёт вправо, сейчас корабль только двигается внутри рамки;
- Energy как второй ресурс;
- Hype Mode как действие игрока, а не только полоска;
- улучшения корабля;
- волны, новые враги, боссы, комбо;
- пять этапов и переход между ними;
- награда в конце этапа;
- достижения, скины, ежедневная награда, магазин;
- реклама Яндекс SDK: rewarded и interstitial;
- WebGL-сборка под Яндекс Игры;
- пиксель-арт вместо цветных прямоугольников.

Предлагать эти пункты по одному. Не писать сразу весь список в код.
