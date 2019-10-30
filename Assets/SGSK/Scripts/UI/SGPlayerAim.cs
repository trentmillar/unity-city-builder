using UnityEngine;

/// <summary>
/// This script is meant to be attached to a widget that's a child of the HUD camera. It will
/// reposition this widget to always follow a point 100 units in front of the player's ship.
/// </summary>

[AddComponentMenu("UI/Player Aim")]
[RequireComponent(typeof(SGWidget))]
public class SGPlayerAim : MonoBehaviour
{
	public Camera hudCamera;
	public float verticalOffset = 0f;

	Camera mMain;
	Transform mTrans;
	SGWidget mWidget;
	float mDist;
	float mAlpha;

	/// <summary>
	/// Find all the required components.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		mMain = Camera.main;
		mWidget = GetComponent<SGWidget>();
		mAlpha = mWidget.color.a;
		
		// Find the camera automatically
		if (hudCamera == null) hudCamera = HUDCamera.cam;
		if (hudCamera == null) hudCamera = Tools.FindInParents<Camera>(mTrans);

		if (hudCamera == null)
		{
			Debug.LogError(Tools.GetHierarchy(gameObject) + " needs a camera to work with");
			Destroy(this);
		}
		else if (mMain == hudCamera)
		{
			Debug.LogWarning("HUD camera and main cameras are the same. This script will do nothing.");
			Destroy(this);
		}
		else
		{
			// Remember the initial distance
			mDist = hudCamera.WorldToViewportPoint(mTrans.position).z;
		}
	}

	/// <summary>
	/// Convert from main camera to view space, them from view space to the HUD camera's coordinates.
	/// </summary>

	void LateUpdate ()
	{
		Color c = mWidget.color;
		Transform pt = Player.trans;

		if (pt != null)
		{
			// Tracked target is 100 units in front of the player
			Vector3 target = pt.position + pt.rotation * (Vector3.forward * 100f);

			// Convert to view space
			Vector3 vt = mMain.WorldToViewportPoint(target);

			if (vt.z < 0f)
			{
				// The target is behind the screen -- make it invisible
				c.a = 0f;
			}
			else
			{
				// If the ship is damaged, don't show the widget
				c.a = Player.ship.navigation * mAlpha;

				// Clamp to the edges of the screen
				vt.x = Mathf.Clamp(vt.x, 0f, 1f);
				vt.y = Mathf.Clamp(vt.y + verticalOffset, 0f, 1f);

				// Z should stay fixed so the widget always appears the same size
				vt.z = mDist;

				// Convert back to world
				mTrans.position = hudCamera.ViewportToWorldPoint(vt);
			}
		}
		else
		{
			// The player is not available
			c.a = 0f;
		}
		mWidget.color = c;
	}
}