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
            EditorApplication.delayCall += EnsureAsteroid;
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
    }
}
