using UnityEngine;

[AddComponentMenu("UI/Button: Host Game")]
public class SGButtonHostGame : MonoBehaviour
{
	public string levelToLoad = "Game";

	void OnClick ()
	{
		NetworkManager.Host(7777);
		NetworkManager.LoadLevel(levelToLoad);
	}
}