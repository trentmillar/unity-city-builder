using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Heal While Inside")]
public class HealWhileInside : MonoBehaviour
{
	// Rate at which units get healed -- hit points per second
	public float rate = 15f;

	float mNextUpdate = 0f;
	GameFaction mFaction;

	void Start ()
	{
		mFaction = Tools.FindInParents<GameFaction>(transform);
		ProximityManager.AddOnUpdate(this, OnUpdate);
	}

	bool OnUpdate (List<ProximityManager.Entry> list)
	{
		if (Time.time > mNextUpdate)
		{
			mNextUpdate = Time.time + 0.5f;

			foreach (ProximityManager.Entry ent in list)
			{
				GameUnit gu = ent.rb.GetComponent<GameUnit>();
				
				if (gu != null)
				{
					if (mFaction == null || mFaction.factionID == gu.factionID)
					{
						gu.ApplyDamage(-rate * 0.5f);
					}
				}
			}
		}
		return true;
	}
}