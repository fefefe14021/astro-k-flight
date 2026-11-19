using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class YandexWebGLBuild
{
    private static readonly string TriggerPath = Path.Combine("Temp", "trigger_webgl_build.flag");
    private static readonly string ReloadFlagPath = Path.Combine("Temp", "webgl_pending_reload.flag");
    private static readonly string StatusPath = Path.Combine("Temp", "webgl_build_status.txt");
    private static bool isBuilding = false;

    static YandexWebGLBuild()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (isBuilding || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        // Step 1: Trigger detected -> Request AssetDatabase refresh to guarantee domain reload and synchronized serialization layout
        if (File.Exists(TriggerPath))
        {
            try { File.Delete(TriggerPath); } catch {}
            File.WriteAllText(ReloadFlagPath, "pending");
            File.WriteAllText(StatusPath, "REFRESHING|Synchronizing script assemblies and domain reload...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return;
        }

        // Step 2: Post-refresh / domain reload -> Execute build with fully fresh assemblies
        if (File.Exists(ReloadFlagPath))
        {
            try { File.Delete(ReloadFlagPath); } catch {}
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            ExecuteBuild(false);
        }
    }

    [MenuItem("Build/Build WebGL for Yandex Games")]
    public static void BuildFromMenu()
    {
        ExecuteBuild(false);
    }

    public static void Build()
    {
        // Batchmode entry point
        ExecuteBuild(true);
    }

    public static bool ExecuteBuild(bool exitOnComplete = false)
    {
        if (isBuilding)
        {
            Debug.LogWarning("[YandexWebGLBuild] Build is already in progress.");
            return false;
        }

        isBuilding = true;
        var startTime = DateTime.Now;

        try
        {
            File.WriteAllText(StatusPath, "BUILDING|Started at " + startTime.ToString("HH:mm:ss"));
            Debug.Log("[YandexWebGLBuild] Starting WebGL Build for Yandex Games...");

            var output = Path.GetFullPath(Path.Combine("Builds", "WebGL"));
            if (Directory.Exists(output))
            {
                try { Directory.Delete(output, true); }
                catch (Exception e) { Debug.LogWarning("[YandexWebGLBuild] Clean warning: " + e.Message); }
            }
            Directory.CreateDirectory(output);

            // Ensure proper WebGL player settings for Yandex Games
            PlayerSettings.WebGL.template = "PROJECT:Yandex";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Debug.Log($"[YandexWebGLBuild] Building {scenes.Length} scenes to {output}...");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                var errorMsg = $"WebGL build failed: {summary.result} ({summary.totalErrors} errors)";
                Debug.LogError("[YandexWebGLBuild] " + errorMsg);
                File.WriteAllText(StatusPath, "FAILED|" + errorMsg);
                if (exitOnComplete) EditorApplication.Exit(1);
                return false;
            }

            var duration = DateTime.Now - startTime;
            var successMsg = $"SUCCESS|Duration: {duration.TotalSeconds:F1}s|Size: {summary.totalSize} bytes|Output: {output}";
            Debug.Log("[YandexWebGLBuild] " + successMsg);
            File.WriteAllText(StatusPath, successMsg);

            if (exitOnComplete) EditorApplication.Exit(0);
            return true;
        }
        catch (Exception ex)
        {
            var err = "ERROR|" + ex.Message;
            Debug.LogError("[YandexWebGLBuild] Build exception: " + ex);
            File.WriteAllText(StatusPath, err);
            if (exitOnComplete) EditorApplication.Exit(1);
            return false;
        }
        finally
        {
            isBuilding = false;
        }
    }
}
