// Assets/Scripts/Services/Localization.cs
// Namespace: KANYE.Services
// Lightweight localization system supporting Russian and English.
// Auto-detects player language from platform / browser, saves preference,
// and supports in-game switching with instant UI update.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KANYE.Services
{
    public enum GameLanguage
    {
        Russian,
        English
    }

    public static class Localization
    {
        private const string PrefKey = "KANYE.Language";

        public static GameLanguage CurrentLanguage { get; private set; } = GameLanguage.Russian;

        public static event Action LanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            var saved = PlayerPrefs.GetString(PrefKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
            {
                CurrentLanguage = saved.ToLowerInvariant() == "en" ? GameLanguage.English : GameLanguage.Russian;
                return;
            }

            // Fallback to Unity's system language
            var sys = Application.systemLanguage;
            if (sys == SystemLanguage.Russian || sys == SystemLanguage.Belarusian || sys == SystemLanguage.Ukrainian)
            {
                CurrentLanguage = GameLanguage.Russian;
            }
            else
            {
                CurrentLanguage = GameLanguage.English;
            }
        }

        public static void SetLanguage(GameLanguage lang)
        {
            if (CurrentLanguage == lang)
                return;

            CurrentLanguage = lang;
            PlayerPrefs.SetString(PrefKey, lang == GameLanguage.Russian ? "ru" : "en");
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }

        public static void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == GameLanguage.Russian ? GameLanguage.English : GameLanguage.Russian);
        }

        public static string GetLanguageDisplayName()
        {
            return CurrentLanguage == GameLanguage.Russian ? "РУССКИЙ (RU)" : "ENGLISH (EN)";
        }

        public static string T(string ru, string en)
        {
            return CurrentLanguage == GameLanguage.Russian ? ru : en;
        }

        public static string TFormat(string ruFormat, string enFormat, params object[] args)
        {
            var fmt = CurrentLanguage == GameLanguage.Russian ? ruFormat : enFormat;
            return string.Format(fmt, args);
        }

        public static string GetAchievementTitle(string id, string fallback)
        {
            if (CurrentLanguage == GameLanguage.Russian)
            {
                switch (id)
                {
                    case "stage_deep_space": return "Глубокий космос";
                    case "stage_home_base": return "База Зигги";
                    case "stage_ice_planet": return "Ледяная планета";
                    case "stage_neon_city": return "Неоновый город";
                    case "stage_ocean": return "Океанический сектор";
                    default: return fallback;
                }
            }

            switch (id)
            {
                case "albums_1": return "First Album";
                case "albums_50": return "Full Collection";
                case "clean_run": return "Flawless Run";
                case "hype_full": return "Max Hype";
                case "kill_asteroids_1": return "First Asteroid";
                case "kill_asteroids_100": return "Space Sweeper";
                case "missile_hunter": return "Interceptor";
                case "rank_ace": return "Corridor Ace";
                case "rank_veteran": return "Hype Veteran";
                case "rank_warrior_1": return "Warrior I";
                case "rank_warrior_2": return "Warrior II";
                case "rank_warrior_3": return "Warrior III";
                case "second_chance": return "Second Chance";
                case "stage_deep_space": return "Deep Space";
                case "stage_home_base": return "Home Base";
                case "stage_ice_planet": return "Ice Planet";
                case "stage_neon_city": return "Neon City";
                case "stage_ocean": return "Ocean Sector";
                default: return fallback;
            }
        }

        public static string GetAchievementDesc(string id, string fallback)
        {
            if (CurrentLanguage == GameLanguage.Russian)
            {
                switch (id)
                {
                    case "stage_deep_space": return "Дойди до сектора Глубокий космос.";
                    case "stage_home_base": return "Дойди до сектора База Зигги.";
                    case "stage_ice_planet": return "Дойди до сектора Ледяная планета.";
                    case "stage_neon_city": return "Начни полет в секторе Неоновый город.";
                    case "stage_ocean": return "Дойди до Океанического сектора.";
                    default: return fallback;
                }
            }

            switch (id)
            {
                case "albums_1": return "Collect 1 Hype Core vinyl relic.";
                case "albums_50": return "Collect all 50 Hype Core relics.";
                case "clean_run": return "Complete a mission without taking damage.";
                case "hype_full": return "Fill the Hype meter to maximum.";
                case "kill_asteroids_1": return "Destroy 1 asteroid.";
                case "kill_asteroids_100": return "Destroy 100 asteroids.";
                case "missile_hunter": return "Shoot down an enemy guided missile.";
                case "rank_ace": return "Shoot down 150 hostile targets.";
                case "rank_veteran": return "Shoot down 80 hostile targets.";
                case "rank_warrior_1": return "Shoot down 5 hostile targets.";
                case "rank_warrior_2": return "Shoot down 15 hostile targets.";
                case "rank_warrior_3": return "Shoot down 40 hostile targets.";
                case "second_chance": return "Revive after watching an ad broadcast.";
                case "stage_deep_space": return "Reach the Deep Space sector.";
                case "stage_home_base": return "Reach the Home Base sector.";
                case "stage_ice_planet": return "Reach the Ice Planet sector.";
                case "stage_neon_city": return "Start your flight in the Neon City sector.";
                case "stage_ocean": return "Reach the Ocean sector.";
                default: return fallback;
            }
        }
    }
}
