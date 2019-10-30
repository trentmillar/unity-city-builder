using UnityEngine;

/// <summary>
/// Instantiate the specified prefab.
/// </summary>

[AddComponentMenu("Common/Network Instantiate")]
public class NetworkInstantiate : MonoBehaviour
{
	// Game object that will be instantiated
	public GameObject prefab;

	// If you want to spawn this object with a randomized offset, specify a value higher than 0 (ex: randomized player spawn point)
	public float offsetRandomization = 0f;

	void Start ()
	{
		if (prefab != null)
		{
			// If we're currently connected, we'll be handling instantiation in OnNetworkStart instead
			if (NetworkManager.isConnected) return;

			// We're not currently connected -- local instantiation
			Instantiate(prefab, transform.position + (Random.rotation * Vector3.forward) * offsetRandomization, transform.rotation);
		}
		Destroy(gameObject);
	}

	void OnNetworkStart ()
	{
		Network.Instantiate(prefab, transform.position + (Random.rotation * Vector3.forward) * offsetRandomization,
			transform.rotation, NetworkManager.gameChannel);
		Destroy(gameObject);
	}
}
