using KANYE.Services;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class MissionController : MonoBehaviour
    {
        private HypeState hype;
        private MissionDefinition mission;
        private float spawnTimer;
        private float missileTimer;
        private int spawned;
        private bool won;

        private void Awake()
        {
            if (SaveManager.Instance != null)
                MissionCatalog.Select(SaveManager.Instance.CurrentMission);

            mission = MissionCatalog.Current;
            hype = FindAnyObjectByType<HypeState>();
            if (hype != null)
                hype.BeginMission(mission.AlbumsRequired);

            var existing = FindObjectsByType<Asteroid>(FindObjectsSortMode.None);
            for (var i = 0; i < existing.Length; i++)
                existing[i].Configure(mission.AsteroidSpeed, mission.Damage, 0.55f);
        }

        private void OnDestroy()
        {
            if (hype != null)
                hype.Changed -= OnHypeChanged;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (hype != null)
                hype.Changed += OnHypeChanged;
        }

        private void Update()
        {
            if (won || mission == null)
                return;

            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null && player.IsDead)
                return;

            spawnTimer += Time.deltaTime;
            if (spawned < mission.ExtraAsteroids && spawnTimer >= mission.SpawnInterval)
            {
                spawnTimer = 0f;
                spawned++;
                SpawnAsteroid();
            }

            if (mission.MissileInterval <= 0f)
                return;

            missileTimer += Time.deltaTime;
            if (missileTimer < mission.MissileInterval)
                return;

            missileTimer = 0f;
            for (var i = 0; i < mission.MissileBurst; i++)
                SpawnMissile();
        }

        private void OnHypeChanged()
        {
            if (won || hype == null || hype.Hype < hype.HypeMax - 0.01f)
                return;

            won = true;
            Time.timeScale = 0f;
            var player = FindAnyObjectByType<PlayerHealth>();
            var untouched = player == null || !player.TookDamage;
            GameRules.NotifyMissionCleared(MissionCatalog.Selected, untouched);
            var next = MissionCatalog.Selected + 1;
            SaveManager.Instance?.RememberMission(next);
        }

        private void SpawnAsteroid()
        {
            var large = Random.value < mission.LargeChance;
            var asteroidObject = new GameObject("Asteroid");
            var y = large ? Random.Range(-2.4f, 2.4f) : Random.Range(-3.4f, 3.4f);
            asteroidObject.transform.position = new Vector3(10f + Random.Range(0f, 1.5f), y, 0f);
            asteroidObject.AddComponent<SpriteRenderer>();
            var asteroid = asteroidObject.AddComponent<Asteroid>();
            var speed = large ? mission.AsteroidSpeed * 0.72f : mission.AsteroidSpeed * Random.Range(0.95f, 1.2f);
            asteroid.Configure(speed, large ? mission.Damage + 1 : mission.Damage, large ? 0.95f : 0.48f);
        }

        private void SpawnMissile()
        {
            var missileObject = new GameObject("Missile");
            missileObject.transform.position = new Vector3(Random.Range(-6.5f, 7.5f), 6.4f, 0f);
            missileObject.AddComponent<SpriteRenderer>();
            var missile = missileObject.AddComponent<Missile>();
            missile.Configure(mission.MissileSpeed, 1);
        }

        private void OnGUI()
        {
            if (mission == null)
                return;

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            };
            title.normal.textColor = Color.white;
            GUI.Label(new Rect(220f, 12f, Screen.width - 440f, 32f), mission.Title, title);

            var body = new GUIStyle(title) { fontSize = 16, fontStyle = FontStyle.Normal };
            GUI.Label(new Rect(160f, 44f, Screen.width - 320f, 78f), mission.Briefing, body);

            if (!won)
                return;

            var hasNext = MissionCatalog.Selected + 1 < MissionCatalog.All.Length;
            var showRestart = MissionCatalog.Selected > 0;
            var buttonCount = 1 + (hasNext ? 1 : 0) + (showRestart ? 1 : 0);
            var box = new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f - 120f, 500f, 150f + buttonCount * 52f);
            GUI.Box(box, "Победа");
            var message = new GUIStyle(title) { fontSize = 20 };
            GUI.Label(new Rect(box.x + 24f, box.y + 40f, box.width - 48f, 70f), mission.VictoryText, message);
            var buttonY = box.y + 120f;
            if (GUI.Button(new Rect(box.x + 40f, buttonY, box.width - 80f, 40f), "В меню"))
                SceneLoader.ReturnToMainMenu();
            buttonY += 52f;
            if (hasNext && GUI.Button(new Rect(box.x + 40f, buttonY, box.width - 80f, 40f), "Дальше"))
            {
                Time.timeScale = 1f;
                MissionCatalog.Select(MissionCatalog.Selected + 1);
                SceneLoader.LoadSceneByName(SceneLoader.GameplayScene);
            }

            if (hasNext)
                buttonY += 52f;
            if (showRestart && GUI.Button(new Rect(box.x + 40f, buttonY, box.width - 80f, 40f), "Сначала"))
                SceneLoader.RestartFromFirstMission();
        }
    }
}
