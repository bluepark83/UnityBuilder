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

        var buildVersion = GetArgValue("APPVERSION");
        PlayerSettings.bundleVersion = buildVersion;
        
        var outPath = GetArgValue("-OUTPUTPATH");
        
        
        BuildTarget buildTarget = EvaluateBuildTarget();
        var fileName = buildTarget == BuildTarget.Android ? "Rohan2.apk" : "Rohan2.exe";
        
        var fullPath = $"{outPath}/{fileName}";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new [] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = fullPath,
            target = buildTarget,
            options = BuildOptions.None
        };
        BuildPipeline.BuildPlayer (buildPlayerOptions);

        // var report = BuildPipeline.BuildPlayer(levels, 
        //     fileName,
        //     buildTarget, 
        //     BuildOptions.None);

        //ReportBuildSummary(report);
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