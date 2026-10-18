using System;
using UnityEngine;

namespace KANYE.Gameplay
{
    public class HypeState : MonoBehaviour
    {
        [SerializeField] private float hypeMax = 100f;
        [SerializeField] private float hypePerAlbum = 25f;

        public int Albums { get; private set; }
        public float Hype { get; private set; }
        public float HypeMax => hypeMax;

        public event Action Changed;

        public void AddAlbum(int count)
        {
            if (count <= 0)
                return;

            Albums += count;
            Hype = Mathf.Min(hypeMax, Hype + count * hypePerAlbum);
            Changed?.Invoke();
        }
    }
}
