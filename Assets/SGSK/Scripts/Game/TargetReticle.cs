using UnityEngine;

[AddComponentMenu("Game/Target Reticle")]
public class TargetReticle : ConditionalInstantiate
{
	public Vector2 range = new Vector2(35f, 300f);

	override protected void OnInstantiated (GameObject go)
	{
		SGTarget target = go.GetComponent<SGTarget>();

		if (target != null)
		{
			target.target = transform;
			target.range = range;
		}
	}
}