using UnityEngine;

[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Common/Network Debugger")]
public class NetworkDebugger : MonoBehaviour
{
	void OnServerInitialized ()
	{
		Debug.Log("Server started");
	}

	void OnPlayerConnected (NetworkPlayer player)
	{
		Debug.Log("Player has connected (" + player.ipAddress + ":" + player.port + ")");
	}

	void OnPlayerDisconnected (NetworkPlayer player)
	{
		Debug.Log("Player has disconnected (" + player.ipAddress + ":" + player.port + ")");
	}

	void OnFailedToConnect (NetworkConnectionError error)
	{
		Debug.Log("Failed to connect: " + error.ToString());
	}

	void OnConnectedToServer ()
	{
		Debug.Log("Connection established.");
	}

	void OnDisconnectedFromServer (NetworkDisconnection info)
	{
		Debug.Log("Disconnected from the server: " + info.ToString());
	}

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			NetworkManager.playerName = "Pedro";
		}
		else if (Input.GetKeyDown(KeyCode.K))
		{
			NetworkManager.playerName = "Karl";
		}
		else if (Input.GetKeyDown(KeyCode.O))
		{
			NetworkManager.playerName = "Olga";
		}
	}
}
