using UnityEngine;
using System.Collections;

/**
 * Button to show the recruit panel
 */ 
public class RecruitButton : MonoBehaviour {

	public void OnClick() {
		UIGamePanel.ShowPanel (PanelType.RECRUIT_OCCUPANTS);
		// if (UIGamePanel.activePanel is UIOccupantSelect) ((UIOccupantSelect)UIGamePanel.activePanel).InitialiseWithBuilding(BuildingManager.ActiveBuilding);
	}
}
