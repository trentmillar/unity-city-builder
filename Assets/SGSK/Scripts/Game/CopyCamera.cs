using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Game/Copy Camera")]
public class CopyCamera : MonoBehaviour
{
	public Camera cameraToCopy;
	public bool position = true;
	public bool rotation = true;
	public bool fieldOfView = true;
	public float positionScale = 1f;
	
	Camera mCam;
	
	Transform mTrans;
	Transform mTarget;
	
	/// <summary>
	/// Cache some values and register the
	/// </summary>

	void Start()
	{
		if (cameraToCopy != null)
		{
			mCam = camera;
			mTrans = transform;
			mTarget = cameraToCopy.transform;
			SGUpdateManager.AddLateUpdate(1, this, OnLateUpdate);
		}
		else if (Application.isPlaying)
		{
			Destroy(this);
		}
	}

	/// <summary>
	/// Executed in edit mode.
	/// </summary>

	void Update () { if (!Application.isPlaying) { OnLateUpdate(); } }
	
	/// <summary>
	/// Executed in play mode, or manually via edit mode.
	/// </summary>

	bool OnLateUpdate()
	{
		if (position)
		{
			Vector3 pos = mTarget.position * positionScale;
			if (pos != mTrans.position) mTrans.position = pos;
		}
		if (rotation)
		{
			Quaternion rot = mTarget.rotation;
			if (rot != mTrans.rotation) mTrans.rotation = rot;
		}
		if (fieldOfView && mCam != null)
		{
			float fov = cameraToCopy.fieldOfView;
			if (fov != mCam.fieldOfView) mCam.fieldOfView = fov;
		}
		return true;
	}
}