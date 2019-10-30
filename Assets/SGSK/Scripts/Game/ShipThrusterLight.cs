using UnityEngine;

[RequireComponent(typeof(Light))]
[AddComponentMenu("Game/Ship Thruster Light")]
public class ShipThrusterLight : MonoBehaviour
{
	public Vector2 variationPercent = new Vector2(0.9f, 1.1f);
	public float changeFrequency = 0.01f;
	public float changeSpeed = 30f;

	Light mLight;
	float mOriginal;
	float mTarget;
	float mNextChange = 0f;

	Spaceship mControl;
	Vector3 mDir;

	void Start()
	{
		mLight = GetComponent<Light>();
		mOriginal = mLight.intensity;
		mTarget = mOriginal;
		mControl = Tools.FindInParents<Spaceship>(transform);
		mDir = transform.rotation * Vector3.back;
	}

	void Update()
	{
		if (mNextChange < Time.time)
		{
			mNextChange = Time.time + changeFrequency;
			mTarget = mOriginal * Random.Range(variationPercent.x, variationPercent.y);
		}

		Vector3 move = mControl.movement;
		float dot = Mathf.Min(1f + move.z, 1f) * Vector3.Dot(move, mDir);
		mLight.intensity = dot * Mathf.Lerp(mLight.intensity, mTarget, Time.deltaTime * changeSpeed);
		mLight.enabled = (mLight.intensity > 0.01f);
	}
}