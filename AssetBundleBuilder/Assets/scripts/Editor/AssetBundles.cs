using UnityEditor;
using UnityEngine;

public class AssetBundles
{
    [MenuItem("Asset Bundles/Log Asset Bundle Names")]
    static void LogAssetBundleNames()
    {
        foreach (var s in AssetDatabase.GetAllAssetBundleNames())
        {
            Debug.Log("Asset Bundle: " + s);
        }
    }

    [MenuItem("Asset Bundles/Log Assets")]
    static void LogAssets()
    {
        foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            var paths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            foreach (var path in paths)
            {
                Debug.Log("Asset: " + path);
            }
        }
        
    }

    [MenuItem("Asset Bundles/Build Asset Bundles")]
    static void BuildAllAssetBundles()
    {
        Debug.Log("Building all asset bundles");
        BuildPipeline.BuildAssetBundles("../FireBoltUnity/AssetBundles",BuildAssetBundleOptions.UncompressedAssetBundle);
        Debug.Log("Asset bundle build complete");
    }

    [MenuItem("Asset Bundles/Remove Unused Names")]
    static void RemoveUnusedNames()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    

}
