using UnityEngine;

/// <summary>
/// Makes the game object match the camera's rotation.
/// TODO: Can't this be replaced with CopyCamera?
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("Common/Face Camera")]
public class FaceCamera : MonoBehaviour
{
	public Camera cameraToFace;

	Transform mMyTrans;
	Transform mCamTrans;

	/// <summary>
	/// Register the late update callback.
	/// </summary>

	void Start () { SGUpdateManager.AddLateUpdate(4, this, OnLateUpdate); }

	/// <summary>
	/// Executed in edit mode.
	/// </summary>
	
	void Update () { if (!Application.isPlaying) { OnLateUpdate(); } }

	/// <summary>
	/// Executed in play mode, or manually via edit mode.
	/// </summary>

	bool OnLateUpdate ()
	{
		if (cameraToFace == null) cameraToFace = Camera.main;
		if (mCamTrans == null && cameraToFace != null) mCamTrans = cameraToFace.transform;

		if (mCamTrans != null)
		{
			Quaternion rot = mCamTrans.rotation;
			if (mMyTrans == null) mMyTrans = transform;
			if (rot != mMyTrans.rotation) mMyTrans.rotation = rot;
		}
		return true;
	}
}