using System.IO;
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

        //string[] levels = { "Assets/Scenes/SampleScene.unity" };
        // var buildTarget = EvaluateBuildTarget();
        // var fileName = string.Empty;
        // if (buildTarget == BuildTarget.Android)
        // {
        //     fileName = $"{Application.productName}.apk";
        // }
        // else if (buildTarget == BuildTarget.StandaloneWindows64)
        // {
        //     fileName = $"{Application.productName}.exe";
        // }
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new [] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = "Android",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        BuildPipeline.BuildPlayer (buildPlayerOptions);

        CreateDirectory();

        // var report = BuildPipeline.BuildPlayer(levels, 
        //     fileName,
        //     buildTarget, 
        //     BuildOptions.None);

        //ReportBuildSummary(report);
    }

    static void CreateDirectory()
    {
        var dir = GetArgValue("-OUTPUTPATH");
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
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