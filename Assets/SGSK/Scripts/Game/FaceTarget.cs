using UnityEngine;

[AddComponentMenu("Game/Face Target")]
public class FaceTarget : MonoBehaviour
{
	public Transform target;
	public float speed = 5f;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
	}

	void LateUpdate ()
	{
		if (target != null)
		{
			Vector3 dir = target.position - mTrans.position;
			float mag = dir.magnitude;

			if (mag > 0.001f)
			{
				dir *= 1f / mag;
				Quaternion rot = Quaternion.LookRotation(dir);
				mTrans.rotation = (speed > 0f) ? Quaternion.Slerp(mTrans.rotation, rot, Time.deltaTime * speed) : rot;
			}
		}
	}
}