using UnityEngine;
using System.Collections;

[AddComponentMenu("UI/Flicker")]
[RequireComponent(typeof(SGWidget))]
public class SGFlicker : MonoBehaviour
{
	public float interval = 0.01f;
	public Vector2 alphaRange = new Vector2(0.9f, 1f);

	SGWidget mWidget;

	void Start ()
	{
		mWidget = GetComponent<SGWidget>();
		StartCoroutine(Flicker());
	}

	IEnumerator Flicker ()
	{
		for (; ; )
		{
			if (enabled) mWidget.color.a = Random.Range(alphaRange.x, alphaRange.y);
			yield return new WaitForSeconds(interval);
		}
	}
}