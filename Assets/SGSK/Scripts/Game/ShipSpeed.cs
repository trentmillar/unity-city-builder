using UnityEngine;

[AddComponentMenu("Game/Ship Speed")]
public class ShipSpeed : MonoBehaviour
{
	Spaceship mControl;

	void Start ()
	{
		if (!NetworkManager.IsMine(this)) Destroy(this);
		else mControl = Tools.FindInParents<Spaceship>(transform);
	}

	void Update ()
	{
		HUDStats.text = Mathf.RoundToInt(mControl.currentSpeed).ToString();
	}
}