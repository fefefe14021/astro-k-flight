using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float interval = 2.6f;
        [SerializeField] private int maxAlive = 3;
        [SerializeField] private float spawnX = 10f;
        [SerializeField] private float minY = -3.4f;
        [SerializeField] private float maxY = 3.4f;

        private float timer = 1.2f;

        private void Update()
        {
            if (TutorialHints.IsTutorialActive)
                return;

            var health = FindAnyObjectByType<ShipHealth>();
            if (health != null && health.IsDefeated)
                return;

            if (BossController.ActiveBoss != null && !BossController.ActiveBoss.IsDead)
                return;

            timer -= Time.deltaTime;
            if (timer > 0f)
                return;

            timer = interval;
            if (FindObjectsByType<Enemy>().Length >= maxAlive)
                return;

            var enemy = new GameObject("Enemy", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            enemy.transform.position = new Vector3(spawnX, Random.Range(minY, maxY), 0f);
            enemy.AddComponent<Enemy>();
        }
    }
}
