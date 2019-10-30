using UnityEngine;

[AddComponentMenu("Game/Reset Transform")]
public class ResetTransform : MonoBehaviour
{
	public bool position = false;
	public bool rotation = false;

	Transform mTrans;
	Vector3 mPos;
	Quaternion mRot;

	void Start()
	{
		mTrans = transform;
		mPos = mTrans.position;
		mRot = mTrans.rotation;
	}

	void LateUpdate()
	{
		if (position) mTrans.position = mPos;
		if (rotation) mTrans.rotation = mRot;
	}
}