using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float spawnX = 10.5f;
        [SerializeField] private float minY = -3.2f;
        [SerializeField] private float maxY = 3.2f;

        private float waveTimer = 1.0f;

        private enum FormationType
        {
            SingleScout,
            ArrowWedge,
            VerticalPincer,
            HeavyEscort,
            FastInterceptor
        }

        private void Update()
        {
            if (TutorialHints.IsTutorialActive || MissionRadioComms.IsCommsActive || MissionController.IsMissionFinished || Time.timeScale <= 0f)
                return;

            var health = FindAnyObjectByType<ShipHealth>();
            if (health != null && health.IsDefeated)
                return;

            if (BossController.ActiveBoss != null && !BossController.ActiveBoss.IsDead)
                return;

            var missionIdx = MissionCatalog.Selected;
            var maxAlive = missionIdx >= 7 ? 7 : (missionIdx >= 3 ? 6 : 5);
            var waveCooldown = missionIdx >= 7 ? 2.3f : (missionIdx >= 3 ? 2.8f : 3.4f);

            waveTimer -= Time.deltaTime;
            if (waveTimer > 0f)
                return;

            var currentEnemies = FindObjectsByType<Enemy>().Length;
            if (currentEnemies >= maxAlive)
            {
                waveTimer = 0.9f;
                return;
            }

            waveTimer = waveCooldown;
            SpawnWave(missionIdx, currentEnemies, maxAlive);
        }

        private void SpawnWave(int missionIdx, int currentEnemies, int maxAlive)
        {
            var formation = PickFormation(missionIdx);

            switch (formation)
            {
                case FormationType.ArrowWedge:
                    if (currentEnemies + 3 <= maxAlive)
                    {
                        var centerY = Random.Range(minY + 0.9f, maxY - 0.9f);
                        SpawnEnemy(spawnX, centerY, isHeavy: false, isFast: false);
                        SpawnEnemy(spawnX + 0.9f, centerY + 1.1f, isHeavy: false, isFast: false);
                        SpawnEnemy(spawnX + 0.9f, centerY - 1.1f, isHeavy: false, isFast: false);
                    }
                    else
                    {
                        SpawnEnemy(spawnX, Random.Range(minY, maxY), isHeavy: false, isFast: false);
                    }
                    break;

                case FormationType.VerticalPincer:
                    SpawnEnemy(spawnX, maxY - 0.4f, isHeavy: false, isFast: false);
                    SpawnEnemy(spawnX + 0.35f, minY + 0.4f, isHeavy: false, isFast: false);
                    break;

                case FormationType.HeavyEscort:
                    var heavyY = Random.Range(minY + 0.8f, maxY - 0.8f);
                    SpawnEnemy(spawnX, heavyY, isHeavy: true, isFast: false);
                    if (currentEnemies + 3 <= maxAlive)
                    {
                        SpawnEnemy(spawnX + 1.1f, heavyY + 1.2f, isHeavy: false, isFast: false);
                        SpawnEnemy(spawnX + 1.1f, heavyY - 1.2f, isHeavy: false, isFast: false);
                    }
                    break;

                case FormationType.FastInterceptor:
                    var interceptorY = Random.Range(minY, maxY);
                    SpawnEnemy(spawnX, interceptorY, isHeavy: false, isFast: true);
                    break;

                default: // SingleScout
                    SpawnEnemy(spawnX, Random.Range(minY, maxY), isHeavy: false, isFast: false);
                    break;
            }
        }

        private FormationType PickFormation(int missionIdx)
        {
            var roll = Random.value;
            if (missionIdx >= 6)
            {
                if (roll < 0.32f) return FormationType.HeavyEscort;
                if (roll < 0.62f) return FormationType.ArrowWedge;
                if (roll < 0.82f) return FormationType.VerticalPincer;
                return FormationType.FastInterceptor;
            }
            if (missionIdx >= 3)
            {
                if (roll < 0.22f) return FormationType.HeavyEscort;
                if (roll < 0.58f) return FormationType.ArrowWedge;
                if (roll < 0.80f) return FormationType.VerticalPincer;
                return FormationType.SingleScout;
            }
            // Early missions (0 - 2)
            if (roll < 0.45f) return FormationType.ArrowWedge;
            if (roll < 0.75f) return FormationType.VerticalPincer;
            return FormationType.SingleScout;
        }

        private void SpawnEnemy(float x, float y, bool isHeavy, bool isFast)
        {
            var enemyObj = new GameObject(isHeavy ? "EnemyHeavy" : "EnemyScout", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            enemyObj.transform.position = new Vector3(x, Mathf.Clamp(y, minY, maxY), 0f);
            var enemy = enemyObj.AddComponent<Enemy>();

            var missionIdx = MissionCatalog.Selected;

            // Rebalanced HP: Scouts have 4-6 HP (instead of 1!), Heavy has 22-30 HP (instead of 3!)
            int hp;
            float speed;

            if (isHeavy)
            {
                hp = missionIdx >= 7 ? 30 : (missionIdx >= 3 ? 25 : 22);
                speed = 2.0f;
            }
            else if (isFast)
            {
                hp = missionIdx >= 7 ? 5 : 4;
                speed = 4.2f;
            }
            else
            {
                hp = missionIdx >= 7 ? 6 : (missionIdx >= 4 ? 5 : 4);
                speed = Mathf.Min(3.6f, 2.6f + missionIdx * 0.08f);
            }

            enemy.Configure(isHeavy, hp, speed);
        }
    }
}
