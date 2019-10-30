using UnityEngine;

[AddComponentMenu("Game/Faction")]
[RequireComponent(typeof(NetworkView))]
public class GameFaction : MonoBehaviour
{
	public int factionID = 0;

	protected NetworkView mView;
	bool mOwnedByMe = true;

	/// <summary>
	/// Whether the network view is owned by this player.
	/// </summary>

	public bool ownedByMe { get { return mOwnedByMe; } }

	/// <summary>
	/// Initialize the faction.
	/// </summary>

	void Start ()
	{
		mView = networkView;
		mOwnedByMe = NetworkManager.IsMine(this);
		OnStart();
	}

	/// <summary>
	/// Set the faction ID.
	/// </summary>

	public void SetFaction (int faction)
	{
		if (factionID != faction)
		{
			mView.RPC("OnSetFaction", RPCMode.AllBuffered, faction);
			factionID = faction;
		}
	}

	[RPC] void OnSetFaction (int faction)
	{
		GameFaction[] factions = GetComponentsInChildren<GameFaction>();
		foreach (GameFaction fc in factions) fc.factionID = faction;
	}

	/// <summary>
	/// Any custom desired functionality.
	/// </summary>

	protected virtual void OnStart () { }
}