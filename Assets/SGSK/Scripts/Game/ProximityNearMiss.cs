using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Proximity - Near Miss")]
public class ProximityNearMiss : MonoBehaviour
{
	public float minVelocity = 100f;

	float mTriggerStart = 0f;
	Rigidbody mRb = null;
	List<Collider> mCols = new List<Collider>();

	void Start()
	{
		Spaceship sc = Tools.FindInParents<Spaceship>(transform);

		if (NetworkManager.IsMine(sc))
		{
			mRb = Tools.FindInParents<Rigidbody>(transform);
		}
		else Destroy(this);
	}

	void OnSpaceshipCollision (Collision col)
	{
		mTriggerStart = 0f;
	}

	void OnTriggerEnter (Collider col)
	{
		if (!mCols.Contains(col)) mCols.Add(col);
		if (mTriggerStart == 0f) mTriggerStart = Time.time;
	}

	void OnTriggerExit (Collider col)
	{
		mCols.Remove(col);
		mCols.Remove(null);

		if (mCols.Count == 0 && mTriggerStart != 0f)
		{
			EventListener.Trigger("Near Miss");
			mTriggerStart = 0f;
		}
	}

	void FixedUpdate ()
	{
		if (mTriggerStart != 0f)
		{
			float vel = mRb.velocity.magnitude * 3.6f;
			if (vel < minVelocity) mTriggerStart = 0f;
		}
	}
}