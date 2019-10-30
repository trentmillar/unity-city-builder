using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Spaceship))]
[AddComponentMenu("Game/Spaceship Controller")]
public class SpaceshipController : MonoBehaviour
{
	public enum InputType
	{
		Mouse,
		Controller,
		Touchpad,
	}

	public bool movement = true;
	public bool weapons = true;

	Spaceship mShip;
	Vector3 mDelta;
	int mSteeringTouch = -1;
	float mNextPrimary = 0f;
	float mNextSecondary = 0f;

	List<Weapon> mPrimary = new List<Weapon>();
	List<Weapon> mSecondary = new List<Weapon>();

	/// <summary>
	/// Determine whether we own this ship, input type, and ensure we have a network observer.
	/// </summary>
	
	void Start()
	{
		// If we don't control the ship, this script isn't needed
		if (NetworkManager.isConnected && !NetworkManager.IsMine(this))
		{
			Destroy(this);
			return;
		}

		// We need to know the ship we're controlling.
		mShip = GetComponent<Spaceship>();

		// Find all weapons
		Weapon[] weapons = GetComponentsInChildren<Weapon>();
		
		foreach (Weapon w in weapons)
		{
			if (w.group == Weapon.Group.Primary) mPrimary.Add(w);
			if (w.group == Weapon.Group.Secondary) mSecondary.Add(w);
		}
	}
	
	/// <summary>
	/// Process input if we're controlling this ship.
	/// </summary>

	void Update()
	{
		Vector3 turn = mShip.turningInput;
		Vector3 move = mShip.moveInput;

		if (ControlScheme.uiMode)
		{
			// UI mode -- ignore input
			mDelta = Vector2.zero;
		}
		else
		{
			// Get the movement axes
			if (ControlScheme.current == ControlScheme.InputSource.Touchpad)
			{
				Vector3 touchMove = Vector3.zero;
				Vector3 touchTurn = Vector3.zero;

				foreach (Touch touch in Input.touches)
				{
					// Ignore the touch used to steer
					if (mSteeringTouch == touch.fingerId)
					{
						if (touch.phase == TouchPhase.Moved)
						{
							mDelta.x += touch.deltaPosition.x;
							mDelta.y += touch.deltaPosition.y;
						}
						else if (touch.phase == TouchPhase.Ended ||
								 touch.phase == TouchPhase.Canceled)
						{
							mDelta = Vector2.zero;
							mSteeringTouch = -1;
						}
					}
					else if (touch.position.x < Screen.width * 0.25f)
					{
						// Bottom part of the screen
						if (touch.position.y < Screen.height * 0.25f)
						{
							// Bottom-left corner -- slow down
							touchMove.z = -1f;
						}
						else if (touch.position.y > Screen.height * 0.75f)
						{
							// Top-left corner -- accelerate
							touchMove.z = 1f;
						}
						else
						{
							// Left side -- roll left
							touchTurn.z = -1f;
						}
					}
					else if (touch.position.x > Screen.width * 0.75f)
					{
						if (touch.position.y < Screen.height * 0.25f)
						{
							// Bottom right corner -- shoot primary weapon
							if (weapons && mPrimary.Count > 0 && mNextPrimary < Time.time)
							{
								mNextPrimary = Fire(mPrimary);
							}
						}
						else if (touch.position.y > Screen.height * 0.75f)
						{
							// Top right corner -- shoot secondary weapon
							if (weapons && mSecondary.Count > 0 && mNextSecondary < Time.time)
							{
								mNextSecondary = Fire(mSecondary);
							}
						}
						else
						{
							// Right side -- roll right
							touchTurn.z = 1f;
						}
					}
					else if (touch.position.x > Screen.width * 0.25f &&
							 touch.position.x < Screen.width * 0.75f &&
							 touch.position.y > Screen.width * 0.25f &&
							 touch.position.y < Screen.width * 0.75f)
					{
						// Central part of the screen
						if (touch.phase == TouchPhase.Began)
						{
							mSteeringTouch = touch.fingerId;
						}
					}
				}

				float maxDist = 1.0f / Mathf.Min(Screen.width * 0.4f, Screen.height * 0.4f);

				touchTurn.x = Mathf.Clamp(mDelta.y * maxDist, -1f, 1f);
				touchTurn.y = Mathf.Clamp(mDelta.x * maxDist, -1f, 1f);

				move = touchMove;
				turn = touchTurn;
			}
			else
			{
				if (movement)
				{
					move.x = Input.GetAxis("Right");
					move.y = Input.GetAxis("Up");
					move.z = Input.GetAxis("Forward");

					// Record the roll rotation
					turn.z = Input.GetAxis("Roll");

					if (ControlScheme.current == ControlScheme.InputSource.Controller)
					{
						if (!Application.isEditor) Screen.lockCursor = false;

						// Joystick-based control
						turn.x = Input.GetAxis("Look Up");
						turn.y = Input.GetAxis("Look Right");

						mDelta = Vector2.zero;
					}
					else
					{
						if (!Application.isEditor) Screen.lockCursor = true;

						// Mouse-based control
						mDelta.x += Input.GetAxis("Mouse X");
						mDelta.y += Input.GetAxis("Mouse Y");

						float maxDist = 2.0f / Mathf.Min(Screen.width, Screen.height);

						turn.x = Mathf.Clamp(mDelta.y * maxDist, -1f, 1f);
						turn.y = Mathf.Clamp(mDelta.x * maxDist, -1f, 1f);
					}
				}

				// Fire the primary weapon
				if (weapons)
				{
					if (mPrimary.Count > 0 && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton4)))
					{
						if (mNextPrimary < Time.time)
						{
							mNextPrimary = Fire(mPrimary);
						}
					}

					// Fire the secondary weapon
					if (mSecondary.Count > 0 && (Input.GetMouseButton(1) || Input.GetKey(KeyCode.JoystickButton5)))
					{
						if (mNextSecondary < Time.time)
						{
							mNextSecondary = Fire(mSecondary);
						}
					}
				}
			}
		}

		// Backwards movement should act as an anchor and should negate all other movement
		if (move.z < 0f)
		{
			// Drain power while the "brake" button is held
			if (mShip.powerGenerator != null)
			{
				float mult = Time.deltaTime * 30f;
				move.z = -1.0f + mShip.powerGenerator.DrainPower(-move.z * mult) / mult;
			}

			move.x *= 1f + move.z;
			move.y *= 1f + move.z;
		}

		// Ensure the vector is unit length
		float mag = move.magnitude;
		if (mag > 1f) move *= 1.0f / mag;

		// Ensure the vector is unit length
		mag = turn.magnitude;

		if (mag > 1f)
		{
			turn *= 1.0f / mag;
			mag = 1f;
		}

		// Sensitivity tweak to make it easier to snipe-aim
		turn *= Tools.Ramp(mag, 0.35f);

		// Set the ship's values
		mShip.turningInput = turn;
		mShip.moveInput = move;
	}

	/// <summary>
	/// Helper function used above. Fires a weapon from the specified list and returns the next time we'll be able to fire.
	/// </summary>

	static float Fire (List<Weapon> list)
	{
		foreach (Weapon wp in list)
		{
			if (wp.canFire)
			{
				wp.Fire();
				return Time.time + wp.firedObject.firingFrequency / (float)list.Count;
			}
		}
		return Time.time;
	}
}