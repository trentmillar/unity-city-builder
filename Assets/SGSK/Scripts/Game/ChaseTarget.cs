using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Chase Camera Target")]
public class ChaseTarget : MonoBehaviour
{
	public static Vector3 position = Vector3.zero;
	public static Quaternion rotation = Quaternion.identity;
	public static Vector3 velocity = Vector3.zero;

	Transform mTrans;
	Rigidbody mRb;

	void Start ()
	{
		if (!NetworkManager.isConnected || NetworkManager.IsMine(this))
		{
			mTrans = transform;
			mRb = rigidbody;
			SGUpdateManager.AddLateUpdate(-1, this, OnLateUpdate);
			OnLateUpdate();
		}
		else
		{
			Destroy(this);
		}
	}

	bool OnLateUpdate()
	{
		position = mTrans.position;
		rotation = mTrans.rotation;
		velocity = (mRb != null) ? mRb.velocity : Vector3.zero;
		return true;
	}
}
