using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildTool
{
    private const string OutputPath = "Builds/Android/kahve-oyunu-dev.apk";
    private const string PackageId = "com.akkus.kahveoyunu";

    [MenuItem("Oyun Araclari/Build Android Development APK")]
    public static void BuildDevelopmentApk()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
        }

        string outputDirectory = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log("[AndroidBuildTool] Result=" + summary.result +
                  " Errors=" + summary.totalErrors +
                  " Warnings=" + summary.totalWarnings +
                  " Size=" + summary.totalSize +
                  " Output=" + Path.GetFullPath(OutputPath));
        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Android build failed with " + summary.totalErrors + " error(s).");
        }
    }
}
