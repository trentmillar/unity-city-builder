using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
[AddComponentMenu("Game/Delayed Text Input")]
public class DelayedTextInput : MonoBehaviour
{
	public float delayPerCharacter = 0.04f;
	public float delayPerParagraph = 0.2f;
	public string[] textLines;

	TextMesh mMesh;

	void Start ()
	{
		mMesh = GetComponent<TextMesh>();
		StartCoroutine(DelayedInput());
	}

	IEnumerator DelayedInput ()
	{
		string text = "";

		foreach (string s in textLines)
		{
			if (s.Length > 0)
			{
				for (int i = 0; i < s.Length; ++i)
				{
					text += s[i];
					mMesh.text = text;
					if (delayPerCharacter > 0f) yield return new WaitForSeconds(delayPerCharacter);
				}
			}
			
			// End of line
			text += "\n";
			mMesh.text = text;

			// End of paragraph -- wait a little
			if (s.Length == 0 && delayPerParagraph > 0f) yield return new WaitForSeconds(delayPerParagraph);
		}
		Destroy(this);
	}
}