using UnityEngine;

/// <summary>
/// This script is designed to work with a UITarget attached to a parent node. It keeps an eye on the parent's
/// target, and animates the transform by scaling / rotating it when the target matches the player's target.
/// </summary>

[AddComponentMenu("UI/Target Lock")]
public class SGTargetLock : MonoBehaviour
{
	public float scaleChange = 0.7f;
	public bool reverseRotation = false;
	public float animationSpeed = 8f;

	Transform mTrans;
	Vector3 mBaseScale = Vector3.one;
	SGTarget mTarget;
	Quaternion mTargetRot = Quaternion.identity;
	bool mLocked = false;
	float mOffset = 0f;

	void Start ()
	{
		mTrans = transform;
		mBaseScale = mTrans.localScale;
		mTarget = Tools.FindInParents<SGTarget>(mTrans);

		// Just in case we forget -- UITarget must be present on this game object or on one of the parents
		if (mTarget == null) Debug.LogWarning(Tools.GetHierarchy(gameObject) + " needs a " + typeof(SGTarget) + " to work with");

		// Starting rotation
		mOffset = mTrans.localRotation.eulerAngles.z;
	}

	void Update ()
	{
		bool isPlayerTarget = (mTarget != null) && mTarget.isPlayerTarget;

		// If the locked status changes, we want to calculate the target rotation using the current one.
		if (mLocked != isPlayerTarget)
		{
			mLocked = isPlayerTarget;

			if (mLocked)
			{
				Vector3 euler = mTrans.localRotation.eulerAngles;

				euler.z -= mOffset;

				if (reverseRotation) euler.z = (euler.z < 180f) ? -180f : 0f;
				else euler.z = (euler.z < 180f) ? 360f : 540;

				euler.z += mOffset;

				mTargetRot = Quaternion.Euler(euler);
			}
		}

		float delta = Time.deltaTime * animationSpeed;

		// Target is locked -- rotate the transform to desired orientation
		if (mLocked) mTrans.localRotation = Quaternion.Slerp(mTrans.localRotation, mTargetRot, delta);

		// Scale the transform to match the base scale (if not locked) or scaled scale (if locked)
		mTrans.localScale = Vector3.Lerp(mTrans.localScale, (mLocked) ? mBaseScale * scaleChange : mBaseScale, delta);
	}
}