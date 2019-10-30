using UnityEngine;

[AddComponentMenu("Game/Player")]
[RequireComponent(typeof(Spaceship))]
public class Player : MonoBehaviour
{
	static public Transform trans;
	static public GameUnit unit;
	static public Spaceship ship;
	static public DamageShield shield;
	static public Transform target;

	// This simply allows the player class to be disabled via inspector
	void OnEnable () { }

	void Awake ()
	{
		if (enabled && NetworkManager.IsMine(this))
		{
			trans	= transform;
			unit	= GetComponent<GameUnit>();
			ship	= GetComponent<Spaceship>();
			shield	= GetComponentInChildren<DamageShield>();

			ship.name = "Player";
		}
		else
		{
			Destroy(this);
		}
	}

	void Update ()
	{
		// TODO: This is temporary. In the future it should allow opening up the UI, but it won't be in this class.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Debug.Log("UI Mode");
			ControlScheme.uiMode = true;
		}
		else if (Input.anyKeyDown) ControlScheme.uiMode = false;
	}
}