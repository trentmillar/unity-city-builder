using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Apply Gravity While Inside")]
public class ApplyGravityWhileInside : MonoBehaviour
{
	public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
	public bool twoSidedGravity = false;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
		ProximityManager.AddOnUpdate(this, OnUpdate);
	}

	bool OnUpdate (List<ProximityManager.Entry> list)
	{
		foreach (ProximityManager.Entry ent in list)
		{
			Quaternion myRot = mTrans.rotation;
			Vector3 force = myRot * gravity * (Time.deltaTime * 100f * ent.rb.mass);

			// In case of two-sided gravity, it should be affected by the dot product
			if (twoSidedGravity) force *= Vector3.Dot(ent.rb.transform.up, mTrans.up);

			// Apply the force to the rigidbody
			ent.rb.AddForce(force);
		}
		return true;
	}
}