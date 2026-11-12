// Assets/Scripts/Gameplay/ShipHealth.cs
// Namespace: KANYE.Gameplay
// Base health interface for player ship.

using System;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class ShipHealth : MonoBehaviour
    {
        [SerializeField] protected int maxLives = 3;
        [SerializeField] protected float invulnerableSeconds = 1.35f;

        public int Lives { get; protected set; }
        public int MaxLives => maxLives;
        public bool IsDefeated { get; protected set; }
        public bool IsInvulnerable => invulnerableUntil > Time.time;

        public event Action Changed;

        protected float invulnerableUntil;

        protected virtual void Awake()
        {
            Lives = maxLives;
        }

        protected void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
