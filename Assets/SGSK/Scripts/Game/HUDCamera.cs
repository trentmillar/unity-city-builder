using UnityEngine;

[AddComponentMenu("Game/HUD Camera")]
public class HUDCamera : MonoBehaviour
{
	static public Camera cam;
	Transform mTrans;

	void Awake ()
	{
		if (cam == null) cam = camera;
	}

	void Start ()
	{
		mTrans = transform;
		SGUpdateManager.AddLateUpdate(2, this, OnLateUpdate);
	}

	bool OnLateUpdate ()
	{
		mTrans.localRotation = ChaseCamera.rumbleRotation;
		return true;
	}
}