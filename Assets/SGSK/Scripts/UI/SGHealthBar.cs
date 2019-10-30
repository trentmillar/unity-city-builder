using UnityEngine;

/// <summary>
/// Extends the Progress Bar's functionality to add a color gradient based on unit's health.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("UI/Health Bar")]
[RequireComponent(typeof(SGSprite))]
public class SGHealthBar : SGProgressBar
{
	public GameUnit unit;
	public Color[] colors;

	protected override void OnUpdate ()
	{
		if (unit == null && Player.unit != null) unit = Player.unit;
		factor = (unit != null) ? unit.healthPercent : 0f;
	}

	protected override void OnChange ()
	{
		if (colors == null || colors.Length == 0) return;

		Color c;

		if (colors.Length == 1 || factor <= 0f)
		{
			c = colors[0];
		}
		else if (factor >= 1f)
		{
			c = colors[colors.Length - 1];
		}
		else
		{
			float val = (colors.Length - 1) * factor;
			int i = Mathf.FloorToInt(val);
			c = Color.Lerp(colors[i], colors[i + 1], val - i);
		}

		// Keep the alpha
		c.a = mSprite.color.a;
		mSprite.color = c;
	}
}