using UnityEngine;

/// <summary>
/// Want something to spin? Attach this script to it. Works equally well with rigidbodies as without.
/// </summary>

[AddComponentMenu("Common/Spin")]
public class SGSpinTransform : MonoBehaviour
{
	public Vector3 rpm = new Vector3(0f, 0f, 1f);

	Rigidbody mRb;
	Transform mTrans;
	float mLastTime = 0f;

	void Start ()
	{
		mTrans = transform;
		mRb = rigidbody;
	}

	void Update ()
	{
		if (mRb == null)
		{
			ApplyDelta(mLastTime == 0f ? Time.deltaTime : Time.time - mLastTime);
		}
	}
	void FixedUpdate ()
	{
		if (mRb != null)
		{
			ApplyDelta(mLastTime == 0f ? Time.deltaTime : Time.time - mLastTime);
		}
	}

	void OnStartClientRigidbody (NetworkRigidbodySync nr)
	{
		Start();
		nr.onPostReceive += ApplyDelta;
	}

	public void ApplyDelta (float delta)
	{
		delta *= Mathf.Rad2Deg * Mathf.PI * 2f;
		Quaternion offset = Quaternion.Euler(rpm * delta);

		if (mRb == null)
		{
			mTrans.rotation = mTrans.rotation * offset;
		}
		else
		{
			mRb.MoveRotation(mRb.rotation * offset);
		}
		mLastTime = Time.time;
	}
}