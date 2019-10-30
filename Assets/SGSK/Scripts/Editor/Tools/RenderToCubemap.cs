using UnityEngine;
using UnityEditor;

public class RenderToCubemap : ScriptableWizard
{
	public Camera cameraToUse;
	public Cubemap cubemapToRenderInto;

	[MenuItem("Tools/Render Into Cubemap")]
	public static void CreateWizard ()
	{
		ScriptableWizard.DisplayWizard<RenderToCubemap>("Render cubemap", "Render!");
	}

	void OnWizardUpdate ()
	{
		helpString = "Select transform to render from and cubemap to render into";
		isValid = (cameraToUse != null) && (cubemapToRenderInto != null);
	}

	void OnWizardCreate ()
	{
		cameraToUse.RenderToCubemap(cubemapToRenderInto);
	}
}