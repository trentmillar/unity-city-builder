using UnityEngine;

/// <summary>
/// Simple script that scrolls the detail texture's UV over time.
/// </summary>

[RequireComponent(typeof(Renderer))]
[AddComponentMenu("Common/Scroll Detail UV")]
public class ScrollDetailUV : MonoBehaviour
{
	public Vector2 rpm = Vector2.zero;

	Material mMat = null;
	
	void Start()
	{
		mMat = renderer.material;
	}
	
	void Update()
	{
		Vector2 offset = mMat.GetTextureOffset("_Detail");
		mMat.SetTextureOffset("_Detail", offset + rpm * Time.deltaTime);
	}
}