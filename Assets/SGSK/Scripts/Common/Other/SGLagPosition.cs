using UnityEngine;

/// <summary>
/// Attach to a game object to make its position always lag behind its parent as the parent moves.
/// </summary>

[AddComponentMenu("Game/Lag Position")]
public class SGLagPosition : MonoBehaviour
{
	public float speed = 5f;
	
	Transform mTrans;
	Vector3 mRelative;
	Vector3 mAbsolute;
	
	void Start()
	{
		mTrans = transform;
		mRelative = mTrans.localPosition;
		mAbsolute = mTrans.position;
	}
	
	void LateUpdate()
	{
		Transform parent = mTrans.parent;
		
		if (parent != null)
		{
			mAbsolute = Vector3.Lerp(mAbsolute, parent.position + parent.rotation * mRelative,
				Time.deltaTime * speed);
			mTrans.position = mAbsolute;
		}
	}
}