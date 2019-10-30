using UnityEngine;

[AddComponentMenu("Game/Destroy if not owned")]
public class DestroyIfNotOwned : MonoBehaviour
{
	void Start ()
	{
		if (!NetworkManager.IsMine(this)) Destroy(gameObject);
		else Destroy(this);
	}
}