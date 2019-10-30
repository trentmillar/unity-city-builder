using UnityEngine;

/// <summary>
/// Special class created as a feature request. It facilitates creation of games similar to the
/// space combat of Star Wars: The Old Republic (and from what I understand Star Fox).
/// </summary>

[AddComponentMenu("Game/Arcade Movement")]
public class ArcadeMovement : MonoBehaviour
{
	public float screenArea = 0.4f;
	public float responsiveness = 10f;
	public float turnSensitivity = 10f;
	public Vector2 movementDistance = Vector2.one;
	public Vector3 turnDegrees = new Vector3(30f, 30f, 30f);

	Transform mTrans;
	Vector2 mInput;
	Vector2 mTurn;

	void Start ()
	{
		mTrans = transform;
	}

	void Update ()
	{
		Vector3 pos = Input.mousePosition;

		float x = -Mathf.Clamp((Screen.width  * 0.5f - pos.x) / (Screen.width  * screenArea), -1f, 1f);
		float y = -Mathf.Clamp((Screen.height * 0.5f - pos.y) / (Screen.height * screenArea), -1f, 1f);

		Vector2 vec = new Vector2(x, y);
		float mag = vec.magnitude;
		if (mag > 1f) vec *= 1.0f / mag;

		mInput = Vector2.Lerp(mInput, vec, Mathf.Clamp01(Time.deltaTime * responsiveness));
		mTurn = Vector2.Lerp(mTurn, vec - mInput, Mathf.Clamp01(Time.deltaTime * turnSensitivity));

		mTrans.localPosition = new Vector3(mInput.x * movementDistance.x, mInput.y * movementDistance.y, 0f);
		mTrans.localRotation = Quaternion.Euler(-mTurn.y * turnDegrees.x, mTurn.x * turnDegrees.y, -mTurn.x * turnDegrees.z);
	}
}