using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Game/Spaceship")]
public class Spaceship : MonoBehaviour
{
	public PowerGenerator powerGenerator = null;
	public float maximumSpeed			= 150f;
	public float mainThruster			= 1f;
	public float maneuveringThrusters	= 1f;
	public float turnThrusters			= 1f;
	public float inertiaDampeners		= 1f;

	Rigidbody mRb = null;
	Transform mTrans = null;
	NetworkView mView = null;
	Quaternion mCurrentRot;
	bool mControlled = false;

	float mNavigation		= 1f;	// The current amount of power to the navigation system
	float mFullNavTime		= 0f;	// When the power reaches 100%
	float mAppliedThrust	= 0f;	// Amount of thrust currently applied in any direction
	float mVelocity			= 0f;
	
	Vector3 mTurn 		= Vector3.zero;
	Vector3 mMove		= Vector3.zero;
	Vector3 mSmoothMove	= Vector3.zero;
	Vector3 mTorque 	= Vector3.zero;

	// List of all children, used to send messages
	Transform[] mChildren = null;

	// Values used for network synchronization
	Vector3 mLastMove	= Vector3.zero;
	Vector3 mLastTurn	= Vector3.zero;
	double	mLastPacketTime = 0f;

	// Whether to force-sync the ship's information on the next network update
	bool mForceSync = false;
	
	/// <summary>
	/// Relative -1 to 1 range steer movement.
	/// </summary>
	
	public Vector3 movement { get { return mSmoothMove; } }

	/// <summary>
	/// Relative -1 to 1 range movement input.
	/// </summary>

	public Vector3 moveInput { get { return mMove; } set { mMove = value; } }

	/// <summary>
	/// Relative -1 to 1 range turning input.
	/// </summary>

	public Vector3 turningInput { get { return mTurn; } set { mTurn = value; } }

	/// <summary>
	/// Amount of thrust currently applied in any direction.
	/// </summary>
	
	public float appliedThrust { get { return mAppliedThrust; } }

	/// <summary>
	/// The amount of power currently available to the navigation system.
	/// Damaged by collisions and by taking damage without shields.
	/// </summary>

	public float navigation { get { return mNavigation; } }

	/// <summary>
	/// Current speed at which the ship is traveling.
	/// </summary>

	public float currentSpeed { get { return mVelocity; } }

	/// <summary>
	/// Determine whether we own this ship, input type, and ensure we have a network observer.
	/// </summary>
	
	void Start()
	{
		mRb = rigidbody;
		mTrans = transform;
		mChildren = GetComponentsInChildren<Transform>();
		mControlled = !NetworkManager.isConnected || NetworkManager.IsMine(this);
		if (mControlled) mRb.isKinematic = false;

		mView = NetworkManager.GetObserver(this);
		if (powerGenerator == null) powerGenerator = GetComponentInChildren<PowerGenerator>();

		if (mView != null)
		{
			mView.group = NetworkManager.gameChannel;
		}
		else
		{
			mView = networkView;
			Debug.LogWarning(GetType() + " " + " on " + name + " needs to have a NetworkView observing it");
		}
	}

	/// <summary>
	/// Update the smooth movement value. In LateUpdate because mMove and mTurn can be updated in another script's Update.
	/// </summary>

	void LateUpdate ()
	{
		// Adjust the power
		mNavigation = 1.0f - Mathf.Clamp01(mFullNavTime - Time.time);

		// Interpolate for smoother result
		mSmoothMove = Vector3.Lerp(mSmoothMove, mMove, Mathf.Clamp01(Time.deltaTime * 10f));
	}

	/// <summary>
	/// Update the steering and torque -- both use physics, so they are in FixedUpdate.
	/// </summary>

	void FixedUpdate ()
	{
		UpdateSteering();
		UpdateTorque();
	}
	
	/// <summary>
	/// Steer the ship by applying proper forces.
	/// </summary>
	
