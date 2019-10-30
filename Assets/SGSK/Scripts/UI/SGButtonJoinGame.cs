using UnityEngine;

[AddComponentMenu("UI/Button: Join Game")]
public class SGButtonJoinGame : MonoBehaviour
{
	public SGInput addressField;

	void OnClick ()
	{
		if (addressField != null)
		{
			NetworkManager.Connect(addressField.text, 7777);
		}
	}
}