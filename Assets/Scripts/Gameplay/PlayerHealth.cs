using System;
using KANYE.Services;
using KANYE.Systems;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private Vector2 hitSize = new Vector2(1.1f, 0.5f);
        [SerializeField] private float crashY = -4.7f;

        public int Current { get; private set; }
        public bool IsDead { get; private set; }
        public bool TookDamage { get; private set; }
        public event Action<int> Changed;

        private Rigidbody2D body;
        private bool crashed;
        private bool reviveUsed;
        private bool showingAd;
        private float adUntil;

        private void Awake()
        {
            Current = maxHealth;
            body = GetComponent<Rigidbody2D>();
            var hitBox = GetComponent<BoxCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<BoxCollider2D>();

            hitBox.isTrigger = false;
            hitBox.size = hitSize;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            TookDamage = true;
            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current);
            GetComponent<ShipHealth>()?.Mirror(Current, Current == 0);
            if (Current > 0)
                return;

            IsDead = true;
            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            var shooter = GetComponent<ShipShooter>();
            if (shooter != null)
                shooter.enabled = false;

            if (body != null)
            {
                body.gravityScale = 4.5f;
                body.linearVelocity = new Vector2(body.linearVelocity.x * 0.3f, 1.2f);
                body.angularVelocity = UnityEngine.Random.Range(-220f, 220f);
            }
        }

        private void Update()
        {
            if (!IsDead || body == null)
                return;

            if (showingAd)
            {
                if (adUntil <= 0f)
                    adUntil = Time.unscaledTime + 1.6f;
                if (Time.unscaledTime < adUntil)
                    return;

                adUntil = 0f;
                showingAd = false;
                Revive();
                return;
            }

            if (crashed || transform.position.y > crashY)
                return;

            crashed = true;
            transform.position = new Vector3(transform.position.x, crashY, transform.position.z);
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;

            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = IsDead ? Color.red : Color.white;
            var rank = SaveManager.Instance == null
                ? "Пилот"
                : PilotRank.Title(SaveManager.Instance.Data.totalKills);
            var text = crashed ? "Корабль разбит" : IsDead ? "Падение" : $"HP {Current}/{maxHealth}  {rank}";
            GUI.Label(new Rect(24f, 24f, 420f, 40f), text, style);

            if (showingAd)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 40f, 360f, 80f), "Реклама...");
                return;
            }

            if (!crashed)
                return;

            var box = new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 90f, 460f, 190f);
            GUI.Box(box, "Поражение");
            if (GUI.Button(new Rect(box.x + 30f, box.y + 120f, 180f, 40f), "В меню"))
                SceneLoader.ReturnToMainMenu();
            if (!reviveUsed && GUI.Button(new Rect(box.x + 230f, box.y + 120f, 200f, 40f), "Реклама: возродиться"))
                showingAd = true;
        }

        private void Revive()
        {
            reviveUsed = true;
            AchievementManager.Instance?.UnlockAchievement("second_chance");
            IsDead = false;
            crashed = false;
            Current = maxHealth;
            Changed?.Invoke(Current);
            GetComponent<ShipHealth>()?.Mirror(Current, false);

            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                body.gravityScale = 0f;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            transform.position = new Vector3(transform.position.x, -1.2f, transform.position.z);
            transform.rotation = Quaternion.identity;

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = true;
            var shooter = GetComponent<ShipShooter>();
            if (shooter != null)
                shooter.enabled = true;

            var sprite = GetComponent<SpriteRenderer>();
            ShipSkinCatalog.Apply(sprite);
        }
    }
}
