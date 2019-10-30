using UnityEngine;

[AddComponentMenu("Game/Weapon Launcher")]
public class WeaponLauncher : Weapon
{
	public PowerGenerator generator;

	protected Transform mTrans;
	protected Rigidbody mRb;
	protected GameUnit mUnit;
	protected bool mIsPlayerControlled = true;

	Collider[] mCollidersToIgnore;
	float mNextFire = 0f;

	/// <summary>
	/// Whether the weapon launcher can fire.
	/// </summary>

	public override bool canFire
	{
		get
		{
			return (firedObject != null) && (generator != null) &&
				(generator.currentReserve >= firedObject.energyCost) &&
				(mNextFire < Time.time);
		}
	}

	/// <summary>
	/// Only keep this script around if we're the owner.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		mRb = Tools.FindInParents<Rigidbody>(mTrans);
		mUnit = Tools.FindInParents<GameUnit>(mTrans);
		mCollidersToIgnore = mRb.GetComponentsInChildren<Collider>();
		mIsPlayerControlled = (mUnit != null) && (Player.unit == mUnit);

		if (generator == null) generator = GetComponent<PowerGenerator>();
		if (generator == null) generator = mRb.gameObject.GetComponentInChildren<PowerGenerator>();
		if (generator == null) Debug.LogWarning("No generator powering " + Tools.GetHierarchy(gameObject));

		OnStart();
	}

	/// <summary>
	/// Fire the weapon.
	/// </summary>

	public override void Fire ()
	{
		if (!mIsPlayerControlled || !canFire) return;

		float remainder = generator.DrainPower(firedObject.energyCost);
		
		if (remainder > 0f)
		{
			Debug.Log("TODO: Some kind of 'out of power' message");
			return;
		}

		mNextFire = Time.time + firedObject.firingFrequency;

		// Instantiate a new object
		GameObject go = NetworkManager.RemoteInstantiate(prefab, mTrans.position, mTrans.rotation);

		// The weapon's initial velocity should match the launcher's
		if (go != null)
		{
			if (mCollidersToIgnore != null)
			{
				FiredObject fo = go.GetComponent<FiredObject>();
				if (fo != null) fo.ignoreColliders = mCollidersToIgnore;
			}

			NetworkRigidbody nrb = go.GetComponent<NetworkRigidbody>();

			if (nrb != null)
			{
				if (mRb != null)
				{
					nrb.SetVelocity(mRb.velocity + mTrans.rotation * (Vector3.forward * (firedObject.firingVelocity / 3.6f)));
				}
				else
				{
					nrb.SetVelocity(mTrans.rotation * (Vector3.forward * (firedObject.firingVelocity / 3.6f)));
				}
			}
			else
			{
				Debug.LogError("No " + typeof(NetworkRigidbody) + " found on " + Tools.GetHierarchy(go));
			}

			// Any additional functionality
			OnFire(go);
		}
	}

	/// <summary>
	/// Optional virtual functionality.
	/// </summary>

	protected virtual void OnStart () { }

	/// <summary>
	/// Optional post-fire functionality.
	/// </summary>

	protected virtual void OnFire (GameObject go) { }
}