using UnityEngine;

[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Game/Fired Object")]
public class FiredObject : MonoBehaviour
{
	/// <summary>
	/// How long the object will live from the time it has been created.
	/// </summary>

	public float timeToLive = 10f;
	public float firingVelocity = 150f;
	public float firingFrequency = 0.25f;
	public float energyCost = 2f;

	Collider[] mColliders;
	float mSpawnTime = 0f;
	float mDestroyTime = 0f;
	bool mIsMine = true;

	// Colliders that have been ignored for the sake of the fired object not hitting the weapon that fired it
	Collider[] mIgnoredColliders;

	/// <summary>
	/// Colliders that should be ignored. Set by WeaponLauncher, and removed automatically after some time has passed.
	/// </summary>

	public Collider[] ignoreColliders { set { mIgnoredColliders = value; } }

	/// <summary>
	/// Current lifetime progress of the fired object.
	/// </summary>

	public float lifetime { get { return (Time.time - mSpawnTime) / timeToLive; } }

	/// <summary>
	/// Only the missile's owner should be controlling it.
	/// </summary>

	void Start ()
	{
		mIsMine = NetworkManager.IsMine(this);
		mSpawnTime = Time.time;
		mDestroyTime = mSpawnTime + timeToLive;

		if (mIsMine)
		{
			mColliders = GetComponentsInChildren<Collider>();

			// Ignore the collision between specified colliders
			IgnoreColliders(true);
			
			// In order to avoid collisions that occur as soon as the projectile gets instantiated,
			// the correct solution is to use Physics.IgnoreCollision. Unfortunately this
			// data does not carry across the network, and using an RPC call after Network.Instantiate
			// may result in a delay (and collision callbacks) before IgnoreCollision kicks in.
			// The work-around? Start all fired objects on the "No Collisions" layer and change
			// to the "Projectile" layer only on the network instance that owns the projectile.

			int ignoreLayer = LayerMask.NameToLayer("No Collisions");
			int projectileLayer = LayerMask.NameToLayer("Projectile");

			Transform[] children = GetComponentsInChildren<Transform>();

			foreach (Transform t in children)
			{
				if (t.gameObject.layer == ignoreLayer)
				{
					t.gameObject.layer = projectileLayer;
				}
			}
		}
		else
		{
			enabled = false;
		}
	}

	/// <summary>
	/// Internal function that disables collision between all of the object's colliders and all of the specified colliders.
	/// </summary>

	void IgnoreColliders (bool val)
	{
		if (mIgnoredColliders != null && mColliders != null)
		{
			foreach (Collider mc in mColliders)
			{
				if (mc == null || !mc.enabled) continue;

				foreach (Collider rc in mIgnoredColliders)
				{
					if (rc == null || !rc.enabled) continue;
					Physics.IgnoreCollision(mc, rc, val);
				}
			}
		}
	}

	/// <summary>
	/// Restore the physics collision or destroy the fired object if enough time has passed.
	/// </summary>

	void Update ()
	{
		float time = Time.time;

		if (mIgnoredColliders != null && time - mSpawnTime > 1f)
		{
			IgnoreColliders(false);
			mIgnoredColliders = null;
		}

		if (time > mDestroyTime)
		{
			Explosive exp = GetComponentInChildren<Explosive>();

			if (exp != null)
			{
				exp.Explode();
			}
			else
			{
				NetworkManager.RemoteDestroy(gameObject);
			}
		}
	}
}