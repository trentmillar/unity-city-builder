using UnityEngine;

[RequireComponent(typeof(SGSprite))]
[AddComponentMenu("UI/Mouse Drag Indicator")]
public class SGMouseDragIndicator : MonoBehaviour
{
	public float orthoSize = 400f;

	Transform mTrans;
	Vector3 mMouseClick;
	Vector3 mStart;
	Vector3 mDelta;
	SGSprite mSprite;

	/// <summary>
	/// We need a sprite to work with.
	/// </summary>

	void Start ()
	{
		mTrans = transform;
		mSprite = GetComponent<SGSprite>();
	}

	/// <summary>
	/// Convenience helper function that converts the specified main camera screen space coordinates into orthoSize-based camera's.
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>

	Vector3 ScreenToOrtho (Vector3 pos)
	{
		float width = Screen.width;
		float height = Screen.height;
		float aspect = width / height;

		pos.x = (pos.x - width * 0.5f) / width;
		pos.y = (pos.y - height * 0.5f) / height;
		pos.z = mTrans.localPosition.z;

		pos.x *= orthoSize * 2f * aspect;
		pos.y *= orthoSize * 2f;
		return pos;
	}

	/// <summary>
	/// Check input, stretch and reposition the widget.
	/// </summary>

	void Update ()
	{
		// If a mouse button is clicked, record the initial position
		if (Input.GetMouseButtonDown(0))
		{
			mMouseClick = Input.mousePosition;
			mStart = ScreenToOrtho(mMouseClick);
			mDelta = Vector3.zero;
		}

		float alpha = 0f;

		// If a mouse button is held we want to reposition the widget
		if (Input.GetMouseButton(0))
		{
			// Using the axes instead of Input.mousePosition works even if the mouse is hidden
			mDelta.x += Input.GetAxis("Mouse X");
			mDelta.y += Input.GetAxis("Mouse Y");

			float width = Screen.width;
			float height = Screen.height;
			float aspect = width / height;

			// Convert to the orthographic camera's coordinates
			Vector3 dir = new Vector3(mDelta.x / (width * 0.5f), mDelta.y / (height * 0.5f), 0f);
			dir.x *= orthoSize * aspect;
			dir.y *= orthoSize;
			float mag = dir.magnitude;

			// Only make visible after it has been dragged more than 50 pixels on a screen that' orthoSize*2 pixels tall
			float factor = Mathf.Clamp01(Mathf.Max(0f, mag - 50f) / 50f);
			alpha = Mathf.Lerp(0f, 0.15f, factor);

			// If the widget is visible there is more work to be done
			if (alpha > 0f)
			{
				// Reposition the widget to be between the start position and the current one
				mTrans.localPosition = mStart + dir * 0.5f;

				// Figure out the desired rotation
				dir.Normalize();
				float deg = Mathf.Rad2Deg * Mathf.Asin(dir.x);
				mTrans.localRotation = Quaternion.Euler(0f, 0f, 180f + ((dir.y < 0f) ? deg : 180f - deg));

				// Scale the widget to stretch from the start position to the current one
				Vector3 scale = mTrans.localScale;
				scale.y = mag;
				mTrans.localScale = scale;
			}
		}

		// Interpolation for smoother results
		mSprite.color.a = Mathf.Lerp(mSprite.color.a, alpha, Time.deltaTime * 10f);
		mSprite.enabled = mSprite.color.a > 0.01f;
	}
}