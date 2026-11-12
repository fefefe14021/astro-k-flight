using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KANYE.Gameplay;

namespace KANYE.EditorTools
{
    [InitializeOnLoad]
    public static class GameplaySceneRepair
    {
        static GameplaySceneRepair()
        {
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.delayCall += EnsureAsteroid;
            EditorApplication.delayCall += EnsureShipVisual;
        }

        private static void EnsureAsteroid()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != "Gameplay")
                return;

            if (Object.FindAnyObjectByType<Asteroid>() != null)
                return;

            GameplayBootstrap.EnsureEditorAsteroid();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureShipVisual()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // Force reimport ship sprite to clear any invalid cache
            AssetDatabase.ImportAsset("Assets/Resources/Sprites/ship.png", ImportAssetOptions.ForceUpdate);

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != "Gameplay")
                return;

            var player = GameObject.Find("Player");
            if (player == null)
                return;

            var visual = player.GetComponent<PlaceholderShipVisual>();
            if (visual != null)
            {
                visual.EnsureShipSetup();
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }
}
