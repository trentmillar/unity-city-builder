using UnityEngine;

[AddComponentMenu("Game/HUD Stats")]
public class HUDStats : MonoBehaviour
{
	static public string text = "";

	public TextMesh foreground;
	public TextMesh background;

	void FixedUpdate ()
	{
		if (foreground != null) foreground.text = text;
		if (background != null) background.text = text;
	}
}