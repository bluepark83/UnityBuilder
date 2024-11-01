using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Content;
using UnityEngine;

public partial class BuildRunner
{
    public static bool BuildAddressables()
    {
        Debug.Log($"BuildAddressables");
            
        AddressableAssetSettings.BuildPlayerContent(out var result);
        return true;
    }
}
