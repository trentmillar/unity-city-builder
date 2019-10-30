using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("UI/Target Indicator")]
public class SGTarget : MonoBehaviour
{
	// Camera used to draw this target
	public Camera hudCamera;

	// Target object this widget is following
	public Transform target;

	// Min/max range from Camera.main that this target is visible (distance is affected by target's absolute scale)
	public Vector2 range = new Vector2(40f, 400f);

	// Optional global alpha multiplier
	public float alphaTint = 1f;

	// Destroy this object if the target gets destroyed
	public bool destroyWithTarget = true;

	// Widgets belonging to this target that should be tinted
	public SGWidget[] coloredWidgets;

	// Scripts to enable and disable when this target matches the player's target
	public MonoBehaviour[] disableOnPlayerTarget;
	public MonoBehaviour[] enableOnPlayerTarget;

	// Cached values
	Camera mMainCam;
	Transform mMainCamTrans;
	Transform mTrans;
	Transform mLastTarget;
	Transform mTargetedRoot;
	float mAlpha = 0f;
	bool mIsPlayerTarget = false;

	SGWidget[] mWidgets;
	SGProgressBar[] mBars;

	/// <summary>
	/// Whether it's the player's current target.
	/// </summary>

	public bool isPlayerTarget { get { return mIsPlayerTarget; } }

	/// <summary>
	/// Register the late update function.
	/// </summary>

	void Awake ()
	{
		mWidgets = gameObject.GetComponentsInChildren<SGWidget>();
		mBars = gameObject.GetComponentsInChildren<SGProgressBar>();

		if (mWidgets != null)
		{
			foreach (SGWidget w in mWidgets)
			{
				w.color.a = mAlpha * alphaTint;
				w.enabled = w.color.a > 0.01f;
			}
		}
		
		// Parts of the code rely on updated camera's orientation, so it should be executed after all camera updates.
		SGUpdateManager.AddLateUpdate(3, this, OnLateUpdate);
	}

	/// <summary>
	/// Locate all the required components.
	/// </summary>

	void Start()
	{
		mTrans = transform;
		mMainCam = Camera.main;
		mMainCamTrans = mMainCam.transform;
		if (hudCamera == null) hudCamera = HUDCamera.cam;
		if (hudCamera == null && Application.isPlaying)
		{
			Debug.LogError("UITarget needs a HUD camera to work with");
			Destroy(this);
		}
	}

	/// <summary>
	/// Update the target and destroy the UITarget if the target was lost.
	/// </summary>

	void Update ()
	{
		// If the target changes, find the unit responsible for the target
		if (mLastTarget != target)
		{
			mLastTarget = target;

			// The root of the target may be different if the actual target indicator is a child of a larger object
			Rigidbody rb = Tools.FindInParents<Rigidbody>(mLastTarget);
			mTargetedRoot = (rb != null) ? rb.transform : mLastTarget;
		}

		if (mLastTarget == null && destroyWithTarget)
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Executed after the cameras finish updating.
	/// </summary>

	bool OnLateUpdate ()
	{
		// Only proceed if we have a target
		if (mLastTarget != null && mMainCamTrans != null)
		{
			// Convert to view space
			Vector3 pos = mLastTarget.position;
			Vector3 vt = mMainCam.WorldToViewportPoint(pos);

			if (vt.z < 0f)
			{
				// The target is behind the screen -- make it invisible
				mAlpha = 0f;
			}
			else
			{
				float dist = (pos - mMainCamTrans.position).magnitude;
				mAlpha = (range.x < dist && dist < range.y) ? 1f : 0f;

				// Clamp to the edges of the screen
				vt.x = Mathf.Clamp01(vt.x);
				vt.y = Mathf.Clamp01(vt.y);
				vt.z = dist;

				// Convert back to world
				pos = hudCamera.ViewportToWorldPoint(vt);
				if (pos != mTrans.position) mTrans.position = pos;
			}
		}
		else mAlpha = 0f;

		// If one of the progress bars is below 1, we don't want to apply the alpha tint.
		// This way the alpha tint can be set to 0, meaning that the target will fade away
		// unless the target happens to be actually damaged.

		bool isTargeted = (mTargetedRoot == Player.target);
		bool useTint = !isTargeted;

		if (useTint)
		{
			foreach (SGProgressBar bar in mBars)
			{
				if (bar.factor < 1f)
				{
					useTint = false;
					break;
				}
			}
		}

		float factor = Mathf.Clamp01(Time.deltaTime * 8f);

		// Adjust the alpha of all the widgets belonging to the target
		foreach (SGWidget w in mWidgets)
		{
			Color c = w.color;
			c.a = Mathf.Lerp(c.a, useTint ? mAlpha * alphaTint : mAlpha, factor);
			w.color = c;
			w.enabled = w.color.a > 0.01f;
		}

		// Adjust the color of all allowed widgets
		foreach (SGWidget w in coloredWidgets)
		{
			Color c = isTargeted ? Color.red : Color.white;
			c.a = w.color.a;
			w.color = Color.Lerp(w.color, c, factor);
		}

		// Enable/disable scripts if the player target matches (or no longer matches) this one
		if (mIsPlayerTarget != isTargeted)
		{
			mIsPlayerTarget = isTargeted;
			foreach (MonoBehaviour mb in disableOnPlayerTarget) mb.enabled = !isTargeted;
			foreach (MonoBehaviour mb in enableOnPlayerTarget) mb.enabled = isTargeted;
		}
		return true;
	}
}