	void UpdateSteering()
	{
		// Ensure that we are not traveling faster than we are allowed to
		Vector3 vel = mRb.velocity;

		// 3600 seconds in an hour, but 1000 meters in a km, 3600/1000=3.6
		float speed = vel.magnitude * 3.6f;
		
		if (speed > maximumSpeed)
		{
			vel *= maximumSpeed / speed;
			mRb.velocity = vel;
		}
		
		// Amount of thrust being applied right now
		mAppliedThrust = mSmoothMove.magnitude * mNavigation;
		
		// No point in doing these calculations if the engine is idle
		if (mAppliedThrust > 0.001f)
		{
			// Object's current local velocity
			vel = Quaternion.Inverse(mTrans.rotation) * vel;
			float factor = 1000f * Time.deltaTime * mRb.mass;
			float dampenerPower = (powerGenerator != null) ? (0.25f + 0.75f * powerGenerator.effectiveness) * inertiaDampeners : 0f;
			
			if (mMove.z < -0f)
			{
				// Anchor/brake button is held -- we want to slow down
				dampenerPower *= -0.15f;
				Vector3 dampeningForce = vel * dampenerPower;
				mRb.AddRelativeForce(dampeningForce * factor);
			}
			else
			{
				float forward = mMove.z * mainThruster;
				float right = mMove.x * maneuveringThrusters * 0.25f;
				float up = mMove.y * maneuveringThrusters * 0.25f;
				
				// Calculate the force we want to apply to the ship
				Vector3 appliedForce = new Vector3(right, up, forward);
				
				// Normalized direction of the applied force
				float mag = appliedForce.magnitude;
				Vector3 forceDirection = (mag > 0.001f) ? appliedForce * (1.0f / mag) : Vector3.forward;
				
				// Applied force should take the current engine's power into account
				appliedForce *= mNavigation;
				
				// This is how much of velocity is currently what we need it to be:
				Vector3 compatible = Vector3.Project(vel, forceDirection);
				
				// Dampener power depends on inertia dampeners strength as well as the current speed
				dampenerPower *= 0.15f * speed / maximumSpeed;
				
				// This is the incompatible velocity (for side dampeners)
				Vector3 dampeningForce = (compatible - vel) * dampenerPower;
				
				// If we're traveling in the opposite direction, we want to slow down first (in addition to the engine's force)
				float dot = Vector3.Dot(vel, forceDirection);
				if (dot < 0f) dampeningForce += vel * (dot * 0.1f * dampenerPower);
				
				// Apply the force and the incompatible velocity together
				mRb.AddRelativeForce((dampeningForce + appliedForce) * factor);
			}
		}
		mVelocity = Mathf.RoundToInt(mRb.velocity.magnitude * 3.6f);
	}
	
	/// <summary>
	/// Turn the ship by applying torque.
	/// </summary>
	
	void UpdateTorque()
	{
		float delta = Time.deltaTime;
		Vector3 torque = new Vector3(-mTurn.x, mTurn.y, -mTurn.z);

		// Tweaking number based on what feels right
		torque *= delta * 250f;

		// Calculate the offset rotation
		mTorque += torque;
		
		// Account for mass
		torque = mTorque * mRb.mass * turnThrusters;

		// Apply a dampening force to the calculated torque
		mTorque = Vector3.Lerp(mTorque, Vector3.zero, Mathf.Clamp01(delta * 5f));

		// Roll turning should be less sensitive
		torque.z *= 0.35f;

		// It seems the 'pitch' rotation is faster than 'yaw'? Slow it down manually.
		// Note to self: investigate this later.
		torque.x *= 0.6f;

		// Energy reserves affect dampeners by up to 75%
		float energyFactor = (powerGenerator != null) ? (0.25f + 0.75f * powerGenerator.effectiveness) : 0f;

		// Dampen the current angular velocity, causing the ship to gradually stop turning
		mRb.isKinematic = false;
		mRb.angularVelocity = mRb.angularVelocity * (1.0f - 0.5f * mNavigation * energyFactor);

		// Apply the calculated torque
		mRb.AddRelativeTorque(torque * mNavigation * energyFactor);
	}
	
	/// <summary>
	/// Shake the camera and damage the navigation system on collision.
	/// </summary>
	
