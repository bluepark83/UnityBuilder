using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public partial class BuildRunner
{
    public static void BuildPlayer(string[] arguments)
    {
        InternalBuildPlayer(arguments);
    }

    public static void BuildPlayer()
    {
        var commandLineArgs = System.Environment.GetCommandLineArgs();
        InternalBuildPlayer(commandLineArgs);
    }
    
    private static void InternalBuildPlayer(string[] arguments)
    {
        foreach (var arg in arguments)
        {
            Debug.Log($"arg : {arg}");
        }

        AssignLogos(arguments);
        
        var buildVersion = GetArgValue(arguments, "-APPVERSION");
        PlayerSettings.bundleVersion = buildVersion;
        Debug.Log($"BuildVersion : {buildVersion}");
        
        CreateDirectory(arguments);
        
        var outPath = GetArgValue(arguments, "-OUTPUTPATH");
        Debug.Log($"output Path : {outPath}");
        
        BuildTarget buildTarget = EvaluateBuildTarget(arguments);
        var fileName = buildTarget == BuildTarget.Android ? "Rohan2.apk" : "Rohan2.exe";
        
        var fullPath = $"{outPath}/{fileName}";
        Debug.Log($"fullPath : {fullPath}");
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
    
    public static void AssignLogos(string[] arguments)
    {
        var splashScreenValue = GetArgValue(arguments, "-SPLASH");
        if (string.IsNullOrEmpty(splashScreenValue))
            return;

        if (int.TryParse(splashScreenValue, out var index) == false)
            return;
        
        var logos = new PlayerSettings.SplashScreenLogo[2];
        
        var companyLogo1 = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/SplashScreen/sprite_splash_00.png", typeof(Sprite));
        logos[0] = PlayerSettings.SplashScreenLogo.Create(2f, companyLogo1);

        if (index > 1)
        {
            var companyLogo2 =
                (Sprite)AssetDatabase.LoadAssetAtPath("Assets/SplashScreen/sprite_splash_01.png", typeof(Sprite));
            logos[1] = PlayerSettings.SplashScreenLogo.Create(2f, companyLogo2);
        }

        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
        PlayerSettings.SplashScreen.logos = logos;
    }

    static void CreateDirectory(string[] arguments)
    {
        var dir = GetArgValue(arguments, "-OUTPUTPATH");
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
    
    public static void BuildAddressablesAndPlayer()
    {
        BuildAddressables();
        BuildPlayer();
    }

    static BuildTarget EvaluateBuildTarget(string[] arguments)
    {
        var value = GetArgValue(arguments, "-buildTarget");

        switch (value)
        {
            case "Win64":
                EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                return BuildTarget.StandaloneWindows64;
            // case "Android":
            default:
                EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                return BuildTarget.Android;
        }
    }
}