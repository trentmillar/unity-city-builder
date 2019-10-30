using UnityEngine;

[AddComponentMenu("Game/Conditional Instantiate")]
public class ConditionalInstantiate : MonoBehaviour
{
	public enum Condition
	{
		None,
		IfOwned,
		IfNotOwned,
		IfPlayer,
		IfNotPlayer,
	}

	public GameObject prefab;
	public Condition condition = Condition.None;
	public float scale = 1f;

	GameObject mGO;

	/// <summary>
	/// Instantiated game object.
	/// </summary>

	public GameObject instantiatedObject { get { return mGO; } }

	/// <summary>
	/// Instantiate the prefab object.
	/// </summary>

	void Update()
	{
		// Wait for the player to be created
		if (mGO != null || Player.unit == null) return;

		if (prefab != null)
		{
			GameUnit gu = Tools.FindInParents<GameUnit>(transform);
			bool isPlayer = (gu != null && gu == Player.unit);
			bool owned = NetworkManager.IsMine(this);

			if ( condition == Condition.None ||
				(condition == Condition.IfOwned && owned) ||
				(condition == Condition.IfNotOwned && !owned) ||
				(condition == Condition.IfPlayer && isPlayer) ||
				(condition == Condition.IfNotPlayer && !isPlayer))
			{
				Transform trans = transform;
				mGO = Instantiate(prefab, trans.position, Quaternion.identity) as GameObject;

				Transform ct = mGO.transform;
				Vector3 retScale = ct.localScale;
				Vector3 myScale = trans.localScale;

				float avg = (myScale.x + myScale.y + myScale.z) / 3f;
				myScale.x = avg;
				myScale.y = avg;
				myScale.z = avg;

				ct.localScale = new Vector3(
					scale * retScale.x * myScale.x,
					scale * retScale.y * myScale.y,
					scale * retScale.z * myScale.z);

				OnInstantiated(mGO);
			}
		}
		enabled = false;
	}

	/// <summary>
	/// Triggered when an object was successfully instantiated.
	/// </summary>

	protected virtual void OnInstantiated (GameObject go) {}
}