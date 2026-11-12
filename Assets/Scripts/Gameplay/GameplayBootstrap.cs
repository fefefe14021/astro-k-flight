// Assets/Scripts/Gameplay/GameplayBootstrap.cs
// Namespace: KANYE.Gameplay

using KANYE.Systems;
using KANYE.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KANYE.Gameplay
{
    public static class GameplayBootstrap
    {
        private static readonly Vector3 AsteroidPosition = new Vector3(5f, 0f, 0f);

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

            // Always restore timeScale
            Time.timeScale = 1f;

            // 1. Ensure HypeState
            if (Object.FindAnyObjectByType<HypeState>() == null)
                new GameObject("HypeState").AddComponent<HypeState>();

            // 2. Ensure MissionController
            if (Object.FindAnyObjectByType<MissionController>() == null)
                new GameObject("Mission").AddComponent<MissionController>();

            // 3. Ensure SpaceBackdrop
            if (Object.FindAnyObjectByType<SpaceBackdrop>() == null)
                new GameObject("SpaceBackdrop").AddComponent<SpaceBackdrop>();

            // 4. Ensure CameraShake
            if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
                Camera.main.gameObject.AddComponent<CameraShake>();

            // 5. Ensure Player components
            var player = GameObject.Find("Player");
            if (player != null)
            {
                if (player.GetComponent<PlayerHealth>() == null)
                    player.AddComponent<PlayerHealth>();

                if (player.GetComponent<ShipThrusterVisual>() == null)
                    player.AddComponent<ShipThrusterVisual>();
            }

            // 6. Ensure EnemySpawner
            if (Object.FindAnyObjectByType<EnemySpawner>() == null)
                new GameObject("EnemySpawner").AddComponent<EnemySpawner>();

            // 7. Tutorial hints if active
            var tutorialHints = Object.FindAnyObjectByType<TutorialHints>();
            if (tutorialHints == null && TutorialHints.IsTutorialActive)
            {
                var hintsObject = new GameObject("TutorialHints");
                tutorialHints = hintsObject.AddComponent<TutorialHints>();
            }

            var shipInput = player?.GetComponent<ShipInput>();
            if (shipInput != null && tutorialHints != null)
            {
                shipInput.OnFirstShot += tutorialHints.OnFirstShot;
                shipInput.OnFirstMove += tutorialHints.OnFirstMove;
            }

            // 8. Clean up stale pre-existing scene asteroids when NOT in tutorial
            if (!TutorialHints.IsTutorialActive)
            {
                var staleAsteroids = Object.FindObjectsByType<Asteroid>();
                for (var i = 0; i < staleAsteroids.Length; i++)
                    Object.Destroy(staleAsteroids[i].gameObject);
            }
        }
    }
}
