using UnityEngine;

/// <summary>
/// Attach to a rigidbody that you want to always be sync'd over the network, whether only
/// on start, when collisions happen, or every chance possible.
/// </summary>

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Common/Network Rigidbody Sync")]
public class NetworkRigidbodySync : MonoBehaviour
{
	public enum UpdateFrequency
	{
		OnStart,
		OnCollision,
		Frequent,
	};

	public UpdateFrequency updateFrequency = UpdateFrequency.OnCollision;

	public delegate void OnPostReceive (float delta);
	public event OnPostReceive onPostReceive;

	Rigidbody mRb = null;

	// Whether to force-send all values next sync update
	bool mForceUpdate = true;

	/// <summary>
	/// Ensure we have an observer.
	/// </summary>

	void Start()
	{
		NetworkView view = NetworkManager.GetObserver(this);

		if (view != null)
		{
			if (mRb == null) mRb = rigidbody;
			view.group = NetworkManager.gameChannel;
		}
		else
		{
			Debug.LogWarning(GetType() + " " + " on " + name + " needs to have a NetworkView observing it");
		}
	}

	void OnCollisionEnter (Collision col) { if (updateFrequency != UpdateFrequency.OnStart) mForceUpdate = true; }
	void OnCollisionExit  (Collision col) { if (updateFrequency != UpdateFrequency.OnStart) mForceUpdate = true; }

	bool mSendOnStartMessage = true;

	/// <summary>
	/// Serialize the rigidbody's properties.
	/// </summary>

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (mRb == null) mRb = rigidbody;

		if (stream.isWriting)
		{
			if (!mForceUpdate && updateFrequency != UpdateFrequency.Frequent) return;

			Vector3 pos = mRb.position;
			Vector3 rot = mRb.rotation.eulerAngles;

			stream.Serialize(ref pos);
			stream.Serialize(ref rot);

			if (!mRb.isKinematic)
			{
				Vector3 vel = mRb.velocity;
				Vector3 avl = mRb.angularVelocity;

				stream.Serialize(ref vel);
				stream.Serialize(ref avl);
			}
			mForceUpdate = false;
		}
		else
		{
			if (mSendOnStartMessage)
			{
				mSendOnStartMessage = false;
				SendMessage("OnStartClientRigidbody", this, SendMessageOptions.DontRequireReceiver);
			}

			Vector3 pos = mRb.position;
			Vector3 rot = mRb.rotation.eulerAngles;

			stream.Serialize(ref pos);
			stream.Serialize(ref rot);

			float delay = (float)(Network.time - info.timestamp);

			if (mRb.isKinematic)
			{
				mRb.position = pos;
				mRb.rotation = Quaternion.Euler(rot);
			}
			else
			{
				Vector3 vel = mRb.velocity;
				Vector3 avl = mRb.angularVelocity;

				stream.Serialize(ref vel);
				stream.Serialize(ref avl);

				Quaternion newRot = Quaternion.Euler(rot) * Quaternion.Euler(avl * (delay * Mathf.Rad2Deg));

				mRb.MovePosition(pos + vel * delay);
				mRb.MoveRotation(newRot);

				mRb.velocity = vel;
				mRb.angularVelocity = avl;
			}

			// Notify the listeners
			if (onPostReceive != null) onPostReceive(delay);
		}
	}
}