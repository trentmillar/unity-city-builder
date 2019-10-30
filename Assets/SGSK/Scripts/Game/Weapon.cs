using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	public enum Group
	{
		Primary,
		Secondary,
	}

	public GameObject prefab;
	public Group group = Group.Primary;

	FiredObject mFo;

	/// <summary>
	/// Get the expected component on the prefab.
	/// </summary>

	public FiredObject firedObject
	{
		get
		{
			if (mFo == null)
			{
				mFo = prefab.GetComponent<FiredObject>();

				if (mFo == null)
				{
					Debug.LogError("Weapon's prefab must have a " + typeof(FiredObject) + " script attached to it");
				}
			}
			return mFo;
		}
	}

	/// <summary>
	/// Whether the weapon is ready to fire.
	/// </summary>

	public abstract bool canFire { get; }

	/// <summary>
	/// Fire the weapon.
	/// </summary>

	public abstract void Fire ();
}