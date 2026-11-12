// Assets/Scripts/Editor/GameSmokeTest.cs
// Namespace: KANYE.EditorTools
// Automated batchmode validation test for Astro K: Flight systems.

using System;
using KANYE.Audio;
using KANYE.Gameplay;
using KANYE.Services;
using KANYE.Systems;
using KANYE.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KANYE.EditorTools
{
    public static class GameSmokeTest
    {
        public static void RunAllTests()
        {
            Debug.Log("==================================================");
            Debug.Log("[SMOKE TEST] Starting Astro K Flight Validation...");
            Debug.Log("==================================================");

            var passed = 0;
            var failed = 0;

            Test("1. SaveManager Data & Defaults", () =>
            {
                var go = new GameObject("SaveTest");
                var sm = go.AddComponent<SaveManager>();
                sm.Load();
                if (sm.Data == null) throw new Exception("SaveManager.Data is null");
                sm.AddCoins(50);
                if (sm.Data.coins < 50) throw new Exception("AddCoins failed");
                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Test("2. GameRules & Scoring Calculations", () =>
            {
                GameRules.ResetMissionStats();
                GameRules.NotifyParry();
                if (GameRules.ParriesThisRun != 1) throw new Exception("Parry count mismatch");

                var gradeS = GameRules.CalculateGrade(3, 3, 2, 45f);
                if (gradeS != "S") throw new Exception($"Expected Grade S, got {gradeS}");

                var gradeA = GameRules.CalculateGrade(2, 3, 0, 80f);
                if (gradeA != "A") throw new Exception($"Expected Grade A, got {gradeA}");
            }, ref passed, ref failed);

            Test("3. Mission Catalog & Sectors", () =>
            {
                if (MissionCatalog.All == null || MissionCatalog.All.Length != 5)
                    throw new Exception($"Expected 5 sectors, got {MissionCatalog.All?.Length}");

                for (var i = 0; i < MissionCatalog.All.Length; i++)
                {
                    var m = MissionCatalog.All[i];
                    if (string.IsNullOrEmpty(m.Title) || m.AlbumsRequired <= 0)
                        throw new Exception($"Mission {i} definition invalid");
                }
            }, ref passed, ref failed);

            Test("4. PixelUiAtlas Sprites & Slicing", () =>
            {
                if (PixelUiAtlas.Panel == null) throw new Exception("PixelUiAtlas.Panel is null");
                if (PixelUiAtlas.SubPanel == null) throw new Exception("PixelUiAtlas.SubPanel is null");
                if (PixelUiAtlas.ButtonPrimary == null) throw new Exception("PixelUiAtlas.ButtonPrimary is null");
                if (PixelUiAtlas.ButtonSecondary == null) throw new Exception("PixelUiAtlas.ButtonSecondary is null");
                if (PixelUiAtlas.ButtonDanger == null) throw new Exception("PixelUiAtlas.ButtonDanger is null");
            }, ref passed, ref failed);

            Test("5. Procedural AudioManager & Synthesizers", () =>
            {
                var go = new GameObject("AudioTest");
                var am = go.AddComponent<AudioManager>();
                AudioManager.SetVolume(0.8f);
                if (Mathf.Abs(AudioManager.MasterVolume - 0.8f) > 0.01f)
                    throw new Exception("MasterVolume setter failed");
                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Test("6. Gameplay Scene Asset Verification", () =>
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                if (!scene.IsValid()) throw new Exception("Gameplay.unity could not be opened");

                var player = GameObject.Find("Player");
                if (player == null) throw new Exception("Player object missing in Gameplay scene");

                var rb = player.GetComponent<Rigidbody2D>();
                if (rb == null) throw new Exception("Player Rigidbody2D missing");

                var shooter = player.GetComponent<ShipShooter>();
                if (shooter == null) throw new Exception("Player ShipShooter missing");

                var controller = player.GetComponent<PlayerController>();
                if (controller == null) throw new Exception("Player PlayerController missing");
            }, ref passed, ref failed);

            Test("6b. All Other Scenes Verification", () =>
            {
                string[] scenes = { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Shop.unity", "Assets/Scenes/Achievements.unity" };
                foreach (var sc in scenes)
                {
                    var s = EditorSceneManager.OpenScene(sc, OpenSceneMode.Single);
                    if (!s.IsValid()) throw new Exception($"{sc} could not be opened");
                }
            }, ref passed, ref failed);

            Test("7. Boss Configurations (All 5 Sectors)", () =>
            {
                for (var i = 0; i < 5; i++)
                {
                    var go = new GameObject($"Boss_{i}");
                    var boss = go.AddComponent<BossController>();
                    boss.Configure(i);

                    if (string.IsNullOrEmpty(boss.BossName)) throw new Exception($"Boss {i} has empty name");
                    if (boss.MaxHealth <= 0) throw new Exception($"Boss {i} has invalid health");

                    boss.TakeDamage(10);
                    if (boss.CurrentHealth != boss.MaxHealth - 10) throw new Exception($"Boss {i} damage calculation failed");

                    UnityEngine.Object.DestroyImmediate(go);
                }
            }, ref passed, ref failed);

            Test("8. Parry Hazard Mechanics", () =>
            {
                var go = new GameObject("ParryTest");
                var hazard = go.AddComponent<ParryHazard>();
                hazard.Launch(Vector2.left, 4.0f);
                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Test("9. EX Missile Rocket", () =>
            {
                var go = new GameObject("EXTest");
                var ex = go.AddComponent<EXMissile>();
                ex.Launch(Vector2.right);
                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Test("10. All 5 Scene Paths in EditorBuildSettings", () =>
            {
                var scenes = EditorBuildSettings.scenes;
                if (scenes.Length < 3) throw new Exception($"Too few build scenes: {scenes.Length}");
                foreach (var s in scenes)
                {
                    if (!System.IO.File.Exists(s.path))
                        throw new Exception($"Scene file missing on disk: {s.path}");
                }
            }, ref passed, ref failed);

            Test("11. Boss Combat Simulation & Projectile Salvo", () =>
            {
                var boss = BossController.SpawnBoss(0);
                if (boss == null) throw new Exception("SpawnBoss returned null");

                // Execute all 4 attack phases (twin blaster, parry volley, torpedo salvo, spread blaster)
                boss.ExecuteNextAttack();
                boss.ExecuteNextAttack();
                boss.ExecuteNextAttack();
                boss.ExecuteNextAttack();

                var missiles = UnityEngine.Object.FindObjectsByType<Missile>(FindObjectsSortMode.None);
                var parries = UnityEngine.Object.FindObjectsByType<ParryHazard>(FindObjectsSortMode.None);
                var lasers = UnityEngine.Object.FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);

                if (missiles.Length == 0) throw new Exception("Boss torpedo salvo did not spawn missiles");
                if (parries.Length == 0) throw new Exception("Boss parry volley did not spawn parry orbs");
                if (lasers.Length == 0) throw new Exception("Boss blasters did not spawn lasers");

                // Clean up spawned test entities
                foreach (var m in missiles) UnityEngine.Object.DestroyImmediate(m.gameObject);
                foreach (var p in parries) UnityEngine.Object.DestroyImmediate(p.gameObject);
                foreach (var l in lasers) UnityEngine.Object.DestroyImmediate(l.gameObject);
                UnityEngine.Object.DestroyImmediate(boss.gameObject);
            }, ref passed, ref failed);

            Test("12. FTUE Tutorial Dummy & Fail-Safe Progression", () =>
            {
                TutorialHints.ResetTutorial();
                if (!TutorialHints.IsTutorialActive) throw new Exception("Tutorial should be active after ResetTutorial");

                var go = new GameObject("TutorialTest");
                var th = go.AddComponent<TutorialHints>();

                // Simulate collision without destruction
                TutorialHints.NotifyAsteroidCollided();

                // Test complete / skip
                th.CompleteTutorial();
                if (TutorialHints.IsTutorialActive) throw new Exception("Tutorial should be inactive after CompleteTutorial");
                if (!TutorialHints.IsTutorialCompleted) throw new Exception("IsTutorialCompleted should be true");

                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Debug.Log("==================================================");
            Debug.Log($"[SMOKE TEST FINISHED] Passed: {passed}, Failed: {failed}");
            Debug.Log("==================================================");

            if (failed > 0)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }

        private static void Test(string name, Action action, ref int passed, ref int failed)
        {
            try
            {
                action();
                Debug.Log($"[PASS] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FAIL] {name}: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }
        }
    }
}
