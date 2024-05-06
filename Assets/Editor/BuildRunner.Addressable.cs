using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public partial class BuildRunner
{
    public static bool BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent(out var result);
        return true;
    }
}
