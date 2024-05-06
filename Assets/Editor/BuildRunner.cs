using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

public partial class BuildRunner
{
    public static void BuildPlayer()
    {
        string[] levels = { "Assets/Scenes/SampleScene.unity" };
        var locationPathName = "Builds\\sample.apk";
            
        var report = BuildPipeline.BuildPlayer( levels, locationPathName, 
            BuildTarget.Android, BuildOptions.None); 
        
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    public static void BuildAddressablesAndPlayer()
    {
        BuildAddressables();
        BuildPlayer();
    }
}
