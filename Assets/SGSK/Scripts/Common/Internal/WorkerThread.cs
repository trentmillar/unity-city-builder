using UnityEngine;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// This class creates a worker thread that runs in the background and accepts scheduled work.
/// </summary>

[AddComponentMenu("Internal/Worker Thread Manager")]
public class WorkerThread : MonoBehaviour
{
	static WorkerThread mInstance = null;
	
	// Callback function prototype -- should return whether to keep it in the thread pool
	public delegate bool CallbackFunction (object param);
	
	// Worker thread function and its accompanying parameter
	class Entry
	{
		public CallbackFunction fnct;
		public object param;
	};
	
	// Actual worker thread
	Thread mThread = null;
	
	// List of callbacks executed in order by the worker thread
	List<Entry> mActive = new List<Entry>();
	
	// How long the worker thread will sleep after executing all functions
	public int sleepMilliseconds = 10;
	
	/// <summary>
	/// Abort the worker thread on application quit.
	/// </summary>
	
	void OnApplicationQuit()
	{
		if (mThread != null)
		{
			mThread.Abort();
			while (mThread.IsAlive) Thread.Sleep(1);
			mThread = null;
		}
		ClearQueues();
		mInstance = null;
	}
	
	/// <summary>
	/// Manually clear both worker queues.
	/// </summary>
	
	void ClearQueues()
	{
		foreach (Entry ent in mActive)
		{
			if (ent != null)
			{
				ent.param = null;
				ent.fnct = null;
			}
		}
		mActive.Clear();
	}
	
	/// <summary>
	/// Worker thread function.
	/// </summary>
	
	void ThreadFunction()
	{
		for (;;)
		{
			if (mActive.Count > 0)
			{
				lock (mInstance)
				{
					for (int i = mActive.Count; i > 0; )
					{
						Entry ent = mActive[--i];
						if (!ent.fnct(ent.param)) mActive.RemoveAt(i);
					}
				}
			}
			Thread.Sleep(sleepMilliseconds);
		}
	}
	
	/// <summary>
	/// Add a new callback function to the worker thread.
	/// </summary>
	
	static public void Add (CallbackFunction fn, object param)
	{
		if (mInstance == null)
		{
			GameObject go = new GameObject("_WorkerThread");
			DontDestroyOnLoad(go);
			mInstance = go.AddComponent<WorkerThread>();
			mInstance.mThread = new Thread(mInstance.ThreadFunction);
			mInstance.mThread.Start();
		}
		
		Entry ent = new Entry();
		ent.fnct = fn;
		ent.param = param;
		lock (mInstance) mInstance.mActive.Add(ent);
	}
	
	/// <summary>
	/// Removes the specified function from the worker thread.
	/// </summary>
	
	static public void Remove (CallbackFunction fn)
	{
		if (mInstance != null)
		{
			lock (mInstance)
			{
				foreach (Entry ent in mInstance.mActive)
				{
					if (ent.fnct == fn)
					{
						mInstance.mActive.Remove(ent);
						break;
					}
				}
			}
		}
	}
}