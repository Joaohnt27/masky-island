using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Build reproduzível usando as cenas habilitadas do projeto.</summary>
public static class BuildRelease
{
    [MenuItem("Masky Island/Build Windows Release")]
    public static void Windows()
    {
        int result = 1;
        try
        {
            string output = Environment.GetEnvironmentVariable("MASKY_BUILD_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = "Builds/Windows/MaskyIsland.exe";
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0 || !scenes[0].EndsWith("/Splash.unity"))
                throw new InvalidOperationException("A primeira cena da build deve ser Splash.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output), "build-report.txt"),
                "Masky Island | Windows x64\nUnity: " + Application.unityVersion +
                "\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors +
                "\nWarnings: " + report.summary.totalWarnings + "\nBytes: " + report.summary.totalSize +
                "\nScenes:\n" + string.Join("\n", scenes));
            result = report.summary.result == BuildResult.Succeeded ? 0 : 1;
        }
        catch (Exception error) { Debug.LogException(error); }
        if (Application.isBatchMode) EditorApplication.Exit(result);
    }
}
