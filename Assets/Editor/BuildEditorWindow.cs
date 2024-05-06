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
            BuildRunner.BuildPlayer();
        }
            
        if (GUILayout.Button("Build Addressables & Player"))
        {
            BuildRunner.BuildAddressablesAndPlayer();
        }
    }
}
