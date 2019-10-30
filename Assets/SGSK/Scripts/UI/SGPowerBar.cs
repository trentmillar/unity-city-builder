using UnityEngine;

/// <summary>
/// Extends the Progress Bar's functionality to add a color gradient based on the generator's power reserve.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("UI/Power Bar")]
[RequireComponent(typeof(SGSprite))]
public class SGPowerBar : SGProgressBar
{
	public PowerGenerator generator;
	public Color[] colors;

	protected override void OnUpdate ()
	{
		if (generator == null && Player.shield != null) generator = Player.shield.generator;
		factor = (generator != null) ? generator.powerPercent : 0f;
	}

	protected override void OnChange ()
	{
		if (generator == null || colors == null || colors.Length == 0) return;

		// Color is affected 65% by the generator's current reserve, and 35% by its effectiveness
		float power = Mathf.Lerp(generator.effectiveness, factor, 0.65f);
		
		Color c;
		
		if (colors.Length == 1 || power <= 0f)
		{
			c = colors[0];
		}
		else if (power >= 1f)
		{
			c = colors[colors.Length - 1];
		}
		else
		{
			float val = (colors.Length - 1) * power;
			int i = Mathf.FloorToInt(val);
			c = Color.Lerp(colors[i], colors[i + 1], val - i);
		}
		
		// Keep the alpha
		c.a = mSprite.color.a;
		mSprite.color = c;
	}
}