using UnityEngine;

[RequireComponent(typeof(ImprovedTrail))]
[AddComponentMenu("Game/Ship Thruster Trail")]
public class ShipThrusterTrail : MonoBehaviour
{
	ImprovedTrail mTrail;
	Spaceship mControl;

	void Start ()
	{
		mTrail = GetComponent<ImprovedTrail>();
		mControl = Tools.FindInParents<Spaceship>(transform);
	}

	void Update ()
	{
		float alpha = (mControl.currentSpeed / mControl.maximumSpeed);
		mTrail.alpha = Mathf.Max(0f, alpha * mControl.navigation * Mathf.Max(0f, mControl.movement.z));
	}
}