using UnityEngine;

/// <summary>
/// Trivial progress bar component -- takes a UISprite and scales it depending on the specified 'factor' value.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("UI/Progress Bar")]
[RequireComponent(typeof(SGSprite))]
public class SGProgressBar : MonoBehaviour
{
	public float factor = 1f;

	protected SGSprite mSprite;
	protected Transform mSpriteTrans;

	float mSpriteScale;
	float mSpriteSize;
	float mFactor = 1f;

	/// <summary>
	/// Remember the sprite's original dimensions.
	/// </summary>

	void Start ()
	{
		mSprite = GetComponent<SGSprite>();
		mSpriteTrans = mSprite.transform;
		mSpriteScale = mSpriteTrans.localScale.x;
		mSpriteSize = mSprite.textureRect.width;
		OnStart();
	}

	/// <summary>
	/// Restore the sprite's original dimensions.
	/// </summary>

	void OnDestroy ()
	{
		mFactor = factor = 1f;
		Rescale();
	}

	/// <summary>
	/// Update the sprite, and if the factor changes -- rescale it.
	/// </summary>

	void Update ()
	{
		if (Application.isPlaying) OnUpdate();

		factor = Mathf.Clamp01(factor);

		if (mFactor != factor)
		{
			mFactor = factor;
			Rescale();
		}
	}

	/// <summary>
	/// Rescale the sprite.
	/// </summary>

	void Rescale ()
	{
		if (mSpriteTrans != null)
		{
			Vector3 scale = mSpriteTrans.localScale;
			scale.x = mSpriteScale * factor;
			mSpriteTrans.localScale = scale;
		}

		if (mSprite != null)
		{
			Rect rect = mSprite.textureRect;
			rect.width = mSpriteSize * factor;
			mSprite.textureRect = rect;
		}
		OnChange();
	}

	/// <summary>
	/// Custom functionality for derived classes -- such as change the color of the progress bar.
	/// </summary>

	protected virtual void OnStart () { }
	protected virtual void OnUpdate () { }
	protected virtual void OnChange () { }
}