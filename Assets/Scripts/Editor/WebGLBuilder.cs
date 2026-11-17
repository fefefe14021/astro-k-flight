using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KANYE.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Tools/Собрать WebGL для Яндекс Игр (.zip)")]
        public static void BuildForYandex()
        {
            // 1. Устанавливаем сжатие Disabled (как в исходном билде на 54 МБ)
            // Это гарантирует стабильную работу на Яндекс Играх без проблем с распаковкой gzip
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.template = "PROJECT:Yandex";

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildsRoot = Path.Combine(projectRoot, "Builds");

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string currentBuildFolder = Path.Combine(buildsRoot, $"Build_{timestamp}");
            string webglDir = Path.Combine(currentBuildFolder, "webgl_files");
            string zipPath = Path.Combine(currentBuildFolder, $"astro-k-flight_{timestamp}.zip");

            Directory.CreateDirectory(webglDir);

            string[] scenes = new string[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Gameplay.unity",
                "Assets/Scenes/Shop.unity",
                "Assets/Scenes/Achievements.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = webglDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"[WebGLBuilder] Запуск сборки в {currentBuildFolder}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                long uncompressedBytes = 0;
                foreach (var file in Directory.GetFiles(webglDir, "*", SearchOption.AllDirectories))
                {
                    uncompressedBytes += new FileInfo(file).Length;
                }

                long uncompressedMb = uncompressedBytes / (1024 * 1024);
                Debug.Log($"[WebGLBuilder] Сборка завершена. Несжатый размер: {uncompressedMb} МБ. Упаковка в ZIP...");

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(webglDir, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);

                long zipBytes = new FileInfo(zipPath).Length;
                long zipMb = zipBytes / (1024 * 1024);
                Debug.Log($"[WebGLBuilder] Архив успешно создан: {zipPath} ({zipMb} МБ)");

                if (!Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(zipPath);
                    EditorUtility.DisplayDialog(
                        "Сборка готова!",
                        $"Сборка успешно завершена!\n\n" +
                        $"Папка: Build_{timestamp}\n" +
                        $"Размер файлов внутри: ~{uncompressedMb} МБ (как в прошлом билде)\n" +
                        $"Размер ZIP архива: ~{zipMb} МБ\n\n" +
                        $"Файл для Яндекс Игр:\n{zipPath}",
                        "OK"
                    );
                }
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] Ошибка сборки: {summary.result}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Ошибка сборки",
                        $"Не удалось собрать WebGL. Подробности смотрите в окне Console.",
                        "OK"
                    );
                }
            }
        }
    }
}
