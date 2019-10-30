using UnityEngine;

[RequireComponent(typeof(NetworkView))]
[AddComponentMenu("Game/Power Generator")]
public class PowerGenerator : MonoBehaviour
{
	public float powerOutput	= 20f;		// Power generated per second
	public float currentReserve	= 100f;		// Power reserves
	public float maxReserve		= 100f;		// Maximum reserve power

	NetworkView mView;

	/// <summary>
	/// Current power in percent.
	/// </summary>

	public float powerPercent { get { return Mathf.Clamp01(currentReserve / maxReserve); } }

	/// <summary>
	/// Power effectiveness curve. Key = amount of power left (percentage). Value = effectiveness of the output power.
	/// </summary>

	public AnimationCurve effectivenessCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });

	/// <summary>
	/// Effective level of power -- directly affects everything powered by this generator.
	/// </summary>

	public float effectiveness
	{
		get
		{
			float f = currentReserve / maxReserve;
			return effectivenessCurve.Evaluate(f);
		}
	}

	/// <summary>
	/// We need a NetworkView to send RPC calls.
	/// </summary>

	void Start ()
	{
		mView = networkView;
	}

	/// <summary>
	/// Drain the specified amount of power from the power generator.
	/// Returns the amount of power that still needs to be gathered from other sources.
	/// </summary>

	public float DrainPower (float amount)
	{
		float eff = effectiveness;
		currentReserve -= amount;
		float leftover = Mathf.Max(0f, -currentReserve);
		currentReserve = Mathf.Clamp(currentReserve, 0f, maxReserve);

		if (NetworkManager.isConnected)
		{
			mView.RPC("SetPowerLevel", RPCMode.Others, currentReserve);
		}
		return Mathf.Lerp(amount, leftover, eff);
	}

	/// <summary>
	/// Recover the power over time.
	/// </summary>

	void Update ()
	{
		currentReserve = Mathf.Clamp(currentReserve + powerOutput * Time.deltaTime, 0f, maxReserve);
	}

	/// <summary>
	/// RPC call that sets the power generator's current power level across the network.
	/// </summary>

	[RPC] void SetPowerLevel (float reserve)
	{
		currentReserve = reserve;
	}
}