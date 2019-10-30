using UnityEngine;

/// <summary>
/// Attach to a game object that has a material using a cube map. This script will render
/// a real-time cubemap, replacing the static cube map with this one.
/// </summary>

[AddComponentMenu("Common/Realtime Cubemap")]
public class RealtimeCubemap : MonoBehaviour
{
	public enum Size
	{
		Tiny32,
		Small64,
		Medium128,
		Large256,
		Huge512,
	}

	public Size cubemapSize = Size.Medium128;
	public Vector2 cameraNearFar = new Vector2(1.0f, 50.0f);
	public LayerMask cameraMask = -1;
	public Material materialToUpdate = null;
	public string textureFieldToUpdate = "_Cube";
	public float updateFrequency = 0f;

	Camera mCam;
	Transform mCamTrans;
	RenderTexture mTex;
	Texture mPrevious;
	float mNextUpdate = 0f;

	void Start ()
	{
		if (NetworkManager.IsMine(this) && QualitySettings.currentLevel == QualityLevel.Fantastic)
		{
			if (materialToUpdate == null)
			{
				Renderer ren = GetComponent<Renderer>();
				if (ren != null) materialToUpdate = ren.material;
			}
			LateUpdate();
		}
		else
		{
			Destroy(this);
		}
	}

	void LateUpdate ()
	{
		if (materialToUpdate == null || Time.time < mNextUpdate) return;

		mNextUpdate = Time.time + updateFrequency;

		if (mCam == null)
		{
			GameObject go = new GameObject("RealtimeCubemap Camera");
			go.hideFlags = HideFlags.HideAndDontSave;
			mCamTrans = go.transform;

			mCam = go.AddComponent<Camera>();
			mCam.enabled = false;
		}

		if (mTex == null)
		{
			int size = 32 << (int)cubemapSize;
			mTex = new RenderTexture(size, size, 16);
			mTex.isPowerOfTwo = true;
			mTex.isCubemap = true;
			mTex.hideFlags = HideFlags.HideAndDontSave;

			if (!string.IsNullOrEmpty(textureFieldToUpdate))
			{
				mPrevious = materialToUpdate.GetTexture(textureFieldToUpdate);
				materialToUpdate.SetTexture(textureFieldToUpdate, mTex);
			}
		}

		// Render to texture
		mCamTrans.position = transform.position;
		mCam.cullingMask = cameraMask;
		mCam.nearClipPlane = cameraNearFar.x;
		mCam.farClipPlane = cameraNearFar.y;

		// Move the distant objects to the current position and render the view
		DistantObjects.AssumeCameraPosition(mCamTrans.position);
		mCam.RenderToCubemap(mTex);
		DistantObjects.RestoreCameraPosition();
	}

	void OnDisable ()
	{
		if (materialToUpdate != null && !string.IsNullOrEmpty(textureFieldToUpdate))
		{
			materialToUpdate.SetTexture(textureFieldToUpdate, mPrevious);
		}
		if (mCam != null) DestroyImmediate (mCam);
		if (mTex != null) DestroyImmediate (mTex);
	}
}