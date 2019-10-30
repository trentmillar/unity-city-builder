using UnityEngine;

public abstract class DamageReduction : MonoBehaviour
{
	// Components get sorted and absorb from highest to lowest
	public int layer = 0;

	/// <summary>
	/// Attempt to absorb the specified amount of damage.
	/// Return the amount of damage that has not been absorbed.
	/// </summary>

	public abstract float OnAbsorbDamage (float damage);
}