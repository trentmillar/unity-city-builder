using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIButtonSwitchPanel : MonoBehaviour {
	
	public PanelType type;

	public void OnClick() {
		UIGamePanel.ShowPanel (type);
	}
}
