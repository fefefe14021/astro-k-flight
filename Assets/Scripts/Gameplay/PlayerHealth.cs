// Assets/Scripts/Gameplay/PlayerHealth.cs
// Namespace: KANYE.Gameplay
// Canonical Player Health & State controller.

using System;
using System.Collections;
using KANYE.Audio;
using KANYE.Services;
using KANYE.Systems;
using KANYE.UI;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class PlayerHealth : ShipHealth
    {
        [SerializeField] private Vector2 hitBoxSize = new Vector2(1.1f, 0.5f);
        [SerializeField] private float crashY = -4.6f;

        public int Current => Lives;
        public bool IsDead => IsDefeated;
        public bool TookDamage { get; private set; }

        public new event Action<int> Changed;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private bool crashed;
        private bool reviveUsed;
        private bool defeatShown;

        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            var hitBox = GetComponent<BoxCollider2D>();
            if (hitBox == null)
                hitBox = gameObject.AddComponent<BoxCollider2D>();

            hitBox.isTrigger = false;
            hitBox.size = hitBoxSize;

            var save = Systems.SaveManager.Instance;
            if (save != null && save.Data != null && save.Data.hullMetaLevel > 1)
            {
                maxLives = 3 + (save.Data.hullMetaLevel - 1);
                Lives = maxLives;
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0 || IsInvulnerable)
                return;

            invulnerableUntil = Time.time + invulnerableSeconds;
            StartCoroutine(DamageBlinkRoutine());

            TookDamage = true;
            Lives = Mathf.Max(0, Lives - amount);

            NotifyChanged();
            Changed?.Invoke(Lives);

            // Audio & visual juice
            AudioManager.PlayHit();
            EffectsManager.PlayImpact(transform.position, Vector2.left, Color.red);
            EffectsManager.PlayExplosion(transform.position, new Color(1f, 0.35f, 0.2f), isMechanical: true);
            CameraShake.Shake(0.35f, 0.3f);

            if (Lives > 0)
                return;

            // Player defeated
            IsDefeated = true;
            AudioManager.PlayDefeat();
            CameraShake.HitStop(0.1f);
            CameraShake.Shake(0.65f, 0.6f);

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            var shooter = GetComponent<ShipShooter>();
            if (shooter != null)
                shooter.enabled = false;

            if (body != null)
            {
                body.gravityScale = 4.5f;
                body.linearVelocity = new Vector2(body.linearVelocity.x * 0.25f, 1.4f);
                body.angularVelocity = UnityEngine.Random.Range(-220f, 220f);
            }
        }

        public void GrantParryInvulnerability(float duration)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
            StartCoroutine(ParryShieldBlinkRoutine(duration));
        }

        public bool Heal(int amount = 1)
        {
            if (IsDead || Lives >= MaxLives)
                return false;

            Lives = Mathf.Min(MaxLives, Lives + amount);
            NotifyChanged();
            Changed?.Invoke(Lives);
            FloatingCombatText.ShowHeal(transform.position, amount);
            AudioManager.PlayPowerup();
            EffectsManager.PlayImpact(transform.position, Vector2.up, Color.green);
            return true;
        }

        public void IncreaseMaxLives(int bonus = 1)
        {
            maxLives += bonus;
            Lives = Mathf.Min(maxLives, Lives + bonus);
            NotifyChanged();
            Changed?.Invoke(Lives);
            AudioManager.PlayPowerup();
        }

        private IEnumerator ParryShieldBlinkRoutine(float duration)
        {
            var end = Time.time + duration;
            while (Time.time < end && !IsDead)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = (Mathf.Sin(Time.time * 30f) > 0f) 
                        ? new Color(0.3f, 1f, 0.9f, 1f) 
                        : Color.white;
                }
                yield return null;
            }

            if (spriteRenderer != null && !IsDead)
            {
                ShipSkinCatalog.Apply(spriteRenderer);
            }
        }

        private IEnumerator DamageBlinkRoutine()
        {
            var end = Time.time + invulnerableSeconds;
            while (Time.time < end && !IsDead)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.08f);
            }

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
        }

        private void Update()
        {
            if (!IsDead || body == null)
                return;

            if (crashed || transform.position.y > crashY)
                return;

            crashed = true;
            transform.position = new Vector3(transform.position.x, crashY, transform.position.z);
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;

            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            if (defeatShown)
                return;

            defeatShown = true;
            GamePause.Lock();
            DefeatModal.Show(!reviveUsed, OnReviveRequested);
        }

        private void OnReviveRequested()
        {
            var sdk = YandexGamesService.Instance;
            if (sdk == null)
            {
                Revive();
                return;
            }

            sdk.ShowRewarded(Revive);
        }

        public void Revive()
        {
            reviveUsed = true;
            AchievementManager.Instance?.UnlockAchievement("second_chance");

            IsDefeated = false;
            crashed = false;
            defeatShown = false;
            Lives = maxLives;

            NotifyChanged();
            Changed?.Invoke(Lives);

            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                body.gravityScale = 0f;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            transform.position = new Vector3(-4f, 0f, 0f);
            transform.rotation = Quaternion.identity;

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = true;

            var shooter = GetComponent<ShipShooter>();
            if (shooter != null)
                shooter.enabled = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                ShipSkinCatalog.Apply(spriteRenderer);
            }

            GrantParryInvulnerability(1.5f);
            DefeatModal.Hide();
            GamePause.Release();
        }
    }
}