	void OnCollisionEnter (Collision col)
	{
		if (mControlled)
		{
			mForceSync = true;

			foreach (Transform t in mChildren)
			{
				if (t != null)
				{
					t.gameObject.SendMessage("OnSpaceshipCollision", col, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	/// <summary>
	/// Send out a network update as soon as possible.
	/// </summary>

	void OnCollisionExit (Collision col)
	{
		mForceSync = true;
	}

	/// <summary>
	/// Serialize the ship's control values.
	/// </summary>

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (mRb == null) mRb = rigidbody;

		if (stream.isWriting)
		{
			mRb.isKinematic = false;

			double time = Network.time;
			int mask = 0;

			if (mForceSync || NetworkManager.forceSync || mLastPacketTime + 1.0 < time)
			{
				mask = 3;
			}
			else
			{
				if (Tools.Deviates(mLastMove, mMove, 0.05f)) mask |= 1;
				if (Tools.Deviates(mLastTurn, mTurn, 0.05f)) mask |= 2;
			}

			if (mask != 0)
			{
				char charMask = (char)mask;
				stream.Serialize(ref charMask);

				bool sendPosition = false;
				bool sendRotation = false;

				// 24 bytes
				if ((mask & 1) != 0)
				{
					mLastMove = mSmoothMove;
					stream.Serialize(ref mMove);
					stream.Serialize(ref mSmoothMove);
					sendPosition = true;
					sendRotation = true;
				}

				// 12 bytes
				if ((mask & 2) != 0)
				{
					mLastTurn = mTurn;
					stream.Serialize(ref mTurn);
					sendRotation = true;
				}

				// 24 bytes
				if (sendPosition)
				{
					Vector3 v = mRb.position;
					stream.Serialize(ref v);
					v = mRb.velocity;
					stream.Serialize(ref v);
				}

				// 24 bytes
				if (sendRotation)
				{
					Vector3 v = mRb.rotation.eulerAngles;
					stream.Serialize(ref v);
					v = mRb.angularVelocity;
					stream.Serialize(ref v);
				}

				// Remember the last time we sent all the data at once
				if (mask == 3)
				{
					mLastPacketTime = time;
					mForceSync = false;
				}
			}
		}
		else if (mLastPacketTime < info.timestamp)
		{
			mLastPacketTime = info.timestamp;

			char charMask = (char)0;
			stream.Serialize(ref charMask);
			int mask = charMask;

			bool readPosition = false;
			bool readRotation = false;

			if ((mask & 1) != 0)
			{
				stream.Serialize(ref mMove);
				stream.Serialize(ref mSmoothMove);
				mAppliedThrust = mSmoothMove.magnitude * mNavigation;
				readPosition = true;
				readRotation = true;
			}

			if ((mask & 2) != 0)
			{
				stream.Serialize(ref mTurn);
				readRotation = true;
			}

			if (readPosition)
			{
				Vector3 v = Vector3.zero;
				stream.Serialize(ref v);

				if (mRb.isKinematic)
				{
					mRb.position = v;
					mRb.isKinematic = false;
				}
				else mRb.MovePosition(v);

				stream.Serialize(ref v);
				mRb.velocity = v;
			}

			if (readRotation)
			{
				Vector3 v = Vector3.zero;
				stream.Serialize(ref v);

				if (mRb.isKinematic)
				{
					mRb.rotation = Quaternion.Euler(v);
					mRb.isKinematic = false;
				}
				else mRb.MoveRotation(Quaternion.Euler(v));

				stream.Serialize(ref v);
				mRb.angularVelocity = v;
			}
		}
	}

	/// <summary>
	/// Damage the navigation system.
	/// </summary>

	public void DamageNavigation (float seconds)
	{
		if (NetworkManager.isConnected)
		{
			if (!NetworkManager.HasBeenDestroyed(mView))
			{
				mView.RPC("DamageNavigationRPC", RPCMode.All, seconds);
			}
		}
		else
		{
			DamageNavigationRPC(seconds);
		}
	}

	[RPC] void DamageNavigationRPC (float seconds)
	{
		if (mFullNavTime < Time.time) mFullNavTime = Time.time;
		mFullNavTime = Mathf.Min(Time.time + 4f, mFullNavTime + seconds);
	}
}