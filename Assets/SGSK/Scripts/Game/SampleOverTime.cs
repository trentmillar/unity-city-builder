using UnityEngine;

[AddComponentMenu("Game/Sample Over Time")]
public class SampleOverTime : MonoBehaviour
{
	public float timePerPoint = 1f;
	public Transform[] controlPoints;

	SplineV mPos;
	SplineQ mRot;
	Transform mTrans;

	public void Restart ()
	{
		if (controlPoints != null)
		{
			mPos = new SplineV();
			mRot = new SplineQ();

			float time = Time.time;

			foreach (Transform t in controlPoints)
			{
				mPos.AddKey(time, t.position);
				mRot.AddKey(time, t.rotation);
				time += timePerPoint;
			}
		}
	}

	void Start ()
	{
		mTrans = transform;
		Restart();
	}

	void Update ()
	{
		if (controlPoints != null)
		{
			mTrans.position = mPos.Sample(Time.time, true);
			mTrans.rotation = mRot.Sample(Time.time, true);
		}

		if (Input.GetKeyDown(KeyCode.R)) Restart();
	}
}