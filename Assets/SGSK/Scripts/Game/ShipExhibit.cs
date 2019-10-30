using UnityEngine;

[AddComponentMenu("Game/Ship Exhibit")]
public class ShipExhibit : MonoBehaviour
{
	Transform mTrans;
	Vector3 mOffset;
	Vector2 mTime;

	void Start ()
	{
		mTrans = transform;
		mOffset.x = Random.Range(0.0f, 10.0f);
		mOffset.y = Random.Range(0.0f, 10.0f);
	}

	void Update ()
	{
		float delta = Time.deltaTime;

		mTime.x += delta * 0.3326f;
		mTime.y += delta * 0.765f;

		Vector3 rot = new Vector3(
			Mathf.Sin(mOffset.x + mTime.x) * 0.75f, 0f,
			Mathf.Sin(mOffset.y + mTime.y) * 1.5f);

		mTrans.localRotation = Quaternion.Euler(rot);
	}
}