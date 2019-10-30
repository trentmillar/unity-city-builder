using UnityEngine;

[AddComponentMenu("Game/Condition")]
public class Condition : MonoBehaviour
{
	public enum Check
	{
		IsOwned,
		IsNotOwned,
		IsPlayer,
		IsNotPlayer,
	}

	public Check condition = Check.IsPlayer;

	Transform[] mTransforms;

	/// <summary>
	/// Start by deactivating all children.
	/// </summary>

	void Awake ()
	{
		if (enabled)
		{
			Transform trans = transform;
			mTransforms = GetComponentsInChildren<Transform>();

			foreach (Transform t in mTransforms)
			{
				if (t != trans) t.gameObject.active = false;
			}
		}
	}

	/// <summary>
	/// Check to see if conditions match -- if so, activate all children.
	/// </summary>

	void Start ()
	{
		bool isMine = NetworkManager.IsMine(this);

		if ((condition == Check.IsOwned && isMine) ||
			(condition == Check.IsNotOwned && !isMine))
		{
			foreach (Transform t in mTransforms) t.gameObject.active = true;
			Destroy(this);
		}
		else Update();
	}

	/// <summary>
	/// Wait for the player information to become available, then check the last set of conditions.
	/// </summary>

	void Update ()
	{
		if (Player.unit == null) return;

		GameUnit gu = Tools.FindInParents<GameUnit>(transform);
		bool isPlayer = (Player.unit == gu);

		if ((condition == Check.IsPlayer && isPlayer) ||
			(condition == Check.IsNotPlayer && !isPlayer))
		{
			foreach (Transform t in mTransforms) t.gameObject.active = true;
		}
		Destroy(this);
	}
}