using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public partial class BuildEditorwindow : EditorWindow
{
    [MenuItem("Window/AddressablePatchWindow")]
    static void Init()
    {
        GetWindow(typeof(BuildEditorwindow));
    }
    
    private void OnGUI()
    {
        OnDrawContentBuild();
    }


    void OnDrawContentBuild()
    {
        GUILayout.Label("[BUILD CONTENT]");

        if (GUILayout.Button("Build Addressables only"))
        {
            BuildRunner.BuildAddressables();
        }
            
        if (GUILayout.Button("Build only"))
        {
            var arguments = new[]
            {
                "-SPLASH",
                "1",
                
                "-APPVERSION",
                "0.9.8",
                
                "-OUTPUTPATH",
                $@"C:\\RM2_Build\\{DateTime.Now:yyyyMMddHHmm}",
                
                "-buildTarget",
                "Win64"
            };
            
            BuildRunner.BuildPlayer(arguments);
        }
            
        if (GUILayout.Button("Build Addressables & Player"))
        {
            BuildRunner.BuildAddressablesAndPlayer();
        }
    }
}
