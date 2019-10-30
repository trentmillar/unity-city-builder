using UnityEngine;

[AddComponentMenu("Game/Ship Debug")]
public class ShipDebug : MonoBehaviour
{
	Spaceship mSc;

	void Start ()
	{
		if (NetworkManager.IsMine(this)) mSc = Tools.FindInParents<Spaceship>(transform);
		else Destroy(this);
	}

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			mSc.maximumSpeed = 100f;
			mSc.mainThruster = 0.5f;
			mSc.maneuveringThrusters = 0f;
			mSc.turnThrusters = 0.25f;
			mSc.inertiaDampeners = 0f;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			mSc.maximumSpeed = 150f;
			mSc.mainThruster = 1f;
			mSc.maneuveringThrusters = 1f;
			mSc.turnThrusters = 1f;
			mSc.inertiaDampeners = 1f;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			mSc.maximumSpeed = 200f;
			mSc.mainThruster = 2f;
			mSc.maneuveringThrusters = 2f;
			mSc.turnThrusters = 1.5f;
			mSc.inertiaDampeners = 2f;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			mSc.maximumSpeed = 250f;
			mSc.mainThruster = 3f;
			mSc.maneuveringThrusters = 3f;
			mSc.turnThrusters = 2f;
			mSc.inertiaDampeners = 3f;
		}
	}

	/*void OnGUI ()
	{
		float f = mSc.power * 100f;
		UI.DrawLabel(Mathf.RoundToInt(mRb.velocity.magnitude * 3.6f) + " km/h", Config.instance.infoStyle, 1f);
		UI.DrawLabel(Mathf.RoundToInt(mSc.mainThruster * f) + "% thrusters", Config.instance.infoStyle, 1f);
		UI.DrawLabel(Mathf.RoundToInt(mSc.maneuveringThrusters * f) + "% maneuvering", Config.instance.infoStyle, 1f);
		UI.DrawLabel(Mathf.RoundToInt(mSc.turnThrusters * f) + "% turning", Config.instance.infoStyle, 1f);
		UI.DrawLabel(Mathf.RoundToInt(mSc.inertiaDampeners * f) + "% dampeners", Config.instance.infoStyle, 1f);
		UI.DrawLabel(mSc.movement.ToString(), Config.instance.infoStyle, 1f);
	}*/
}