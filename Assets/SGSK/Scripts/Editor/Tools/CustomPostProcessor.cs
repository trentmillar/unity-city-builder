using UnityEngine;
using UnityEditor;

public class CustomPostProcessor : AssetPostprocessor
{
	void OnPreprocessTexture()
	{
		if (assetPath.Contains("UI/") || assetPath.Contains("UI\\"))
		{
			TextureImporter imp = assetImporter as TextureImporter;
			imp.textureType = TextureImporterType.GUI;
			imp.textureFormat = TextureImporterFormat.ARGB32;
		}
	}
}