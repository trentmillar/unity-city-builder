using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Network manager is a helper class for network-related functionality. Connect, send, receive, all of it
/// and more has been made more accessible to the developer.
/// </summary>

[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Common/Network Manager")]
public class NetworkManager : MonoBehaviour
{
	// We only want one NetworkManager in the scene
	static NetworkManager mInstance = null;

	// Counter used to give players unique IDs
	static int mCounter = 0;

	// Player's name
	static string mPlayerName = "<Anonymous>";

	// The player list is available on both the server and the client.
	public class Player
	{
		public int id;
		public string name;
		public NetworkPlayer net;
		public bool isReady = false;
	}

	// List of all players currently in the scene
	static List<Player> mPlayers = new List<Player>();

	// Used by the list of recently destroyed items
	class DestroyedEntry
	{
		public float time;
		public NetworkViewID id;
	}

	// Objects that have been destroyed recently (cached for a little bit in order to ignore certain calls)
	static List<DestroyedEntry> mDestroyed = new List<DestroyedEntry>();

	// Level counter used to prefix messages
	static int mLevelCounter = 0;

	// Network-unique identifier of this player. The server's ID is '0', player IDs increment from 1 onward.
	static int mId = 0;

	/// <summary>
	/// It should be possible to retrieve the unique identifier.
	/// </summary>

	static public int uniqueID { get { return mId; } }

	// Whether this class will remain in the scene even after loading another
	public bool isPersistent = false;

	/// <summary>
	/// Scene that will be playing when there is a level being loaded.
	/// </summary>

	public string loadingScreenLevel = "";

	/// <summary>
	/// Scene that will be loaded after the client gets disconnected.
	/// </summary>

	public string disconnectedLevel = "";

	// Network Manager must have a network view
	NetworkView mView;

	// When a new player joins, all data must be force-sent the next sync, regardless of whether it has changed.
	// 0 = No force sync
	// 1 = Force sync
	// 2 = Sync completed, return to '0' next update.
	int mForceSync = 1;

	/// <summary>
	/// Ensure that the NetworkView is watching this object and isn't trying to sync anything.
	/// </summary>

	void Awake ()
	{
		if (mInstance == null)
		{
			mInstance = this;
			mView = GetObserver(this);

			if (mView == null)
			{
				Debug.LogError("Expected to find a NetworkView watching the NetworkManager, found none");
				Destroy(this);
			}
			else
			{
				mView.group = priorityChannel;
				if (isPersistent) DontDestroyOnLoad(gameObject);
			}
		}
		else
		{
			if (!isPersistent || !mInstance.isPersistent) Debug.LogWarning("Can only have one NetworkManager in the scene");
			if (isConnected) Network.Destroy(gameObject);
			else Destroy(gameObject);
		}
	}

	/// <summary>
	/// Disconnect before shutting down.
	/// </summary>

	void OnDestroy ()
	{
		if (mInstance == this)
		{
			Disconnect();
			mInstance = null;
		}
	}

	/// <summary>
	/// Curious hack. Testing it...
	/// </summary>

	void LateUpdate ()
	{
		if (mForceSync == 2)
		{
			mForceSync = 0;
		}
	}

#region Server Functions

	/// <summary>
	/// Server notification: the server is listening for incoming connections.
	/// </summary>

	void OnServerInitialized ()
	{
		Tools.Broadcast("OnNetworkStart");
	}

	/// <summary>
	/// Server notification: a player has connected.
	/// </summary>

	void OnPlayerConnected (NetworkPlayer player)
	{
		// Add a new player entry
		Player ent = null;

		// Find a usable ID
		while (ent == null)
		{
			int id = ++mCounter;
			bool found = false;

			foreach (Player p in mPlayers)
			{
				if (p.id == id)
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				// ID not found: create a new player entry with this ID
				ent = new Player();
				ent.id = id;
				mPlayers.Add(ent);
				break;
			}
		}

		// Create a new player entry
		ent.name = "<Anonymous>";
		ent.net = player;

		// Inform the player of their ID
		mView.RPC("ClientSetID", player, ent.id);

		// Notify everyone that this player has connected
		mView.RPC("ClientAddPlayer", RPCMode.Others, ent.id, ent.name, player);

		// Inform the player of the server's ID and name
		mView.RPC("ClientAddPlayer", player, mId, mPlayerName, Network.player);

		// Inform the player of other players
		foreach (Player pe in mPlayers)
		{
			if (pe != ent)
			{
				mView.RPC("ClientAddPlayer", player, pe.id, pe.name, pe.net);
			}
		}
	}

	/// <summary>
	/// Server notification: a player has disconnected.
	/// </summary>

	void OnPlayerDisconnected (NetworkPlayer player)
	{
		// Remove everything this player has created directly
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);

		foreach (Player p in mPlayers)
		{
			if (p.net == player)
			{
				// Remove this player from the list
				mPlayers.Remove(p);

				// Notify the connected players
				mView.RPC("ClientRemovePlayer", RPCMode.Others, p.id);
				return;
			}
		}
		Debug.LogWarning("Unknown player has disconnected (" + player.ipAddress + ":" + player.port + ")");
	}

	/// <summary>
	/// Do nothing, just keep an eye on sync messages so the manager knows when they happen.
	/// </summary>

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (mForceSync == 1)
		{
			mForceSync = 2;
		}
	}

