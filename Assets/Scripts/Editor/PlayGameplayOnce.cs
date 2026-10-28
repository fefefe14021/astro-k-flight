using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KANYE.Editor
{
    [InitializeOnLoad]
    static class PlayGameplayOnce
    {
        const string PrefKey = "KANYE.PlayGameplayOnce6";

        static PlayGameplayOnce()
        {
            if (EditorPrefs.GetBool(PrefKey, false))
                return;

            EditorApplication.delayCall += TryStart;
        }

        static void TryStart()
        {
            if (EditorPrefs.GetBool(PrefKey, false))
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryStart;
                return;
            }

            var ship = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/ship.png");
            if (ship == null)
            {
                EditorApplication.delayCall += TryStart;
                return;
            }

            EditorPrefs.SetBool(PrefKey, true);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Gameplay.unity");
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }
    }
}
