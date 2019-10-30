using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Explosive")]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkView))]
public class Explosive : MonoBehaviour
{
	public GameObject explosionPrefab;
	public float force = 200f;
	public float radius = 20f;

	bool mIsMine = true;

	/// <summary>
	/// We need to know if this explosive is ours or not. If it is, we'll be the ones exploding it.
	/// </summary>

	void Start ()
	{
		mIsMine = NetworkManager.IsMine(this);
	}

	/// <summary>
	/// Explode on collision.
	/// </summary>

	void OnCollisionEnter (Collision col) { if (mIsMine) Explode(); }

	/// <summary>
	/// Explode the explosive, adding an explosion force and creating an explosion prefab.
	/// </summary>

	public void Explode ()
	{
		if (mIsMine)
		{
			Rigidbody myRigidbody = rigidbody;
			Vector3 pos = transform.position;

			// Get a list of colliders caught int he blast
			Collider[] cols = Physics.OverlapSphere(pos, radius);

			// Convert the list of colliders into a list of rigidbodies
			List<Rigidbody> rbs = Tools.GetRigidbodies(cols);

			// Apply the explosion force to all rigidbodies caught in the blast
			foreach (Rigidbody rb in rbs)
			{
				if (rb != myRigidbody)
				{
					// TODO: Apply damage here
					NetworkRigidbody nrb = NetworkRigidbody.Find(rb);
					if (nrb != null) nrb.AddExplosionForce(force, pos, radius, 0f);
				}
			}

			// Instantiate an explosion prefab
			if (explosionPrefab != null)
			{
				NetworkManager.RemoteInstantiate(explosionPrefab, pos,
					Quaternion.identity, NetworkManager.gameChannel);
			}

			// Destroy this game object
			NetworkManager.RemoteDestroy(gameObject);
		}
	}
}