#endregion

#region Client Functions
	
	/// <summary>
	/// Client notification: failed to establish a connection to the server.
	/// </summary>

	//void OnFailedToConnect (NetworkConnectionError error)
	//{
	//}

	/// <summary>
	/// Client notification: established connection with the server.
	/// </summary>

	//void OnConnectedToServer ()
	//{
	//}

	/// <summary>
	/// Client notification: disconnected from the server.
	/// </summary>

	void OnDisconnectedFromServer (NetworkDisconnection info)
	{
		mId = 0;
		mPlayers.Clear();
		mCounter = 0;

		// Load the "disconnected" notification level, if one was specified
		if (Application.isPlaying && !string.IsNullOrEmpty(disconnectedLevel))
		{
			StartCoroutine(LoadLevelAsync(disconnectedLevel));
		}
	}

#endregion

#region RPC Functions

	/// <summary>
	/// Client RPC: Set player ID -- the very first RPC message that should be sent to the client.
	/// </summary>

	[RPC]
	void ClientSetID (int myID)
	{
		if (Network.isClient)
		{
			// Inform everyone what this client's name is
			mView.RPC("GenericSetPlayerName", RPCMode.Others, mId = myID, mPlayerName);
		}
		else
		{
			Error("SetPlayerList is never supposed to be called on the server!");
		}
	}

	/// <summary>
	/// Client RPC: Add a new player to the game.
	/// </summary>

	[RPC]
	void ClientAddPlayer (int playerID, string playerName, NetworkPlayer netPlayer)
	{
		if (playerID != mId)
		{
			mForceSync = 1;

			foreach (Player pl in mPlayers)
			{
				if (pl.id == playerID)
				{
					Error("ID " + playerID + " already belongs to " + pl.name);
					return;
				}
			}

			Player p = new Player();
			p.id = playerID;
			p.name = playerName;
			p.net = netPlayer;
			mPlayers.Add(p);
		}
	}

	/// <summary>
	/// Client RPC: Notification of a player disconnecting. 
	/// </summary>

	[RPC]
	void ClientRemovePlayer (int id)
	{
		Player p = GetPlayerAndWarn(id);
		if (p != null) mPlayers.Remove(p);
	}

	/// <summary>
	/// Generic RPC: Name change notification.
	/// </summary>

	[RPC]
	void GenericSetPlayerName (int id, string val)
	{
		Player p = GetPlayerAndWarn(id);
		if (p != null) p.name = val;
	}

	/// <summary>
	/// Generic RPC call that loads the specified level.
	/// </summary>

	[RPC]
	void GenericLoadLevel (string levelName, int counter)
	{
		mLevelCounter = counter;

		// Don't send anything else on the game channel
		Network.SetSendingEnabled(gameChannel, false);

		// Stop receiving data until the level finishes loading
		Network.isMessageQueueRunning = false;

		// Update the level prefix ensuring that the old messages get discarded
		Network.SetLevelPrefix(mLevelCounter);

		// Load the specified level.
		StartCoroutine(LoadLevelAsync(levelName));
	}

	/// <summary>
	/// Coroutine for level loading.
	/// </summary>

	IEnumerator LoadLevelAsync (string levelName)
	{
		// Notify all objects that we are about to load a new level
		Tools.Broadcast("End");

		// Immediately load the loading screen level, if one was provided
		if (!string.IsNullOrEmpty(loadingScreenLevel)) Application.LoadLevel(loadingScreenLevel);

		// Load the level asynchronously, letting the loading screen play in the background
		AsyncOperation op = Application.LoadLevelAsync(levelName);
		while (!op.isDone) yield return null;

		if (isConnected)
		{
			// Allow receiving data again
			Network.isMessageQueueRunning = true;

			// Now that the level has finished loading we can start sending out data to clients again.
			Network.SetSendingEnabled(gameChannel, true);

			// Notify all objects that the network level has finished loading
			Tools.Broadcast("OnNetworkStart");
		}
	}

