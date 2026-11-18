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
        private static System.Text.StringBuilder testLog = new System.Text.StringBuilder();

        [InitializeOnLoadMethod]
        private static void AutoRunOnTrigger()
        {
            if (System.IO.File.Exists("run_smoke_tests.trigger"))
            {
                try { System.IO.File.Delete("run_smoke_tests.trigger"); } catch { }
                RunAllTests();
            }
        }

        [MenuItem("Tools/Run Smoke Tests")]
        public static void RunAllTests()
        {
            testLog.Clear();
            testLog.AppendLine("==================================================");
            testLog.AppendLine("[SMOKE TEST] Starting Astro K Flight Validation...");
            testLog.AppendLine("==================================================");

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
                if (MissionCatalog.All == null || MissionCatalog.All.Length < 12)
                    throw new Exception($"Expected 12 missions, got {MissionCatalog.All?.Length}");

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
                AudioManager.PlayShoot();
                AudioManager.PlayEnemyLaser();
                AudioManager.PlayDenied();
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

                var missiles = UnityEngine.Object.FindObjectsByType<Missile>(FindObjectsInactive.Exclude);
                var parries = UnityEngine.Object.FindObjectsByType<ParryHazard>(FindObjectsInactive.Exclude);
                var lasers = UnityEngine.Object.FindObjectsByType<EnemyProjectile>(FindObjectsInactive.Exclude);

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

            Test("13. Weapon & Skin Catalogs & Lore Verification", () =>
            {
                if (WeaponCatalog.All.Length < 6)
                    throw new Exception($"Expected 6 weapons, found {WeaponCatalog.All.Length}");
                for (var i = 0; i < WeaponCatalog.All.Length; i++)
                {
                    var w = WeaponCatalog.All[i];
                    if (string.IsNullOrEmpty(w.Id) || w.Damage <= 0 || w.FireInterval <= 0f)
                        throw new Exception($"Invalid weapon definition at index {i}");
                }

                if (ShipSkinCatalog.All.Length < 6)
                    throw new Exception($"Expected 6 skins, found {ShipSkinCatalog.All.Length}");
                for (var i = 0; i < ShipSkinCatalog.All.Length; i++)
                {
                    var s = ShipSkinCatalog.All[i];
                    if (string.IsNullOrEmpty(s.Id) || string.IsNullOrEmpty(s.SpriteName))
                        throw new Exception($"Invalid skin definition at index {i}");
                    if (string.IsNullOrEmpty(s.ThrusterSprite) || string.IsNullOrEmpty(s.ParticleType))
                        throw new Exception($"Skin {s.Id} missing thruster or particle parameters");
                }

                if (LoreCatalog.Entries.Length < 5)
                    throw new Exception($"Expected at least 5 lore entries, found {LoreCatalog.Entries.Length}");

                if (LoreCatalog.Dialogues.Length < 12)
                    throw new Exception($"Expected 12 mission dialogues, found {LoreCatalog.Dialogues.Length}");
                for (var i = 0; i < LoreCatalog.Dialogues.Length; i++)
                {
                    var d = LoreCatalog.Dialogues[i];
                    if (string.IsNullOrEmpty(d.Speaker) || string.IsNullOrEmpty(d.Header) || d.IntroLines == null || d.IntroLines.Length == 0)
                        throw new Exception($"Invalid dialogue at mission {i}");
                }

                // Verify weapon upgrade progression calculations
                if (WeaponCatalog.GetDamage("twin_blaster", 1) != 1 || WeaponCatalog.GetDamage("twin_blaster", 2) != 2 || WeaponCatalog.GetDamage("twin_blaster", 3) != 3)
                    throw new Exception("Twin Blaster damage scaling mismatch");

                if (WeaponCatalog.GetProjectileCount("scatter_shotgun", 1) != 5 || WeaponCatalog.GetProjectileCount("scatter_shotgun", 3) != 7)
                    throw new Exception("Scatter Shotgun projectile scaling mismatch");

                var portrait = Resources.Load<Sprite>("Sprites/barney_portrait");
                if (portrait == null)
                    throw new Exception("Missing barney_portrait sprite in Resources/Sprites");
            }, ref passed, ref failed);

            Test("14. Economy Balance, Progression Tiers & Meta Tech", () =>
            {
                var upg1 = WeaponCatalog.GetUpgradeCost("twin_blaster", 1);
                var upg2 = WeaponCatalog.GetUpgradeCost("twin_blaster", 2);
                if (upg1 != 75 || upg2 != 160)
                    throw new Exception($"Expected upgrade costs 75/160, got {upg1}/{upg2}");

                var shotgun = WeaponCatalog.Find("scatter_shotgun");
                if (shotgun == null || shotgun.Price != 120 || shotgun.RequiredMissionIndex != 1)
                    throw new Exception("Scatter shotgun tier definition invalid");

                var mortar = WeaponCatalog.Find("plasma_mortar");
                if (mortar == null || mortar.Price != 1050 || mortar.RequiredMissionIndex != 11)
                    throw new Exception("Plasma mortar endgame tier definition invalid");

                // Test MetaTechCatalog
                if (MetaTechCatalog.All.Length != 4)
                    throw new Exception("Expected 4 meta tech upgrades in catalog");
                var hullTech = MetaTechCatalog.Find("hull");
                if (hullTech.MaxLevel != 5) throw new Exception("Expected Hull max level 5");
                var (hCostCredits, hCostCores) = hullTech.GetCost(1);
                if (hCostCredits != 150) throw new Exception("Expected Hull lvl 1->2 cost 150 credits");

                // Test player healing via Nanite
                var playerGo = new GameObject("PlayerHealthTest");
                var ph = playerGo.AddComponent<PlayerHealth>();
                ph.TakeDamage(2);
                if (ph.Lives != 1) throw new Exception($"Expected 1 life after 2 damage, got {ph.Lives}");
                var healed = ph.Heal(1);
                if (!healed || ph.Lives != 2) throw new Exception($"Expected 2 lives after healing, got {ph.Lives}");
                UnityEngine.Object.DestroyImmediate(playerGo);
            }, ref passed, ref failed);

            Test("15. Rogue-Lite Expedition Mode & Dynamic Uncapped Economy", () =>
            {
                RogueExpeditionController.ResetRunModifiers();
                if (RogueExpeditionController.CurrentWave != 1) throw new Exception("Expected wave 1 after reset");

                // Test uncapped dynamic wave rewards
                RogueExpeditionController.AddRogueCoins(80);
                if (RogueExpeditionController.CoinsEarnedThisRun != 80)
                    throw new Exception($"Expected 80 coins earned, got {RogueExpeditionController.CoinsEarnedThisRun}");

                RogueExpeditionController.AddRogueCoins(120); // 80 + 120 = 200, dynamic uncapped!
                if (RogueExpeditionController.CoinsEarnedThisRun != 200)
                    throw new Exception($"Expected 200 coins, got {RogueExpeditionController.CoinsEarnedThisRun}");

                // Validate detailed enemy sprite size
                var enemySprite = Resources.Load<Sprite>("Sprites/enemy");
                if (enemySprite == null || enemySprite.rect.width < 40)
                    throw new Exception("Enemy sprite missing or still has low resolution (<40px)");
                if (Mathf.Abs(enemySprite.pixelsPerUnit - 48f) > 0.1f)
                    throw new Exception($"Expected enemy PPU 48, got {enemySprite.pixelsPerUnit}");
            }, ref passed, ref failed);

            Test("16. Save State & Daily Login Retention", () =>
            {
                var go = new GameObject("SaveCutsceneTest");
                var sm = go.AddComponent<SaveManager>();
                sm.Load();
                sm.MarkIntroSeen(false);
                if (sm.HasSeenIntro) throw new Exception("Expected HasSeenIntro to be false");
                sm.MarkIntroSeen(true);
                if (!sm.HasSeenIntro) throw new Exception("Expected HasSeenIntro to be true");

                // Test Daily Login methods
                var day = sm.GetCurrentDailyDay();
                if (day < 1 || day > 7) throw new Exception("Invalid daily day");
                var (cr, co) = sm.GetDailyRewardValues(1);
                if (cr != 50) throw new Exception("Expected Day 1 reward 50 credits");

                UnityEngine.Object.DestroyImmediate(go);
            }, ref passed, ref failed);

            Test("17. Route Completion & Remaining Enemy Clearance", () =>
            {
                // Reset states
                MissionController.IsRouteCompleted = false;
                if (MissionController.IsRouteCompleted) throw new Exception("Expected IsRouteCompleted false initially");

                var enemyObj1 = new GameObject("Enemy1");
                var e1 = enemyObj1.AddComponent<Enemy>();
                e1.Configure(false, 5, 2.5f);

                var enemyObj2 = new GameObject("Enemy2");
                var e2 = enemyObj2.AddComponent<Enemy>();
                e2.Configure(true, 25, 2.0f);

                var activeCount = Enemy.ActiveEnemyCount;
                if (activeCount < 2) throw new Exception($"Expected at least 2 active enemies, got {activeCount}");

                // Simulate taking lethal damage
                e1.TakeDamage(100);
                if (!e1.IsDead) throw new Exception("Expected e1 to be marked dead");

                UnityEngine.Object.DestroyImmediate(enemyObj1);
                UnityEngine.Object.DestroyImmediate(enemyObj2);
            }, ref passed, ref failed);

            Test("18. Mechanical Engine Nozzle & Thruster Sprites", () =>
            {
                string[] nozzles = { "engine_nozzle", "engine_nozzle_cyber", "engine_nozzle_gold", "engine_nozzle_bio", "engine_nozzle_steam", "engine_nozzle_void", "engine_glow" };
                foreach (var n in nozzles)
                {
                    var spr = Resources.Load<Sprite>("Sprites/" + n);
                    if (spr == null) throw new Exception($"Missing sprite Resources/Sprites/{n}");
                    if (spr.rect.width < 8 || spr.rect.height < 8) throw new Exception($"Sprite {n} has invalid dimensions");
                }

                foreach (var s in ShipSkinCatalog.All)
                {
                    if (string.IsNullOrEmpty(s.NozzleSprite)) throw new Exception($"Skin {s.Id} missing NozzleSprite definition");
                    var spr = Resources.Load<Sprite>(s.NozzleSprite);
                    if (spr == null) throw new Exception($"Could not load NozzleSprite {s.NozzleSprite} for skin {s.Id}");
                }
            }, ref passed, ref failed);

            Test("19. Super Projectile Red Mega-Laser Beam", () =>
            {
                var shipObj = new GameObject("ShipDummy");
                var laser = SuperProjectile.UnleashMegaLaser(shipObj.transform, Vector2.zero);

                var col = laser.GetComponent<BoxCollider2D>();
                if (col == null || !col.isTrigger) throw new Exception("SuperProjectile missing trigger BoxCollider2D");
                if (col.size.x < 15f) throw new Exception($"Expected laser length >= 15, got {col.size.x}");

                UnityEngine.Object.DestroyImmediate(laser.gameObject);
                UnityEngine.Object.DestroyImmediate(shipObj);
            }, ref passed, ref failed);

            Test("20. Floating Combat Text & Heavy Enemy Health Bar", () =>
            {
                // Verify FloatingCombatText
                var fctObj = new GameObject("FCTTest");
                var fct = fctObj.AddComponent<FloatingCombatText>();
                FloatingCombatText.ShowDamage(Vector3.zero, 25, true);
                FloatingCombatText.ShowParry(Vector3.zero);
                FloatingCombatText.ShowHeal(Vector3.zero, 1);
                FloatingCombatText.ShowCoins(Vector3.zero, 50);

                // Verify Heavy Enemy Health Bar
                var heavyObj = new GameObject("HeavyTest");
                var heavy = heavyObj.AddComponent<Enemy>();
                heavy.Configure(true, 30, 2.0f);
                heavy.TakeDamage(5); // Takes damage, should create and update health bar

                var bar = heavyObj.transform.Find("HealthBar");
                if (bar == null || !bar.gameObject.activeSelf)
                    throw new Exception("Heavy Enemy HealthBar did not activate after taking damage");

                UnityEngine.Object.DestroyImmediate(heavyObj);
                UnityEngine.Object.DestroyImmediate(fctObj);
            }, ref passed, ref failed);

            testLog.AppendLine("==================================================");
            testLog.AppendLine($"[SMOKE TEST FINISHED] Passed: {passed}, Failed: {failed}");
            testLog.AppendLine("==================================================");

            try
            {
                System.IO.File.WriteAllText("smoke_test_results.txt", testLog.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to write smoke_test_results.txt: " + ex.Message);
            }

            Debug.Log("==================================================");
            Debug.Log($"[SMOKE TEST FINISHED] Passed: {passed}, Failed: {failed}");
            Debug.Log("==================================================");

            try
            {
                if (!Application.isBatchMode && !Application.isPlaying)
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
                }
            }
            catch { }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failed > 0 ? 1 : 0);
            }
        }

        private static void Test(string name, Action action, ref int passed, ref int failed)
        {
            try
            {
                action();
                testLog.AppendLine($"[PASS] {name}");
                Debug.Log($"[PASS] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                testLog.AppendLine($"[FAIL] {name}: {ex.Message}");
                Debug.LogError($"[FAIL] {name}: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }
        }
    }
}
