using UnityEngine;

[AddComponentMenu("Game/Missile Launcher")]
public class MissileLauncher : WeaponLauncher
{
	Missile mMissile;
	HeatSource mTarget;

	/// <summary>
	/// Whether the missile launcher can fire.
	/// </summary>

	public override bool canFire { get { return (mTarget != null) && base.canFire; } }

	/// <summary>
	/// Only keep this script around if we're the owner.
	/// </summary>

	protected override void OnStart ()
	{
		mMissile = firedObject.GetComponent<Missile>();
		if (mMissile == null) Debug.LogWarning("No missile found");
	}

	/// <summary>
	/// Assign the missile's initial target.
	/// </summary>

	protected override void OnFire (GameObject go)
	{
		Missile ms = go.GetComponent<Missile>();
		if (ms != null) ms.currentTarget = mTarget;
	}

	/// <summary>
	/// Keep the reticle updated
	/// </summary>

	void Update ()
	{
		if (!mIsPlayerControlled) return;

		// Find the target in front of the missile launcher that the missile would lock on
		if (mMissile != null && base.canFire)
		{
			mTarget = HeatSource.Find(mTrans.position, mTrans.rotation * Vector3.forward,
				mMissile.sensorRange, mMissile.sensorAngle);
		}
		else mTarget = null;

		// Update the player target if this is the player's missile launcher
		Player.target = (mTarget == null) ? null : mTarget.transform;
	}
}