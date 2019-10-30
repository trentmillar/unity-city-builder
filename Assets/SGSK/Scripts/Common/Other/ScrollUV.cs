using UnityEngine;

/// <summary>
/// Simple script that scrolls the main texture's UVs over time.
/// </summary>

[RequireComponent(typeof(Renderer))]
[AddComponentMenu("Common/Scroll UV")]
public class ScrollUV : MonoBehaviour
{
	public Vector2 rpm = Vector2.zero;

	Material mMat = null;
	
	void Start()
	{
		mMat = renderer.material;
	}
	
	void Update()
	{
		mMat.mainTextureOffset = mMat.mainTextureOffset + rpm * Time.deltaTime;
	}
}