#endregion

#region Static Functions

	/// <summary>
	/// Game channel should be used for all game-related sync communication.
	/// </summary>

	static public int gameChannel { get { return 0; } }

	/// <summary>
	/// Priority channel is used by the NetworkManager.
	/// </summary>

	static public int priorityChannel { get { return 1; } }

	/// <summary>
	/// Chat channel should be used exclusively for chat.
	/// </summary>

	static public int chatChannel { get { return 2; } }

	/// <summary>
	/// Whether we're currently connected to the server (or we are the server).
	/// </summary>

	static public bool isConnected { get { return Network.peerType != NetworkPeerType.Disconnected; } }

	/// <summary>
	/// Whether the network manager has someone to send data to.
	/// </summary>

	static public bool isMultiplayer { get { return mPlayers.Count > 0; } }

	/// <summary>
	/// Curious hack... testing it.
	/// </summary>

	static public bool forceSync { get { return mInstance != null && mInstance.mForceSync != 0; } }

	/// <summary>
	/// Access to the list of other players.
	/// </summary>

	static public List<Player> players { get { return mPlayers; } }

	/// <summary>
	/// Changes the player's name.
	/// </summary>

	static public string playerName
	{
		get
		{
			return mPlayerName;
		}
		set
		{
			mPlayerName = value;

			if (isConnected)
			{
				mInstance.mView.RPC("GenericSetPlayerName", RPCMode.Others, mId, value);
			}
		}
	}

	/// <summary>
	/// Error logging function.
	/// </summary>

	static void Error (string text) { Debug.LogError(text); }

	/// <summary>
	/// In order to send and receive messages of any kind, a NetworkView must be present in the scene. It's a Unity limitation.
	/// </summary>

	static bool InstanceCheck ()
	{
		if (mInstance != null) return true;
		Debug.LogError("Every scene that will use networking must have a NetworkManager present");
		return false;
	}

	/// <summary>
	/// Check to see if an object has been destroyed recently.
	/// </summary>

	static public bool HasBeenDestroyed (NetworkView view) { return HasBeenDestroyed(view.viewID); }

	/// <summary>
	/// Check to see if an object has been destroyed recently.
	/// </summary>

	static public bool HasBeenDestroyed (NetworkViewID viewID)
	{
		foreach (DestroyedEntry de in mDestroyed)
		{
			if (de.id == viewID) return true;
		}
		return false;
	}

	/// <summary>
	/// Helper function that finds the player by ID.
	/// </summary>

	static public Player GetPlayer (int id)
	{
		foreach (Player p in mPlayers) if (p.id == id) return p;
		return null;
	}

	/// <summary>
	/// Helper function that finds the network player that owns the specified NetworkView.
	/// </summary>

	static public Player GetPlayer (NetworkView view)
	{
		if (view == null) return null;
		NetworkPlayer np = view.owner;
		foreach (Player p in mPlayers) if (p.net == np) return p;
		return null;
	}

	/// <summary>
	/// Internally-used version of GetPlayer() function that logs an error if the player was not found.
	/// </summary>

	static Player GetPlayerAndWarn (int id)
	{
		Player p = GetPlayer(id);
		if (p == null) Error("Player with the ID of " + id + " was not found!");
		return p;
	}

	/// <summary>
	/// Start hosting a server on the specified port.
	/// </summary>

	static public bool Host (int port) { return Host(32, port, false); }

	/// <summary>
	/// Start hosting a server on the specified port.
	/// </summary>

	static public bool Host (int maxClients, int port, bool useNat)
	{
		if (!InstanceCheck()) return false;
		Disconnect();
		return (Network.InitializeServer(maxClients, port, useNat) == NetworkConnectionError.NoError);
	}

	/// <summary>
	/// Connect to the specified address.
	/// </summary>

	static public bool Connect (string address, int port)
	{
		if (!InstanceCheck()) return false;
		Disconnect();
		return (Network.Connect(address, port) == NetworkConnectionError.NoError);
	}

	/// <summary>
	/// Disconnect from the server or all clients.
	/// </summary>

	static public void Disconnect ()
	{
		if (mInstance != null)
		{
			if (isConnected) Network.Disconnect();
			mId = 0;
		}
		mPlayers.Clear();
		mCounter = 0;
	}

	/// <summary>
	/// Network.Instantiate equivalent, but works even if offline.
	/// </summary>

	static public GameObject RemoteInstantiate (GameObject prefab, Vector3 pos, Quaternion rot)
	{
		if (prefab == null) return null;
		return (isConnected) ?
			Network.Instantiate(prefab, pos, rot, gameChannel) as GameObject :
			Instantiate(prefab, pos, rot) as GameObject;
	}

	/// <summary>
	/// Network.Instantiate equivalent, but works even if offline.
	/// </summary>

	static public GameObject RemoteInstantiate (GameObject prefab, Vector3 pos, Quaternion rot, int group)
	{
		if (prefab == null) return null;
		return (isConnected) ?
			Network.Instantiate(prefab, pos, rot, group) as GameObject :
			Instantiate(prefab, pos, rot) as GameObject;
	}

	/// <summary>
	/// Network.Destroy equivalent, but works even if offline.
	/// </summary>

	static public void RemoteDestroy (GameObject obj)
	{
		if (obj == null) return;
		if (isConnected)
		{
			NetworkView view = obj.networkView;

			if (view != null)
			{
				// Ignore entries that have already been destroyed
				foreach (DestroyedEntry de in mDestroyed) if (de.id == view.viewID) return;

				// Add a local entry right away so future RemoteDestroy calls get ignored
				DestroyedEntry ent = new DestroyedEntry();
				ent.time = Time.time;
				ent.id = view.viewID;
				mDestroyed.Add(ent);

				// Notify all players that this viewID is being destroyed
				mInstance.mView.RPC("OnRemoteDestroy", RPCMode.All, view.viewID);

				// Destroy the object
				Network.Destroy(view.viewID);
			}
			else
			{
				Debug.LogError(Tools.GetHierarchy(obj) + " has no NetworkView on it!");
			}
		}
		else Destroy(obj);
	}

	/// <summary>
	/// RPC notification of a network view ID being destroyed.
	/// Used to add the destroyed object to all the clients' lists.
	/// </summary>

	[RPC] void OnRemoteDestroy (NetworkViewID viewID)
	{
		bool alreadyDestroyed = false;

		// Check to see if this viewID has already been destroyed
		foreach (DestroyedEntry de in mDestroyed)
		{
			if (de.id == viewID)
			{
				// Match (happens on the same PC that sent out the request)
				alreadyDestroyed = true;
				break;
			}
		}

		float time = Time.time;

		// Remove stale entries
		for (int i = mDestroyed.Count; i > 0; )
		{
			DestroyedEntry de = mDestroyed[--i];
			if (de.time + 5f < time) mDestroyed.RemoveAt(i);
		}

		// Add a new entry
		if (!alreadyDestroyed)
		{
			DestroyedEntry de = new DestroyedEntry();
			de.time = time;
			de.id = viewID;
			mDestroyed.Add(de);
		}

		// Remove all other RPC calls stored on this view
		Network.RemoveRPCs(viewID);
	}

	/// <summary>
	/// Load a level remotely, or if not connected -- locally.
	/// </summary>

	static public void LoadLevel (string levelName)
	{
		if (isConnected)
		{
			// Network manager RPCs
			Network.RemoveRPCsInGroup(priorityChannel);

			// Chat channel RPCs
			Network.RemoveRPCsInGroup(chatChannel);

			// Game channel RPCs
			Network.RemoveRPCsInGroup(gameChannel);

			// Start loading the level
			mInstance.mView.RPC("GenericLoadLevel", RPCMode.AllBuffered, levelName, mLevelCounter+1);
		}
		else
		{
			// Load the specified level.
			mInstance.StartCoroutine(mInstance.LoadLevelAsync(levelName));
		}
	}

	/// <summary>
	/// Finds and returns the network view that controls the specified component.
	/// </summary>

	static public NetworkView GetNetworkView (Component comp)
	{
		Transform t = comp.transform;

		while (t != null)
		{
			NetworkView view = t.GetComponent<NetworkView>();
			if (view != null) return view;
			t = t.parent;
		}
		return null;
	}

	/// <summary>
	/// Whether the specified component is owned by this computer.
	/// </summary>

	static public bool IsMine (Component comp)
	{
		if (!isConnected) return true;
		NetworkView view = GetNetworkView(comp);
		return (view == null || view.isMine);
	}

	/// <summary>
	/// Helper function that returns the observer for the specified component.
	/// </summary>

	static public NetworkView GetObserver (Component comp)
	{
		NetworkView[] views = comp.GetComponents<NetworkView>();

		foreach (NetworkView view in views)
		{
			if (view.observed == comp && view.stateSynchronization != NetworkStateSynchronization.Off)
			{
				return view;
			}
		}
		return null;
	}

#endregion
}