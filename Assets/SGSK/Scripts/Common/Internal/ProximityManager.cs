using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Want to receive once-per-rigidbody OnTriggerEnter/Exit events instead of per-collider?
/// How about calling of a custom Update function while there is a rigidbody inside?
/// That's what this manager is for.
/// </summary>

[AddComponentMenu("Internal/Proximity Manager")]
public class ProximityManager : MonoBehaviour
{
	public delegate bool OnTriggerCallback (Rigidbody rb);
	public delegate bool OnUpdateCallback (List<Entry> list);

	public class Entry
	{
		public Rigidbody rb;
		public int counter;
	}

	class TriggerListener
	{
		public Component mb;
		public OnTriggerCallback callback;
	}

	class UpdateListener
	{
		public Component mb;
		public OnUpdateCallback callback;
	}

	List<Entry>				mList		= new List<Entry>();
	List<TriggerListener>	mOnEnter	= new List<TriggerListener>();
	List<TriggerListener>	mOnExit		= new List<TriggerListener>();
	List<UpdateListener>	mOnUpdate	= new List<UpdateListener>();

	/// <summary>
	/// List of all rigidbodies currently inside the collider.
	/// </summary>

	public List<Entry> rigidbodiesInside { get { return mList; } }

	/// <summary>
	/// Helper function that ensures there is an instance of ProximityTrigger alongside the specified component.
	/// </summary>

	static ProximityManager GetInstance (Component mb)
	{
		ProximityManager pro = mb.GetComponent<ProximityManager>();
		if (pro == null) pro = mb.gameObject.AddComponent<ProximityManager>();
		return pro;
	}

	/// <summary>
	/// Add a new enter event listener.
	/// </summary>

	static public void AddOnEnter (Component mb, OnTriggerCallback callback)
	{
		ProximityManager pro = GetInstance(mb);
		foreach (TriggerListener p in pro.mOnEnter) if (p.mb == mb && p.callback == callback) return;

		TriggerListener ent = new TriggerListener();
		ent.mb = mb;
		ent.callback = callback;
		pro.mOnEnter.Add(ent);
	}

	/// <summary>
	/// Add a new exit event listener.
	/// </summary>

	static public void AddOnExit (Component mb, OnTriggerCallback callback)
	{
		ProximityManager pro = GetInstance(mb);
		foreach (TriggerListener p in pro.mOnExit) if (p.mb == mb && p.callback == callback) return;

		TriggerListener ent = new TriggerListener();
		ent.mb = mb;
		ent.callback = callback;
		pro.mOnExit.Add(ent);
	}

	/// <summary>
	/// Add a new fixed update listener.
	/// </summary>

	static public void AddOnUpdate (Component mb, OnUpdateCallback callback)
	{
		ProximityManager pro = GetInstance(mb);
		foreach (UpdateListener p in pro.mOnUpdate) if (p.mb == mb && p.callback == callback) return;

		UpdateListener ent = new UpdateListener();
		ent.mb = mb;
		ent.callback = callback;
		pro.mOnUpdate.Add(ent);
	}

	void OnTriggerEnter (Collider col)
	{
		Rigidbody rb = Tools.FindInParents<Rigidbody>(col.transform);

		if (rb != null && !rb.isKinematic)
		{
			foreach (Entry ent in mList)
			{
				if (ent.rb == rb)
				{
					++ent.counter;
					return;
				}
			}

			Entry e = new Entry();
			e.rb = rb;
			e.counter = 1;
			mList.Add(e);

			for (int i = mOnEnter.Count; i > 0; )
			{
				TriggerListener listener = mOnEnter[--i];
				if (listener.mb == null || !listener.callback(rb)) mOnEnter.RemoveAt(i);
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		Rigidbody rb = Tools.FindInParents<Rigidbody>(col.transform);

		if (rb != null && !rb.isKinematic)
		{
			foreach (Entry ent in mList)
			{
				if (ent.rb == rb)
				{
					if (--ent.counter <= 0)
					{
						mList.Remove(ent);

						for (int i = mOnExit.Count; i > 0; )
						{
							TriggerListener listener = mOnExit[--i];
							if (listener.mb == null || !listener.callback(rb)) mOnExit.RemoveAt(i);
						}
					}
					return;
				}
			}
		}
	}

	void FixedUpdate()
	{
		for (int i = mList.Count; i > 0; )
		{
			Entry ent = mList[--i];

			if (ent.rb == null)
			{
				mList.RemoveAt(i);
			}
		}

		for (int i = mOnUpdate.Count; i > 0; )
		{
			UpdateListener listener = mOnUpdate[--i];
			if (listener.mb == null || !listener.callback(mList)) mOnUpdate.RemoveAt(i);
		}
	}
}