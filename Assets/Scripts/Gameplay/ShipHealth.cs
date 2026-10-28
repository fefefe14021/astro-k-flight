using System;
using System.Collections;
using UnityEngine;

namespace KANYE.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShipHealth : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invulnerableSeconds = 0.8f;

        public int Lives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsDefeated { get; private set; }

        public event Action Changed;

        private SpriteRenderer spriteRenderer;
        private bool invulnerable;

        private void Awake()
        {
            Lives = maxLives;
            spriteRenderer = GetComponent<SpriteRenderer>();

            var hitBox = GetComponent<CircleCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<CircleCollider2D>();

            hitBox.isTrigger = true;
            hitBox.radius = 0.45f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (GetComponent<PlayerHealth>() != null)
                return;

            if (other.GetComponent<Asteroid>() == null && other.GetComponent<Enemy>() == null)
                return;

            TakeHit();
            Destroy(other.gameObject);
        }

        public void Mirror(int lives, bool defeated)
        {
            Lives = lives;
            IsDefeated = defeated;
            Changed?.Invoke();
        }

        private void TakeHit()
        {
            if (IsDefeated || invulnerable)
                return;

            Lives = Mathf.Max(0, Lives - 1);
            Changed?.Invoke();

            if (Lives == 0)
            {
                Defeat();
                return;
            }

            StartCoroutine(Blink());
        }

        private void Defeat()
        {
            IsDefeated = true;
            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            var shooter = GetComponent<ShipShooter>();
            if (shooter != null)
                shooter.enabled = false;

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
        }

        private IEnumerator Blink()
        {
            invulnerable = true;
            var until = Time.time + invulnerableSeconds;
            while (Time.time < until)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;

                yield return new WaitForSeconds(0.08f);
            }

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            invulnerable = false;
        }
    }
}
