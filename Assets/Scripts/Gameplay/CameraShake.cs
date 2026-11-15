// Assets/Scripts/Gameplay/CameraShake.cs
// Namespace: KANYE.Gameplay
// GameObject: Main Camera (автоматически или вручную)

using UnityEngine;

namespace KANYE.Gameplay
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 initialPosition;
        private float shakeTrauma = 0f;
        private float shakeDuration = 0f;
        private float currentTimer = 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoAttach()
        {
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<CameraShake>() == null)
            {
                cam.gameObject.AddComponent<CameraShake>();
            }
        }

        private void Awake()
        {
            Instance = this;
            initialPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            initialPosition = new Vector3(0f, 0f, -10f);
        }

        public static void Shake(float trauma, float duration = 0.25f)
        {
            if (Instance == null)
            {
                var cam = Camera.main;
                if (cam != null)
                    Instance = cam.gameObject.AddComponent<CameraShake>();
            }

            if (Instance != null)
            {
                Instance.AddTrauma(trauma, duration);
            }
        }

        public void AddTrauma(float trauma, float duration)
        {
            shakeTrauma = Mathf.Clamp01(shakeTrauma + trauma);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            currentTimer = shakeDuration;
        }

        public static void HitStop(float duration = 0.08f)
        {
            if (Instance != null)
                Instance.StartCoroutine(Instance.HitStopRoutine(duration));
        }

        private System.Collections.IEnumerator HitStopRoutine(float duration)
        {
            var prevScale = Time.timeScale > 0.01f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            if (!GamePause.Locked)
                Time.timeScale = prevScale;
        }

        private void Update()
        {
            if (currentTimer <= 0f && shakeTrauma <= 0.001f)
            {
                transform.localPosition = initialPosition;
                return;
            }

            currentTimer -= Time.unscaledDeltaTime;
            var decay = shakeDuration > 0f ? (currentTimer / shakeDuration) : 0f;
            var shakeStrength = shakeTrauma * shakeTrauma * Mathf.Clamp01(decay);

            var offsetX = (Mathf.PerlinNoise(Time.unscaledTime * 32f, 0f) * 2f - 1f) * shakeStrength * 0.45f;
            var offsetY = (Mathf.PerlinNoise(0f, Time.unscaledTime * 32f) * 2f - 1f) * shakeStrength * 0.45f;

            transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0f);

            shakeTrauma = Mathf.MoveTowards(shakeTrauma, 0f, Time.unscaledDeltaTime * 2.5f);
        }
    }
}
