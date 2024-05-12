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

        string[] levels = { "Assets/Scenes/SampleScene.unity" };
        var locationPathName = string.Empty;

        var buildTarget = EvaluateBuildTarget();
        var fileName = string.Empty;
        if (buildTarget == BuildTarget.Android)
        {
            fileName = $"{Application.productName}.apk";
        }
        else if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            fileName = $"{Application.productName}.exe";
        }

        UpdateLocationPathName(ref locationPathName, fileName);

        var report = BuildPipeline.BuildPlayer(levels, 
            locationPathName,
            buildTarget, 
            BuildOptions.None);

        ReportBuildSummary(report);
    }

    static void UpdateLocationPathName(ref string refLocationName, string filename)
    {
        var value = GetArgValue("-buildWindows64Player");
        
        var path = Path.Join(value, filename);

        Debug.Log($"UpdateLocationPathName : {path}");
        if (Directory.Exists(path) == false)
        {
            if (File.Exists(path) == false)
            {
                Directory.CreateDirectory(path);

                Debug.Log($"CreateDirectory : {path}");
            }
        }

        refLocationName = path;
        
        Debug.Log($"refLocationName : {refLocationName}");
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