using UnityEngine;

[AddComponentMenu("Common/Control Scheme")]
public class ControlScheme : MonoBehaviour
{
	public enum InputSource
	{
		MouseKeyboard,
		Controller,
		Touchpad,
	}

	static ControlScheme mInstance = null;
	static InputSource mInput = InputSource.Controller;
	static bool mUIMode = false;

	/// <summary>
	/// Toggle the UI mode on/off. Set this to 'true' when the UI is open, and 'false' when it's closed.
	/// This way game logic for such events such as firing the weapon can check to see "is UI mode on?",
	/// and if it is -- ignore certain events.
	/// </summary>

	public static bool uiMode { get { return mUIMode; } set { mUIMode = value; } }

	/// <summary>
	/// Current control scheme.
	/// </summary>

	public static InputSource current
	{
		get
		{
			if (mInstance == null)
			{
				GameObject go = new GameObject("_ControlScheme");
				DontDestroyOnLoad(go);
				mInstance = go.AddComponent<ControlScheme>();
			}
			return mInput;
		}
	}

	/// <summary>
	/// Automatically set input to 'touchpad' on iOS and Android devices.
	/// </summary>

	void Awake ()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			mInput = InputSource.Touchpad;
		}
	}

	/// <summary>
	/// Switch between Mouse+Keyboard and Joystick input based on what was used last.
	/// </summary>

	void Update ()
	{
		if (mInput != InputSource.Touchpad)
		{
			if (Input.GetKeyDown(KeyCode.JoystickButton0) ||
				Input.GetKeyDown(KeyCode.JoystickButton1) ||
				Input.GetKeyDown(KeyCode.JoystickButton2) ||
				Input.GetKeyDown(KeyCode.JoystickButton3) ||
				Input.GetKeyDown(KeyCode.JoystickButton4) ||
				Input.GetKeyDown(KeyCode.JoystickButton5))
			{
				mInput = InputSource.Controller;
			}
			else if (Input.anyKeyDown)
			{
				mInput = InputSource.MouseKeyboard;
			}
		}
	}
}