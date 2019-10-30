using UnityEngine;

[RequireComponent(typeof(Spaceship))]
[AddComponentMenu("Game/Ship Collision Reaction")]
public class ShipCollisionReaction : MonoBehaviour
{
	Spaceship mSc;

	void Start ()
	{
		if (NetworkManager.IsMine(this))
		{
			mSc = GetComponent<Spaceship>();
		}
		else
		{
			Destroy(this);
		}
	}

	void OnSpaceshipCollision (Collision col)
	{
		if (!enabled) return;
		float impactForce = col.impactForceSum.magnitude;
		float seconds = Mathf.Max(0f, (impactForce - 15f) * 0.1f);
		mSc.DamageNavigation(seconds);
		GameUnit gu = mSc.GetComponent<GameUnit>();
		if (gu != null) gu.ApplyDamage(impactForce * 2f);
		if (Player.ship == mSc) ChaseCamera.rumble += impactForce / 50f;
	}

	void Update ()
	{
		if (Player.ship == mSc) ChaseCamera.followFactor = mSc.navigation;
	}
}