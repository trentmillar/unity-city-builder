using UnityEngine;
using System.Collections;

/// <summary>
/// Helper utility that allows creation of Co-Routines outside of MonoBehaviour-derived classes.
/// </summary>

[AddComponentMenu("Internal/Coroutine Manager")]
public class CoroutineManager : MonoBehaviour
{
	public int activeCoroutines = 0;
	
	static CoroutineManager mInstance = null;
	
	/// <summary>
	/// Add a new coroutine to a stand-alone game object.
	/// </summary>
	
	public static void Add (IEnumerator en)
	{
		if (mInstance == null)
		{
			GameObject go = new GameObject("_CoroutineManager");
			mInstance = go.AddComponent<CoroutineManager>();
			DontDestroyOnLoad(mInstance);
		}
		mInstance.StartCoroutine(en);
	}
	
	void OnApplicationQuit() { mInstance = null; }
}