using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(SGWidget))]
[AddComponentMenu("UI/Button Highlight")]
public class SGButtonHighlight : MonoBehaviour
{
	public Color highlighted = new Color(0.5f, 1f, 0.5f, 1f);
	public Color pressed = new Color(1f, 0.5f, 0.5f, 1f);

	Color mNormal = Color.white;
	bool mHover = false;
	bool mPressed = false;
	SGWidget mWidget;

	void Start ()
	{
		mWidget = GetComponent<SGWidget>();
		mNormal = mWidget.color;
	}

	void OnHover (bool isOver)
	{
		mHover = isOver;
	}

	void OnPress (bool pressed)
	{
		mPressed = pressed;
	}

	void Update ()
	{
		Color c = mNormal;
		if (mPressed) c = pressed;
		else if (mHover) c = highlighted;
		mWidget.color = Color.Lerp(mWidget.color, c, Mathf.Clamp01(Time.deltaTime * 5f));
	}
}