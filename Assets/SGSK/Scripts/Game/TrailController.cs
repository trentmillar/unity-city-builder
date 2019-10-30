using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[AddComponentMenu("Game/Trail Controller")]
public class TrailController : MonoBehaviour
{
	Rigidbody mRb = null;
	TrailRenderer mRen = null;
	
	void Start()
	{
		mRb = Tools.FindInParents<Rigidbody>(transform);
		if (mRb == null) Destroy(this);
		else mRen = GetComponent<TrailRenderer>();
	}
	
	void Update()
	{
		float vel = mRb.velocity.magnitude;
		mRen.time = Mathf.Clamp01(vel / 50f);
	}
}