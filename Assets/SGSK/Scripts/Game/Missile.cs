using UnityEngine;

[AddComponentMenu("Game/Missile")]
public class Missile : MonoBehaviour
{
	public float sensorRange = 150f;
	public float sensorAngle = 60f;
	public float updateFrequency = 0f;
	public bool oneTarget = false;
	public HeatSource currentTarget;

	Spaceship mControl;
	Transform mTrans;
	float mNextUpdate = 0f;
	float mTimeSinceTarget = 0f;
	Vector3 mTurn = Vector3.zero;

	/// <summary>
	/// Only the missile's owner should be controlling it.
	/// </summary>

	void Start ()
	{
		if (NetworkManager.IsMine(this))
		{
			mTrans = transform;
			mControl = Tools.FindInParents<Spaceship>(mTrans);
		}
		else
		{
			Destroy(this);
		}
	}

	/// <summary>
	/// Find the best target and turn the missile for a head-on collision.
	/// </summary>

	void Update ()
	{
		float time = Time.time;

		if (mNextUpdate < time)
		{
			mNextUpdate = time + updateFrequency;

			// Find the most optimal target ahead of the missile
			if (currentTarget == null || !oneTarget)
			{
				currentTarget = HeatSource.Find(mTrans.position, mTrans.rotation * Vector3.forward, sensorRange, sensorAngle);
			}

			if (currentTarget != null)
			{
				// Calculate local space direction
				Vector3 dir = (currentTarget.transform.position - mTrans.position);
				float dist = dir.magnitude;

				dir *= 1.0f / dist;
				dir = Quaternion.Inverse(mTrans.rotation) * dir;

				// Make the missile turn slower if it's far away from the target, and faster when it's close
				float turnSensivitity = 0.5f + 2.5f * (1.0f - dist / sensorRange);

				// Calculate the turn amount based on the direction
				mTurn.x = Mathf.Clamp(dir.y * turnSensivitity, -1f, 1f);
				mTurn.y = Mathf.Clamp(dir.x * turnSensivitity, -1f, 1f);

				// Locked on target
				mTimeSinceTarget = 0f;
			}
			else
			{
				// No target lock -- keep track of how long it has been
				mTimeSinceTarget += updateFrequency + Time.deltaTime;
			}

			mControl.turningInput = mTurn;
			mControl.moveInput = Vector3.forward;
		}

		// If it has been too long
		if (mTimeSinceTarget > 2f)
		{
			Explosive exp = mControl.GetComponentInChildren<Explosive>();

			if (exp != null)
			{
				exp.Explode();
			}
			else
			{
				NetworkManager.RemoteDestroy(mControl.gameObject);
			}
		}
	}
}