using KANYE.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KANYE.Gameplay
{
    public static class GameplayBootstrap
    {
        private static readonly Vector3 AsteroidPosition = new Vector3(4f, 0f, 0f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Application.isPlaying || scene.name != "Gameplay")
                return;

            if (Object.FindAnyObjectByType<HypeState>() == null)
                new GameObject("HypeState").AddComponent<HypeState>();

            GameRules.NotifyGameplayStarted();

            if (Object.FindAnyObjectByType<SpaceBackdrop>() == null)
                new GameObject("SpaceBackdrop").AddComponent<SpaceBackdrop>();

            var player = GameObject.Find("Player");
            if (player != null && player.GetComponent<ShipHealth>() == null)
                player.AddComponent<ShipHealth>();

            if (Object.FindAnyObjectByType<EnemySpawner>() == null)
                new GameObject("EnemySpawner").AddComponent<EnemySpawner>();

            if (Object.FindAnyObjectByType<Asteroid>() != null)
                return;

            var asteroid = new GameObject("Asteroid");
            asteroid.transform.position = AsteroidPosition;
            asteroid.AddComponent<SpriteRenderer>();
            asteroid.AddComponent<Asteroid>();
        }

        public static void EnsureEditorAsteroid()
        {
            if (Object.FindAnyObjectByType<Asteroid>() != null)
                return;

            var asteroid = new GameObject("Asteroid");
            asteroid.transform.position = AsteroidPosition;
            asteroid.AddComponent<SpriteRenderer>();
            asteroid.AddComponent<Asteroid>();
        }
    }
}
