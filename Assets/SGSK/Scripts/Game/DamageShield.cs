using UnityEngine;

[RequireComponent(typeof(Renderer))]
[AddComponentMenu("Game/Damage Shield")]
public class DamageShield : DamageReduction
{
	public float powerPerDamage = 2f;
	public PowerGenerator generator;

	Renderer mRen;
	Material mMat;
	Spaceship mShip;
	float mHitTime = 0f;

	/// <summary>
	/// Locate the spaceship and the power generator components.
	/// </summary>

	void Start()
	{
		mRen = GetComponent<Renderer>();
		mMat = mRen.material;

		Transform t = transform;
		mShip = Tools.FindInParents<Spaceship>(t);
		if (generator == null) generator = Tools.FindInParents<PowerGenerator>(t);
	}

	/// <summary>
	/// Update the visible shield.
	/// </summary>

	void Update()
	{
		if (generator == null)
		{
			mRen.enabled = false;
		}
		else
		{
			Color c = mMat.color;
			float shipAlpha = (mShip != null) ? Mathf.Max(0.0f, -mShip.movement.z) : 0f;
			float timeAlpha = Mathf.Clamp01((Time.time - mHitTime) * 2f);
			c.a = Mathf.Max(shipAlpha, 1f - timeAlpha) * generator.effectiveness;
			mMat.color = c;
			mRen.enabled = (c.a > 0.001f);
		}
	}

	/// <summary>
	/// Function triggered by GameUnit.
	/// </summary>

	public override float OnAbsorbDamage (float damage)
	{
		mHitTime = Time.time;
		return (generator != null && powerPerDamage > 0f) ? generator.DrainPower(damage * powerPerDamage) / powerPerDamage : damage;
	}
}