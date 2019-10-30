using UnityEngine;

/// <summary>
/// Attach to a game object to make its rotation always lag behind its parent as the parent rotates.
/// </summary>

[AddComponentMenu("Game/Lag Rotation")]
public class SGLagRotation : MonoBehaviour
{
	public float speed = 5f;
	
	Transform mTrans;
	Quaternion mRelative;
	Quaternion mAbsolute;
	
	void Start()
	{
		mTrans = transform;
		mRelative = mTrans.localRotation;
		mAbsolute = mTrans.rotation;
	}

	void LateUpdate()
	{
		Transform parent = mTrans.parent;
		
		if (parent != null)
		{
			mAbsolute = Quaternion.Slerp(mAbsolute, parent.rotation * mRelative, Time.deltaTime * speed);
			mTrans.rotation = mAbsolute;
		}
	}
}