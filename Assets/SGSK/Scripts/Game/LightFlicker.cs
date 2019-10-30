using UnityEngine;

[RequireComponent(typeof(Light))]
[AddComponentMenu("Game/Light Flicker")]
public class LightFlicker : MonoBehaviour
{
	public Vector2 variationPercent = new Vector2(0.85f, 1.15f);
	public float changeFrequency = 0.01f;
	public float changeSpeed = 30f;

	Light mLight;
	float mOriginal;
	float mTarget;
	float mNextChange = 0f;

	void Start()
	{
		mLight = GetComponent<Light>();
		mOriginal = mLight.intensity;
		mTarget = mOriginal;
	}

	void Update()
	{
		if (mNextChange < Time.time)
		{
			mNextChange = Time.time + changeFrequency;
			mTarget = mOriginal * Random.Range(variationPercent.x, variationPercent.y);
		}
		mLight.intensity = Mathf.Lerp(mLight.intensity, mTarget, Time.deltaTime * changeSpeed);
	}
}

