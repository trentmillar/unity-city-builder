using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper class that allows to queue work on the main thread, executing one function per update.
/// </summary>

[AddComponentMenu("Internal/Work Queue Manager")]
public class WorkQueue : MonoBehaviour
{
	// Return 'true' when done, 'false' if need more time
	public delegate bool DoWorkCallback();

	class Entry
	{
		public MonoBehaviour mb;
		public DoWorkCallback callback;
	}

	static WorkQueue mInstance = null;
	List<Entry> mList = new List<Entry>();

	/// <summary>
	/// Queue up a new callback to be executed on Update after others finish.
	/// The callback should return 'true' if it's done, or 'false' if it needs more time.
	/// </summary>

	static public void Add (MonoBehaviour mb, DoWorkCallback callback)
	{
		if (mb == null) return;

		if (mInstance == null)
		{
			GameObject go = new GameObject("_WorkQueue");
			mInstance = go.AddComponent<WorkQueue>();
			DontDestroyOnLoad(go);
		}

		Entry ent = new Entry();
		ent.mb = mb;
		ent.callback = callback;
		mInstance.mList.Add(ent);
	}

	/// <summary>
	/// Execute callbacks one at a time.
	/// </summary>

	void Update()
	{
		if (mList.Count > 0)
		{
			Entry ent = mList[0];

			if (ent.mb == null || ent.callback())
			{
				mList.RemoveAt(0);
				return;
			}
		}
	}

	void OnDestroy()
	{
		mInstance = null;
	}
}