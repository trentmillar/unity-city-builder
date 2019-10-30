using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Common/Distant Objects")]
public class DistantObjects : MonoBehaviour
{
	static public DistantObjects instance = null;

	public float scale = 0.001f;
	
	Transform mTrans;
	Transform mCamTrans;
	Vector3 mInitialOffset;
	Vector3 mCameraOrigin;

	static public void RestoreCameraPosition () { if (instance != null) AssumeCameraPosition(instance.mCamTrans.position); }

	static public void AssumeCameraPosition (Vector3 pos)
	{
		if (instance != null)
		{
			// The distant objects are always positioned relative to the main camera.
			// We can assume a different position if we wanted to render from another camera's POV.
			Vector3 offset = instance.mInitialOffset + (pos - instance.mCameraOrigin) * instance.scale;
			pos = pos - offset;
			if (instance.mTrans.position != pos) instance.mTrans.position = pos;
		}
	}

	void OnStart ()
	{
		if (instance != null)
		{
			if (!Application.isEditor)
			{
				Debug.LogWarning("Can only have one DistantObjects script in the scene!");
				Destroy(this);
			}
			return;
		}
		instance = this;
		mTrans = transform;
		mCamTrans = Camera.main.transform;
		mCameraOrigin = mCamTrans.position;
		mInitialOffset = mCameraOrigin - mTrans.position;

		if (Application.isPlaying)
		{
			SGUpdateManager.AddLateUpdate(10, this, CustomUpdate);
		}
	}

	void Awake () { if (Application.isPlaying) OnStart(); }

	void Start () { if (!Application.isPlaying) OnStart(); }

	void OnDestroy () { if (instance == this) instance = null; }

	void Update () { if (!Application.isPlaying) { RestoreCameraPosition(); } }

	bool CustomUpdate () { RestoreCameraPosition(); return true; }
}