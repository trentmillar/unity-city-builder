using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Unit")]
[RequireComponent(typeof(NetworkView))]
public class GameUnit : GameFaction
{
	public delegate float OnDamageCallback (float damage);

	public float currentHealth = 100f;
	public float maxHealth = 100f;
	public GameObject explosionPrefab;

	List<DamageReduction> mProtection = new List<DamageReduction>();

	/// <summary>
	/// Unit's current health in percent.
	/// </summary>

	public float healthPercent { get { return Mathf.Clamp01(currentHealth / maxHealth); } }

	/// <summary>
	/// List sorting function.
	/// </summary>

	static int SortProtection (DamageReduction a, DamageReduction b)
	{
		if (a.layer < b.layer) return -1;
		if (a.layer > b.layer) return 1;
		return 0;
	}

	/// <summary>
	/// Find all damage protection and sort it.
	/// </summary>

	protected override void OnStart ()
	{
		DamageReduction[] prots = GetComponentsInChildren<DamageReduction>();
		foreach (DamageReduction pro in prots) mProtection.Add(pro);
		mProtection.Sort(SortProtection);
	}

	/// <summary>
	/// Apply damage to the unit.
	/// </summary>

	public void ApplyDamage (float damage)
	{
		// Negative damage means healing, and there is no point in healing if the unit has full health.
		if (damage < 0f && currentHealth == maxHealth) return;

		if (!NetworkManager.isConnected || mView.isMine)
		{
			ApplyDamageRPC(damage);
		}
		else if (!NetworkManager.HasBeenDestroyed(mView))
		{
			mView.RPC("ApplyDamageRPC", mView.owner, damage);
		}
	}

	/// <summary>
	/// RPC callback applying damage to the unit. Sent only to the unit's owner.
	/// </summary>

	[RPC] void ApplyDamageRPC (float damage)
	{
		if (currentHealth == 0f) return;

		// Decrease the damage using all available protection
		if (damage > 0f)
		{
			for (int i = mProtection.Count; i > 0; )
			{
				DamageReduction prot = mProtection[--i];

				if (prot == null)
				{
					mProtection.RemoveAt(i);
				}
				else
				{
					damage = prot.OnAbsorbDamage(damage);
					if (damage == 0f) break;
				}
			}
		}

		// Decrease the hull's health
		currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);

		if (currentHealth > 0f)
		{
			// Inform others of the unit's current health
			if (NetworkManager.isMultiplayer)
			{
				mView.RPC("SetHealthRPC", RPCMode.Others, currentHealth, maxHealth);
			}
		}
		else // Once the health drops below zero, destroy the unit
		{
			// Instantiate an explosion prefab
			if (explosionPrefab != null)
			{
				NetworkManager.RemoteInstantiate(explosionPrefab, transform.position,
					Quaternion.identity, NetworkManager.gameChannel);
			}

			// Destroy this unit
			NetworkManager.RemoteDestroy(gameObject);
		}
	}

	/// <summary>
	/// RPC callback setting the unit's current health.
	/// </summary>

	[RPC] void SetHealthRPC (float current, float max)
	{
		currentHealth = current;
		maxHealth = max;
	}
}