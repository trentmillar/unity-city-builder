using UnityEngine;

[AddComponentMenu("Game/Health Bar")]
public class HealthBar : ConditionalInstantiate
{
	public Vector2 range = new Vector2(35f, 300f);

	override protected void OnInstantiated (GameObject go)
	{
		GameUnit unit = Tools.FindInParents<GameUnit>(transform);

		if (unit != null)
		{
			SGHealthBar hb = go.GetComponentInChildren<SGHealthBar>();
			if (hb != null) hb.unit = unit;
		}

		PowerGenerator generator = Tools.FindInParents<PowerGenerator>(transform);

		if (generator != null)
		{
			SGPowerBar pb = go.GetComponentInChildren<SGPowerBar>();
			if (pb != null) pb.generator = generator;
		}

		SGTarget ut = go.GetComponent<SGTarget>();

		if (ut != null)
		{
			ut.target = transform;
			ut.range = range;
		}
	}
}