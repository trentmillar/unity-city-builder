using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TODO: Get rid of this class. It can be replaced with UISetState or just SendMessage() instead.
/// </summary>

public abstract class EventListener : MonoBehaviour
{
	public string eventName;

	static List<EventListener> mList = new List<EventListener>();

	void Awake ()
	{
		mList.Add(this);
	}

	void OnDestroy ()
	{
		mList.Remove(this);
	}

	abstract protected void Trigger ();

	static public void Trigger (string name)
	{
		foreach (EventListener listener in mList)
		{
			if (listener != null && string.Equals(name, listener.eventName, System.StringComparison.OrdinalIgnoreCase))
			{
				listener.Trigger();
			}
		}
	}
}