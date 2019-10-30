using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Game/Set Angular Velocity")]
public class SetAngularVelocity : MonoBehaviour
{
	public Vector3 rotationsPerMinute = new Vector3(0f, 0f, 1f);

	void Start()
	{
		// Convert RPM to angular velocity
		rigidbody.angularVelocity = rotationsPerMinute * (Mathf.PI * 2f);
		Destroy(this);
	}
}