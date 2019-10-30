using UnityEngine;
using UnityEditor;

public class BundleExporter
{
	[MenuItem("AssetBundle/Create (Selection only)")]
	static void ExportWithDependencies()
	{
		Export(false, EditorUserBuildSettings.activeBuildTarget);
	}
	
	[MenuItem("AssetBundle/Create (Include Dependencies) #&b")]
	static void ExportWithoutDependencies()
	{
		Export(true, EditorUserBuildSettings.activeBuildTarget);
	}
	
	/// <summary>
	/// Export the selected prefab into an AssetBundle.
	/// </summary>
	
	static void Export (bool includeDependencies, BuildTarget target)
	{
		if (Selection.objects == null || Selection.objects.Length == 0)
		{
			Debug.LogWarning("You must select an object you wish to export");
		}
		else
		{
			string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
		
			if (!string.IsNullOrEmpty(path))
			{
				BuildAssetBundleOptions opt = BuildAssetBundleOptions.CompleteAssets;
				if (includeDependencies) opt |= BuildAssetBundleOptions.CollectDependencies;
				BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path, opt, target);
			}
		}
	}
}