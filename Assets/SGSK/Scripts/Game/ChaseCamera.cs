using UnityEngine;

[AddComponentMenu("Game/Chase Camera")]
public class ChaseCamera : MonoBehaviour
{
	static public float rumble = 0f;
	static public float followFactor = 1f;
	static public Quaternion rumbleRotation = Quaternion.identity;
	
	public float desiredPitch			= 13f;		// Pitch angle that the camera tries to maintain
	public float desiredDistance		= 7f;		// Distance from the target that the camera tries to maintain
	public float followRotationSpeed	= 10f;		// The higher the value the faster the camera adjusts to rotation changes
	public float followVelocitySpeed	= 7f;		// The higher the value the faster the camera adjusts to velocity changes
	public float followVelocityLag		= 0.05f;	// The higher the value the more the camera will lag behind the target (0 for no lag)
	
	Transform mTrans;
	Quaternion mTargetRot;

	Vector3 mCurrentPos;
	Quaternion mCurrentRot;
	Vector3 mCurrentVel;
	Quaternion mBaseRot;

	Vector2 mCurrentRumble = Vector2.zero;
	Vector2 mTargetRumble = Vector2.zero;
	float mRumbleUpdate = 0f;

	Vector3 mFarTarget = new Vector3(0f, 0f, 50f);
	
	/// <summary>
	/// Register the custom late update function.
	/// </summary>

	void Start()
	{
		mBaseRot = Quaternion.Euler(desiredPitch, 0f, 0f);
		mTrans = transform;
		UpdateTransform(1f);
		SGUpdateManager.AddLateUpdate(0, this, OnLateUpdate);
	}

	/// <summary>
	/// Custom update function, guaranteed to be executed after ChaseTarget's LateUpdate.
	/// </summary>
	
	bool OnLateUpdate()
	{
		float delta = Time.deltaTime;
		UpdateTransform(delta);
		
		// Rumble the camera
		if (rumble > 0.001f)
		{
			// Pick a new vector
			if (mRumbleUpdate < Time.time)
			{
				mRumbleUpdate = Time.time + 0.01f;
				mTargetRumble.x = Random.value * 2f - 1f;
				mTargetRumble.y = Random.value * 2f - 1f;
			}

			float factor = Mathf.Clamp01(delta * 20f);
			mCurrentRumble.x = Mathf.Lerp(mCurrentRumble.x, mTargetRumble.x, factor);
			mCurrentRumble.y = Mathf.Lerp(mCurrentRumble.y, mTargetRumble.y, factor);

			float force = Mathf.Min(0.2f, rumble) * 25f;
			rumbleRotation = Quaternion.Euler(mCurrentRumble.x * force, mCurrentRumble.y * force, 0f);
			mTrans.rotation = mTrans.rotation * rumbleRotation;

			rumble -= delta;
		}
		else
		{
			rumbleRotation = Quaternion.identity;
		}
		return true;
	}

	/// <summary>
	/// Recalculate the object's position based on ChaseTarget's values.
	/// </summary>
	
	void UpdateTransform (float delta)
	{
		// Interpolate the target rotation using the follow factor.
		// 'followFactor' is usually 1 (unless the ship is damaged),
		// resulting in [target rotation = chase target rotation].
		mTargetRot = Quaternion.Slerp(mTargetRot, ChaseTarget.rotation, followFactor);

		// Interpolate the velocity rather than position as the physics updates may not be updated as
		// frequently as framerate, resulting in visible jitter. Velocity doesn't change drastically, so it's safer.
		mCurrentVel = Vector3.Lerp(mCurrentVel, ChaseTarget.velocity, Mathf.Clamp01(delta * followVelocitySpeed));
		mCurrentPos = ChaseTarget.position - mCurrentVel * followVelocityLag;

		// Interpolate the rotation for smoother results
		mCurrentRot = Quaternion.Slerp(mCurrentRot, mTargetRot, Mathf.Clamp01(delta * followRotationSpeed));
		
		// Camera's position should always be behind the target
		Vector3 camPos = mCurrentPos - (mCurrentRot * mBaseRot) * Vector3.forward * desiredDistance;

		// Raycast into the default layer
		RaycastHit hit;
		if (Physics.Raycast(mCurrentPos, camPos - mCurrentPos, out hit, desiredDistance, 1)) camPos = hit.point;

		// Camera's rotation should look at what the target is looking at
		mTrans.position = camPos;
		mTrans.rotation = Quaternion.LookRotation(mCurrentPos - camPos + mCurrentRot * mFarTarget, mCurrentRot * Vector3.up);
	}
}