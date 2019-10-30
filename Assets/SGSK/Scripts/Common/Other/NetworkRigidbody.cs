using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper class that allows applying forces to the rigidbody over the network.
/// </summary>

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkView))]
public class NetworkRigidbody : MonoBehaviour
{
	static List<NetworkRigidbody> mList = new List<NetworkRigidbody>();

	NetworkView mView;
	Rigidbody mRb;

	/// <summary>
	/// Helper function that finds a NetworkRigidbody script for the specified rigidbody.
	/// </summary>

	static public NetworkRigidbody Find (Rigidbody rb)
	{
		foreach (NetworkRigidbody n in mList)
		{
			if (n.mRb == rb)
			{
				return n;
			}
		}
		return null;
	}

	/// <summary>
	/// Add this instance to the list.
	/// </summary>

	void Awake () { mList.Add(this); }

	/// <summary>
	/// Remove this instance from the list.
	/// </summary>

	void OnDestroy () { mList.Remove(this); }

	/// <summary>
	/// Cache the references.
	/// </summary>

	void Start ()
	{
		if (mView == null) mView = networkView;
		if (mRb == null) mRb = rigidbody;
	}

	/// <summary>
	/// Set the rigidbody's velocity to the specified value.
	/// </summary>

	public void SetVelocity (Vector3 vel)
	{
		Start();

		if (NetworkManager.isConnected)
		{
			if (!NetworkManager.HasBeenDestroyed(mView))
			{
				mView.RPC("OnSetVelocity", RPCMode.All, vel);
			}
		}
		else
		{
			OnSetVelocity(vel);
		}
	}

	[RPC] void OnSetVelocity (Vector3 vel)
	{
		if (mRb == null) mRb = rigidbody;
		mRb.isKinematic = false;
		mRb.velocity = vel;
	}

	/// <summary>
	/// Add a force at the specified position.
	/// </summary>

	public void AddForceAtPosition (Vector3 force, Vector3 pos)
	{
		Start();

		if (NetworkManager.isConnected)
		{
			if (!NetworkManager.HasBeenDestroyed(mView))
			{
				mView.RPC("OnAddForceAtPosition", RPCMode.All, force, pos);
			}
		}
		else
		{
			OnAddForceAtPosition(force, pos);
		}
	}

	[RPC] void OnAddForceAtPosition (Vector3 force, Vector3 pos)
	{
		if (mRb == null) mRb = rigidbody;

		if (!mRb.isKinematic)
		{
			mRb.AddForceAtPosition(force, pos);
		}
	}

	/// <summary>
	/// Add an explosion force at the specified position.
	/// </summary>

	public void AddExplosionForce (float force, Vector3 pos, float radius, float upwardsModifier)
	{
		Start();

		if (NetworkManager.isConnected)
		{
			if (!NetworkManager.HasBeenDestroyed(mView))
			{
				mView.RPC("OnAddExplosionForce", RPCMode.All, force, pos, radius, upwardsModifier);
			}
		}
		else
		{
			OnAddExplosionForce(force, pos, radius, upwardsModifier);
		}
	}

	[RPC] void OnAddExplosionForce (float force, Vector3 pos, float radius, float upwardsModifier)
	{
		if (mRb == null) mRb = rigidbody;
		mRb.AddExplosionForce(force, pos, radius, upwardsModifier);
	}
}