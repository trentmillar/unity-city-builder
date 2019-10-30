using UnityEngine;
using System.Collections;

/**
 * Button which speeds up a building or activity.
 */ 
public class SpeedUpButton : MonoBehaviour {

	public void OnClick() {
		BuildingManager.GetInstance().SpeedUp();	
		UIGamePanel.ShowPanel (PanelType.DEFAULT);
	}
}
