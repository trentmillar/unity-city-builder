using UnityEngine;

[RequireComponent(typeof(ParticleEmitter))]
[AddComponentMenu("Game/Ship Thruster Emitter")]
public class ShipThrusterEmitter : MonoBehaviour
{
	ParticleEmitter mEmitter;
	Spaceship mControl;
	Vector2 mEmission;
	Vector3 mDir;

	void Start()
	{
		mEmitter = GetComponent<ParticleEmitter>();
		mControl = Tools.FindInParents<Spaceship>(transform);
		mEmission = new Vector2(mEmitter.minEmission, mEmitter.maxEmission);
		mDir = transform.rotation * Vector3.back;
	}

	void Update()
	{
		Vector3 move = mControl.movement;
		float dot = Mathf.Min(1f + move.z, 1f) * Vector3.Dot(move, mDir);
		mEmitter.minEmission = mEmission.x * dot;
		mEmitter.maxEmission = mEmission.y * dot;
	}
}