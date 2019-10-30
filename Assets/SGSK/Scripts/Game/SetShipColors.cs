using UnityEngine;

[AddComponentMenu("Game/Set Ship Colors")]
public class SetShipColors : MonoBehaviour
{
	public Color[] thrusters = new Color[5];
	public Color lightColor = Color.white;
	public float intensity = 1f;

	public void Start ()
	{
		ParticleAnimator[] animators = GetComponentsInChildren<ParticleAnimator>(true);
		foreach (ParticleAnimator p in animators) p.colorAnimation = thrusters;

		Light[] lights = GetComponentsInChildren<Light>(true);
		foreach (Light l in lights) { l.color = lightColor; l.intensity = intensity; }
	}
}