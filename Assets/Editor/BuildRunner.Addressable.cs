using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

public partial class BuildRunner
{
    public static bool BuildAddressables()
    {
        
        AddressableAssetSettings.CleanPlayerContent();
        BuildCache.PurgeCache(false);
        
        AddressableAssetSettings.BuildPlayerContent(out var result);
        return true;
    }
}
