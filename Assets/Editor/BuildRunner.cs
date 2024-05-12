using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public partial class BuildRunner
{
    public static void BuildPlayer()
    {
        var commandLineArgs = System.Environment.GetCommandLineArgs();

        foreach (var arg in commandLineArgs)
        {
            Debug.Log($"arg : {arg}");
        }

        string[] levels = { "Assets/Scenes/SampleScene.unity" };
        var locationPathName = "Builds\\sample.apk";

        
        var buildTarget = EvaluateBuildTarget();
        if (buildTarget == BuildTarget.Android)
        {
            locationPathName = $"Builds\\{Application.productName}.apk";
        }
        else if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            locationPathName = $"Builds\\{Application.productName}.exe";
        }

        var report = BuildPipeline.BuildPlayer(levels, locationPathName,
            buildTarget, BuildOptions.None);

        ReportBuildSummary(report);
    }

    public static void BuildAddressablesAndPlayer()
    {
        BuildAddressables();
        BuildPlayer();
    }

    static BuildTarget EvaluateBuildTarget()
    {
        var value = GetArgValue("-buildTarget");

        if (string.IsNullOrEmpty(value))
            return BuildTarget.Android;

        return value switch
        {
            "Win64" => BuildTarget.StandaloneWindows64,
            "Android" => BuildTarget.Android,
            _ => BuildTarget.Android
        };
    }
}