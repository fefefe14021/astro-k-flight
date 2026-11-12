using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float spawnX = 10.5f;
        [SerializeField] private float minY = -3.4f;
        [SerializeField] private float maxY = 3.4f;

        private float timer = 0.8f;

        private void Update()
        {
            if (TutorialHints.IsTutorialActive || MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

            var health = FindAnyObjectByType<ShipHealth>();
            if (health != null && health.IsDefeated)
                return;

            if (BossController.ActiveBoss != null && !BossController.ActiveBoss.IsDead)
                return;

            var missionIdx = MissionCatalog.Selected;
            var maxAlive = missionIdx >= 7 ? 6 : (missionIdx >= 3 ? 5 : 4);
            var interval = missionIdx >= 7 ? 1.0f : (missionIdx >= 3 ? 1.35f : 1.6f);

            timer -= Time.deltaTime;
            if (timer > 0f)
                return;

            timer = interval;
            var currentEnemies = FindObjectsByType<Enemy>().Length;
            if (currentEnemies >= maxAlive)
                return;

            SpawnSingleEnemy(Random.Range(minY, maxY));

            // Wingman formation: 35% chance to spawn a second drone in the wave
            if (currentEnemies + 1 < maxAlive && Random.value < 0.35f)
            {
                var wingY = Mathf.Clamp(Random.Range(minY, maxY), minY, maxY);
                SpawnSingleEnemy(wingY, offsetX: 0.9f);
            }
        }

        private void SpawnSingleEnemy(float y, float offsetX = 0f)
        {
            var enemyObj = new GameObject("Enemy", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            enemyObj.transform.position = new Vector3(spawnX + offsetX, y, 0f);
            var enemy = enemyObj.AddComponent<Enemy>();

            var missionIdx = MissionCatalog.Selected;
            var heavyChance = missionIdx >= 7 ? 0.40f : (missionIdx >= 3 ? 0.25f : 0.05f);
            var isHeavy = Random.value < heavyChance;
            var hp = isHeavy ? (missionIdx >= 7 ? 4 : 3) : (missionIdx >= 6 ? 2 : 1);
            var speed = isHeavy ? 2.1f : Mathf.Min(3.6f, 2.4f + missionIdx * 0.08f);

            enemy.Configure(isHeavy, hp, speed);
        }
    }
}
