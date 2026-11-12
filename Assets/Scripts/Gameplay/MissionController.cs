// Assets/Scripts/Gameplay/MissionController.cs
// Namespace: KANYE.Gameplay
// Manages sector pacing, hazard spawning, boss summoning, and sector completion.

using KANYE.Audio;
using KANYE.Services;
using KANYE.Systems;
using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class MissionController : MonoBehaviour
    {
        private MissionDefinition mission;
        private HypeState hype;
        private BossController currentBoss;

        private int spawnedAsteroids;
        private float spawnTimer;
        private float missileTimer;
        private float parryTimer;
        private bool bossSpawned;
        private bool missionCompleted;

        private void Awake()
        {
            var index = MissionCatalog.Selected;
            mission = MissionCatalog.All[Mathf.Clamp(index, 0, MissionCatalog.All.Length - 1)];

            hype = FindAnyObjectByType<HypeState>();
            if (hype != null)
                hype.BeginMission(mission.AlbumsRequired);

            GameRules.ResetMissionStats();
        }

        private void Start()
        {
            if (hype != null)
                hype.Changed += OnHypeChanged;
        }

        private void OnDestroy()
        {
            if (hype != null)
                hype.Changed -= OnHypeChanged;
        }

        private void Update()
        {
            if (missionCompleted || mission == null || TutorialHints.IsTutorialActive || MissionRadioComms.IsCommsActive)
                return;

            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null && player.IsDead)
                return;

            // Spawning phase before boss
            if (!bossSpawned)
            {
                spawnTimer += Time.deltaTime;
                if (spawnedAsteroids < mission.ExtraAsteroids && spawnTimer >= mission.SpawnInterval)
                {
                    spawnTimer = 0f;
                    spawnedAsteroids++;
                    SpawnAsteroid();
                }

                // Falling missiles
                if (mission.MissileInterval > 0f)
                {
                    missileTimer += Time.deltaTime;
                    if (missileTimer >= mission.MissileInterval)
                    {
                        missileTimer = 0f;
                        for (var i = 0; i < mission.MissileBurst; i++)
                            SpawnMissile();
                    }
                }

                // Cuphead parryable pink hazard orbs
                if (GameRules.IsParryUnlocked)
                {
                    parryTimer += Time.deltaTime;
                    if (parryTimer >= 4.5f)
                    {
                        parryTimer = 0f;
                        SpawnParryOrb();
                    }
                }
            }
        }

        private void OnHypeChanged()
        {
            if (missionCompleted || hype == null || mission == null)
                return;

            if (hype.Albums >= mission.AlbumsRequired)
            {
                if (mission.HasBoss)
                {
                    if (!bossSpawned)
                        SummonBoss();
                }
                else
                {
                    CompleteMission();
                }
            }
        }

        private void SummonBoss()
        {
            bossSpawned = true;

            // Clear remaining hazards so arena is clear
            var asteroids = FindObjectsByType<Asteroid>();
            for (var i = 0; i < asteroids.Length; i++)
                Destroy(asteroids[i].gameObject);

            var missiles = FindObjectsByType<Missile>();
            for (var i = 0; i < missiles.Length; i++)
                Destroy(missiles[i].gameObject);

            var parries = FindObjectsByType<ParryHazard>();
            for (var i = 0; i < parries.Length; i++)
                Destroy(parries[i].gameObject);

            // Radio transmission from Navigator Barney warning about incoming Sector Boss!
            // Time is paused during transmission; boss spawns after player dismisses warning.
            MissionRadioComms.ShowBossAlert(mission.SectorIndex, () =>
            {
                currentBoss = BossController.SpawnBoss(mission.SectorIndex);
                currentBoss.Defeated += CompleteMission;
            });
        }

        private void CompleteMission()
        {
            if (missionCompleted) return;
            missionCompleted = true;

            // Clear remaining hazards
            var asteroids = FindObjectsByType<Asteroid>();
            for (var i = 0; i < asteroids.Length; i++)
                Destroy(asteroids[i].gameObject);

            var missiles = FindObjectsByType<Missile>();
            for (var i = 0; i < missiles.Length; i++)
                Destroy(missiles[i].gameObject);

            var parries = FindObjectsByType<ParryHazard>();
            for (var i = 0; i < parries.Length; i++)
                Destroy(parries[i].gameObject);

            var enemies = FindObjectsByType<Enemy>();
            for (var i = 0; i < enemies.Length; i++)
                Destroy(enemies[i].gameObject);

            AudioManager.PlayVictory();

            var player = FindAnyObjectByType<PlayerHealth>();
            var untouched = player == null || !player.TookDamage;
            var hp = player != null ? player.Current : 0;
            var maxHp = player != null ? player.MaxLives : 3;

            var clearTime = Time.time - GameRules.MissionStartTime;
            GameRules.NotifyMissionCleared(MissionCatalog.Selected, untouched);

            var nextMission = MissionCatalog.Selected + 1;
            SaveManager.Instance?.RememberMission(nextMission);

            VictoryResultsModal.Show(
                MissionCatalog.Selected,
                hp,
                maxHp,
                GameRules.ParriesThisRun,
                GameRules.SupersUsedThisRun,
                clearTime,
                () =>
                {
                    MissionCatalog.Select(nextMission);
                    SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
                });
        }

        private void SpawnAsteroid()
        {
            var large = Random.value < mission.LargeChance;
            var asteroidObject = new GameObject("Asteroid", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            var y = large ? Random.Range(-2.4f, 2.4f) : Random.Range(-3.6f, 3.6f);
            asteroidObject.transform.position = new Vector3(10.5f + Random.Range(0f, 1.5f), y, 0f);
            var asteroid = asteroidObject.AddComponent<Asteroid>();
            var speed = large ? mission.AsteroidSpeed * 0.75f : mission.AsteroidSpeed * Random.Range(0.95f, 1.25f);
            asteroid.Configure(speed, large ? mission.Damage + 1 : mission.Damage, large ? 0.95f : 0.52f);
        }

        private void SpawnMissile()
        {
            var missileObject = new GameObject("Missile", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
            missileObject.transform.position = new Vector3(Random.Range(-6.5f, 7.5f), 6.5f, 0f);
            var missile = missileObject.AddComponent<Missile>();
            missile.Configure(mission.MissileSpeed, 1);
        }

        private void SpawnParryOrb()
        {
            var orbObject = new GameObject("ParryOrb", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            orbObject.transform.position = new Vector3(10.5f, Random.Range(-3.2f, 3.2f), 0f);
            var hazard = orbObject.AddComponent<ParryHazard>();
            hazard.Launch(Vector2.left, 3.2f);
        }
    }
}
