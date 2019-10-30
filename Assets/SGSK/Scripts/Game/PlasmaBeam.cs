using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(FiredObject))]
[AddComponentMenu("Game/Plasma Beam")]
public class PlasmaBeam : MonoBehaviour
{
	public float damageOnHit = 10f;
	public float forceOnCollision = 0.2f;
	public Color color = new Color(1f, 60f / 255f, 30f / 255f, 1f);
	public AnimationCurve sizeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 1f) });

	FiredObject mFo;
	Transform mTrans;
	Renderer mRen;
	Material mMat;
	bool mIsMine = true;

	/// <summary>
	/// Cache the needed components and set the beam's color.
	/// </summary>

	void Start ()
	{
		mFo = GetComponent<FiredObject>();
		mTrans = transform;
		mRen = renderer;
		mMat = mRen.material;
		mMat.color = color;
		mIsMine = NetworkManager.IsMine(this);
	}

	/// <summary>
	/// Color, grow or shrink the beam depending on its lifetime and the specified size curve.
	/// </summary>

	void Update ()
	{
		float life = mFo.lifetime;
		Color c = color;
		c.a = 1f - life * life;
		mMat.color = c;
		mTrans.localScale = Vector3.one * sizeCurve.Evaluate(life);
	}

	/// <summary>
	/// When the plasma beam hits something we want to apply damage and destroy the beam.
	/// </summary>

	void OnTriggerEnter (Collider col)
	{
		if (!mIsMine) return;

		Rigidbody rb = Tools.FindInParents<Rigidbody>(col.transform);

		if (rb != null)
		{
			Explosive exp = rb.GetComponentInChildren<Explosive>();

			if (exp != null)
			{
				exp.Explode();
			}
			else
			{
				Rigidbody myRb = rigidbody;

				if (myRb != null)
				{
					Vector3 force = myRb.velocity * myRb.mass * forceOnCollision;
					Vector3 pos = transform.position;

					NetworkRigidbody nrb = NetworkRigidbody.Find(rb);
					if (nrb != null) nrb.AddForceAtPosition(force, pos);
				}
			}
		}

		// Apply damage to the unit, if we hit one
		GameUnit unit = Tools.FindInParents<GameUnit>(col.transform);
		if (unit != null) unit.ApplyDamage(damageOnHit);

		// Destroy this beam
		NetworkManager.RemoteDestroy(gameObject);
	}
}