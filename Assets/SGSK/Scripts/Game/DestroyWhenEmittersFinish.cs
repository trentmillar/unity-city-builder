using UnityEngine;

[AddComponentMenu("Game/Destroy When Emitters Finish")]
public class DestroyWhenEmittersFinish : MonoBehaviour
{
	ParticleEmitter[] mEmitters;

	void Start ()
	{
		if (NetworkManager.IsMine(this))
		{
			mEmitters = GetComponentsInChildren<ParticleEmitter>();
		}
		else
		{
			Destroy(this);
		}
	}

	void Update ()
	{
		if (mEmitters != null)
		{
			foreach (ParticleEmitter em in mEmitters)
			{
				if (em != null && em.emit) return;
			}
			mEmitters = null;
		}
		NetworkManager.RemoteDestroy(gameObject);
	}
}