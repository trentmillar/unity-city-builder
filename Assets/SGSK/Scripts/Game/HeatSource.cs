using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Heat Source")]
public class HeatSource : MonoBehaviour
{
	static public List<HeatSource> mList = new List<HeatSource>();

	Transform mTrans;

	/// <summary>
	/// Register / unregister this heat source with the global list.
	/// </summary>

	void Awake () { mList.Add(this); }
	void OnDestroy () { mList.Remove(this); }

	/// <summary>
	/// Find the target reticle, if there is one.
	/// </summary>

	void Start () { mTrans = transform; }

	/// <summary>
	/// Find the most optimal heat source ahead of the specified point.
	/// </summary>

	static public HeatSource Find (Vector3 pos, Vector3 dir, float maxRange, float maxAngle)
	{
		HeatSource bestSource = null;
		float bestValue = 0f;

		foreach (HeatSource heat in mList)
		{
			if (heat == null || heat.mTrans == null) continue;

			// If the heat source is too far, move on to the next
			Vector3 heatDir = heat.mTrans.position - pos;
			float distance = heatDir.magnitude;
			if (distance > maxRange || distance < 0.01f) continue;

			// Normalize the distance and determine the dot product
			if (distance != 0f) heatDir *= 1.0f / distance;

			// Calculate the angle
			float angle = Vector3.Angle(dir, heatDir);

			// The angle must be within the sensor threshold
			if (angle < maxAngle)
			{
				// Calculate the value of this target
				float val = (maxRange - distance) / maxRange * (1f - angle / maxAngle);

				if (val > bestValue)
				{
					bestValue = val;
					bestSource = heat;
				}
			}
		}
		return bestSource;
	}
}